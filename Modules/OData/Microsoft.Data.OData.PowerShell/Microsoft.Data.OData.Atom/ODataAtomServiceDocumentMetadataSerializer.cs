namespace Microsoft.Data.OData.Atom
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ODataAtomServiceDocumentMetadataSerializer : ODataAtomMetadataSerializer
    {
        internal ODataAtomServiceDocumentMetadataSerializer(ODataAtomOutputContext atomOutputContext) : base(atomOutputContext)
        {
        }

        internal void WriteResourceCollectionMetadata(ODataResourceCollectionInfo collection)
        {
            AtomResourceCollectionMetadata annotation = collection.GetAnnotation<AtomResourceCollectionMetadata>();
            AtomTextConstruct textConstruct = null;
            if (annotation != null)
            {
                textConstruct = annotation.Title;
            }
            if (base.UseServerFormatBehavior && (textConstruct.Kind == AtomTextConstructKind.Text))
            {
                base.WriteElementWithTextContent("atom", "title", "http://www.w3.org/2005/Atom", textConstruct.Text);
            }
            else
            {
                base.WriteTextConstruct("atom", "title", "http://www.w3.org/2005/Atom", textConstruct);
            }
            if (annotation != null)
            {
                string accept = annotation.Accept;
                if (accept != null)
                {
                    base.WriteElementWithTextContent(string.Empty, "accept", "http://www.w3.org/2007/app", accept);
                }
                AtomCategoriesMetadata categories = annotation.Categories;
                if (categories != null)
                {
                    base.XmlWriter.WriteStartElement(string.Empty, "categories", "http://www.w3.org/2007/app");
                    Uri href = categories.Href;
                    bool? @fixed = categories.Fixed;
                    string scheme = categories.Scheme;
                    IEnumerable<AtomCategoryMetadata> source = categories.Categories;
                    if (href != null)
                    {
                        if ((@fixed.HasValue || (scheme != null)) || ((source != null) && source.Any<AtomCategoryMetadata>()))
                        {
                            throw new ODataException(Microsoft.Data.OData.Strings.ODataAtomWriterMetadataUtils_CategoriesHrefWithOtherValues);
                        }
                        base.XmlWriter.WriteAttributeString("href", base.UriToUrlAttributeValue(href));
                    }
                    else
                    {
                        if (@fixed.HasValue)
                        {
                            base.XmlWriter.WriteAttributeString("fixed", @fixed.Value ? "yes" : "no");
                        }
                        if (scheme != null)
                        {
                            base.XmlWriter.WriteAttributeString("scheme", scheme);
                        }
                        if (source != null)
                        {
                            foreach (AtomCategoryMetadata metadata3 in source)
                            {
                                base.WriteCategory("atom", metadata3.Term, metadata3.Scheme, metadata3.Label);
                            }
                        }
                    }
                    base.XmlWriter.WriteEndElement();
                }
            }
        }

        internal void WriteWorkspaceMetadata(ODataWorkspace workspace)
        {
            AtomWorkspaceMetadata annotation = workspace.GetAnnotation<AtomWorkspaceMetadata>();
            AtomTextConstruct textConstruct = null;
            if (annotation != null)
            {
                textConstruct = annotation.Title;
            }
            if (textConstruct == null)
            {
                textConstruct = new AtomTextConstruct {
                    Text = "Default"
                };
            }
            if (base.UseServerFormatBehavior && (textConstruct.Kind == AtomTextConstructKind.Text))
            {
                base.WriteElementWithTextContent("atom", "title", "http://www.w3.org/2005/Atom", textConstruct.Text);
            }
            else
            {
                base.WriteTextConstruct("atom", "title", "http://www.w3.org/2005/Atom", textConstruct);
            }
        }
    }
}

