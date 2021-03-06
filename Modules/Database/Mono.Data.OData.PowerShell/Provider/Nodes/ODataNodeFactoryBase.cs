/*
	Copyright (c) 2012 Code Owls LLC

	Permission is hereby granted, free of charge, to any person obtaining a copy 
	of this software and associated documentation files (the "Software"), to 
	deal in the Software without restriction, including without limitation the 
	rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
	sell copies of the Software, and to permit persons to whom the Software is 
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in 
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
	FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
	IN THE SOFTWARE. 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Mono.Data.PowerShell.Provider;
using Mono.Data.PowerShell.Provider.PathNodes;
using Mono.Data.PowerShell.Provider.PathNodeProcessors;

namespace Mono.Data.OData.Provider
{
    public abstract class ODataNodeFactoryBase : NodeFactoryBase
    {
        protected readonly Uri _uri;
        protected readonly XDocument _metadata;
        protected readonly XElement _element;
        private readonly string _typeName;
        private string _name;

        protected ODataNodeFactoryBase(Uri uri, XDocument metadata, XElement element, string typeName)
        {
            _uri = uri;
            _metadata = metadata;
            _element = element;
            _typeName = typeName;
        }

        protected virtual string Href
        {
            get
            {
                return _element.Attribute(Names.HrefAttribute).Value;
            }
        }

        protected Uri BaseUri
        {
            get
            {
                return new Uri(_element.AncestorsAndSelf().Attributes(Names.XmlBase).First().Value);
            }
        }

        protected virtual Uri DocumentUri
        {
            get
            {
                return new Uri( BaseUri, "./" + Href);
            }
        }

        protected XDocument GetODataDocument()
        {
            return GetODataDocument(DocumentUri);
        }

        protected XDocument GetODataDocument( Uri uri )
        {
            return XDocumentManager.Get(uri);
        }

        string GetQueryStringForSelectedPropertyNames( IEnumerable<string> selectPropertyNames )
        {
            if( null == selectPropertyNames || ! selectPropertyNames.Any())
            {
                return String.Empty;
            }

            var escapedPropertyNames = from s in selectPropertyNames
                        select Uri.EscapeDataString(s);
            return String.Format("$select={0}", String.Join(",", escapedPropertyNames.ToArray()));
        }
        
        protected XDocument GetODataDocument(IContext context)
        {
            XDocument document = null;
            Uri uri = DocumentUri;
            string queryString = null;
            
            var dp = context.DynamicParameters as ODataDynamicParameters;
            if( null != dp )
            {
                queryString = GetQueryString(dp, context.Filter);
            }
            else
            {
                queryString = GetQueryStringForFilter(context.Filter);
            }

            if( String.IsNullOrEmpty( queryString ) )
            {
                return GetODataDocument(uri);
            }

            return GetODataDocument( new Uri( uri, "?" + queryString));
        }

        private string GetQueryString(ODataDynamicParameters dp, string filter)
        {
            var parts = from part in new[]
                                         {
                                             GetQueryStringForFilter(filter),
                                             GetQueryStringForTop(dp.Top),
                                             GetQueryStringForOrderBy(dp.OrderBy, dp.Descending),
                                             GetQueryStringForExpand(dp.Expand),
                                             GetQueryStringForSelect(dp.Select),
                                             GetQueryStringForSkip(dp.Skip)
                                         }
                        where !String.IsNullOrEmpty(part)
                        select part;
            if( ! parts.Any())
            {
                return null;
            }
            return String.Join("&", parts.ToArray());
        }

        private string GetQueryStringForSelect(string[] @select)
        {
            if (null == select)
            {
                return null;
            }
            var items = from s in @select select Uri.EscapeDataString(s);
            return String.Format("$select={0}", String.Join(",", items.ToArray()));
        }
        
        private string GetQueryStringForExpand(string expand)
        {
            if (String.IsNullOrEmpty(expand))
            {
                return null;
            }
            return String.Format("$expand={0}", Uri.EscapeDataString(expand));
        }

        private string GetQueryStringForOrderBy(string orderBy, SwitchParameter @descending)
        {
            if( String.IsNullOrEmpty( orderBy))
            {
                return null;
            }

            return String.Format("$orderby={0} {1}", Uri.EscapeDataString(orderBy), @descending.IsPresent ? "desc" : "asc");

        }

        private string GetQueryStringForTop(int top)
        {
            if( 0 == top )
            {
                return null;
            }

            return String.Format("$top={0}", top);
        }

        private string GetQueryStringForSkip(int skip)
        {
            if (0 == skip)
            {
                return null;
            }

            return String.Format("$skip={0}", skip);
        }

        private string GetQueryStringForFilter(string filter)
        {
            if( String.IsNullOrEmpty( filter ) || filter.Contains("$") )
            {
                return filter;
            }

            return String.Format("$filter={0}", Uri.EscapeDataString(filter));
        }

        public override IPathNode GetNodeValue()
        {
            var pso = PSObject.AsPSObject(new {Uri = DocumentUri, Name = Name, XElement = _element});

            var properties = _element.Descendants(Names.Properties);
            if( properties.Any())
            {
                var props = ( from prop in properties.Descendants()
                            select new PSNoteProperty(prop.Name.LocalName, GetPropertyValueAsPSObject( prop )) ).ToList();

                props.ToList().ForEach( pso.Properties.SafeAdd );
            }

            var typeName = GetEntityTypeName();

            pso.TypeNames.Clear();
            pso.TypeNames.Add( typeName );
            
            return new PathNode( pso, Name, true);
        }

        private PSObject GetPropertyValueAsPSObject(XElement prop)
        {
            var typeAttribute = prop.Attribute(Names.PropertyTypeAttribute);
            if( null == typeAttribute || typeAttribute.Value.StartsWith("Edm."))
            {
                return PSObject.AsPSObject( prop.Value );
            }
            var value = new PSObject();
            var valueProperties =
                (from item in prop.Descendants()
                 select new PSNoteProperty(item.Name.LocalName, GetPropertyValueAsPSObject(item))).ToList();
            valueProperties.ForEach( value.Properties.SafeAdd );
            return value;
        }

        string GetEntityTypeName()
        {
            var typeName = GetType().Namespace + "." + _typeName;
			/*
            var category = GetCategoryTerm();
            if (null != category)
            {
                typeName = category.Value;
            }
            */
            return typeName;
        }

        private XAttribute GetCategoryTerm()
        {
            var category = _element.Descendants(Names.Category).Attributes(Names.TermAttribute).FirstOrDefault();
            return category;
        }

        public override string Name
        {
            get
            {
                if( null == _name )
                {                 
                    var node = _element.Descendants(Names.Title).FirstOrDefault();
                                       
                    if( null == node )
                    {
                        _name = _element.Attribute(Names.TitleAttribute).Value;
                    }
                    else
                    {
                        _name = node.Value;
                    }                    
                }

                return _name;
            }
        }
    }
}
