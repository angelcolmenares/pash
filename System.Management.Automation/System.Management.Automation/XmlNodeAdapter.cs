namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Xml;

    internal class XmlNodeAdapter : PropertyOnlyAdapter
    {
        protected override void DoAddAllProperties<T>(object obj, PSMemberInfoInternalCollection<T> members)
        {
            System.Xml.XmlNode node = (System.Xml.XmlNode) obj;
            Dictionary<string, List<System.Xml.XmlNode>> dictionary = new Dictionary<string, List<System.Xml.XmlNode>>(StringComparer.OrdinalIgnoreCase);
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlNode node2 in node.Attributes)
                {
                    List<System.Xml.XmlNode> list;
                    if (!dictionary.TryGetValue(node2.LocalName, out list))
                    {
                        list = new List<System.Xml.XmlNode>();
                        dictionary[node2.LocalName] = list;
                    }
                    list.Add(node2);
                }
            }
            if (node.ChildNodes != null)
            {
                foreach (System.Xml.XmlNode node3 in node.ChildNodes)
                {
                    if (!(node3 is XmlWhitespace))
                    {
                        List<System.Xml.XmlNode> list2;
                        if (!dictionary.TryGetValue(node3.LocalName, out list2))
                        {
                            list2 = new List<System.Xml.XmlNode>();
                            dictionary[node3.LocalName] = list2;
                        }
                        list2.Add(node3);
                    }
                }
            }
            foreach (KeyValuePair<string, List<System.Xml.XmlNode>> pair in dictionary)
            {
                members.Add(new PSProperty(pair.Key, this, obj, pair.Value.ToArray()) as T);
            }
        }

        protected override PSProperty DoGetProperty(object obj, string propertyName)
        {
            System.Xml.XmlNode[] adapterData = FindNodes(obj, propertyName, StringComparison.OrdinalIgnoreCase);
            if (adapterData.Length == 0)
            {
                return null;
            }
            return new PSProperty(adapterData[0].LocalName, this, obj, adapterData);
        }

        private static System.Xml.XmlNode[] FindNodes(object obj, string propertyName, StringComparison comparisonType)
        {
            List<System.Xml.XmlNode> list = new List<System.Xml.XmlNode>();
            System.Xml.XmlNode node = (System.Xml.XmlNode) obj;
            if (node.Attributes != null)
            {
                foreach (System.Xml.XmlNode node2 in node.Attributes)
                {
                    if (node2.LocalName.Equals(propertyName, comparisonType))
                    {
                        list.Add(node2);
                    }
                }
            }
            if (node.ChildNodes != null)
            {
                foreach (System.Xml.XmlNode node3 in node.ChildNodes)
                {
                    if (!(node3 is XmlWhitespace) && node3.LocalName.Equals(propertyName, comparisonType))
                    {
                        list.Add(node3);
                    }
                }
            }
            return list.ToArray();
        }

        private static object GetNodeObject(System.Xml.XmlNode node)
        {
            XmlText text = node as XmlText;
            if (text != null)
            {
                return text.InnerText;
            }
            XmlAttributeCollection attributes = node.Attributes;
            if ((attributes == null) || (attributes.Count == 0))
            {
                if (!node.HasChildNodes)
                {
                    return node.InnerText;
                }
                XmlNodeList childNodes = node.ChildNodes;
                if ((childNodes.Count == 1) && (childNodes[0].NodeType == XmlNodeType.Text))
                {
                    return node.InnerText;
                }
                System.Xml.XmlAttribute attribute = node as System.Xml.XmlAttribute;
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }
            return node;
        }

        protected override IEnumerable<string> GetTypeNameHierarchy(object obj)
        {
            System.Xml.XmlNode iteratorVariable0 = (System.Xml.XmlNode) obj;
            string namespaceURI = iteratorVariable0.NamespaceURI;
            IEnumerable<string> dotNetTypeNameHierarchy = Adapter.GetDotNetTypeNameHierarchy(obj);
            if (string.IsNullOrEmpty(namespaceURI))
            {
                foreach (string iteratorVariable3 in dotNetTypeNameHierarchy)
                {
                    yield return iteratorVariable3;
                }
            }
            else
            {
                StringBuilder iteratorVariable4 = null;
                foreach (string iteratorVariable5 in dotNetTypeNameHierarchy)
                {
                    if (iteratorVariable4 == null)
                    {
                        iteratorVariable4 = new StringBuilder(iteratorVariable5);
                        iteratorVariable4.Append("#");
                        iteratorVariable4.Append(iteratorVariable0.NamespaceURI);
                        iteratorVariable4.Append("#");
                        iteratorVariable4.Append(iteratorVariable0.LocalName);
                        yield return iteratorVariable4.ToString();
                    }
                    yield return iteratorVariable5;
                }
            }
        }

        protected override object PropertyGet(PSProperty property)
        {
            System.Xml.XmlNode[] adapterData = (System.Xml.XmlNode[]) property.adapterData;
            if (adapterData.Length == 1)
            {
                return GetNodeObject(adapterData[0]);
            }
            object[] objArray = new object[adapterData.Length];
            for (int i = 0; i < adapterData.Length; i++)
            {
                objArray[i] = GetNodeObject(adapterData[i]);
            }
            return objArray;
        }

        protected override bool PropertyIsGettable(PSProperty property)
        {
            return true;
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            System.Xml.XmlNode[] adapterData = (System.Xml.XmlNode[]) property.adapterData;
            if (adapterData.Length != 1)
            {
                return false;
            }
            System.Xml.XmlNode node = adapterData[0];
            if (node is XmlText)
            {
                return true;
            }
            if (node is System.Xml.XmlAttribute)
            {
                return true;
            }
            XmlAttributeCollection attributes = node.Attributes;
            if ((attributes != null) && (attributes.Count != 0))
            {
                return false;
            }
            XmlNodeList childNodes = node.ChildNodes;
            return (((childNodes == null) || (childNodes.Count == 0)) || ((childNodes.Count == 1) && (childNodes[0].NodeType == XmlNodeType.Text)));
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            string str = setValue as string;
            if (str == null)
            {
                throw new SetValueException("XmlNodeSetShouldBeAString", null, ExtendedTypeSystem.XmlNodeSetShouldBeAString, new object[] { property.Name });
            }
            System.Xml.XmlNode[] adapterData = (System.Xml.XmlNode[]) property.adapterData;
            if (adapterData.Length > 1)
            {
                throw new SetValueException("XmlNodeSetRestrictionsMoreThanOneNode", null, ExtendedTypeSystem.XmlNodeSetShouldBeAString, new object[] { property.Name });
            }
            System.Xml.XmlNode node = adapterData[0];
            XmlText text = node as XmlText;
            if (text != null)
            {
                text.InnerText = str;
            }
            else
            {
                XmlAttributeCollection attributes = node.Attributes;
                if ((attributes != null) && (attributes.Count != 0))
                {
                    throw new SetValueException("XmlNodeSetRestrictionsNodeWithAttributes", null, ExtendedTypeSystem.XmlNodeSetShouldBeAString, new object[] { property.Name });
                }
                XmlNodeList childNodes = node.ChildNodes;
                if ((childNodes == null) || (childNodes.Count == 0))
                {
                    node.InnerText = str;
                }
                else if ((childNodes.Count == 1) && (childNodes[0].NodeType == XmlNodeType.Text))
                {
                    node.InnerText = str;
                }
                else
                {
                    System.Xml.XmlAttribute attribute = node as System.Xml.XmlAttribute;
                    if (attribute == null)
                    {
                        throw new SetValueException("XmlNodeSetRestrictionsUnknownNodeType", null, ExtendedTypeSystem.XmlNodeSetShouldBeAString, new object[] { property.Name });
                    }
                    attribute.Value = str;
                }
            }
        }

        protected override string PropertyType(PSProperty property, bool forDisplay)
        {
            object obj2 = null;
            try
            {
                obj2 = base.BasePropertyGet(property);
            }
            catch (GetValueException)
            {
            }
            Type type = (obj2 == null) ? typeof(object) : obj2.GetType();
            if (!forDisplay)
            {
                return type.FullName;
            }
            return ToStringCodeMethods.Type(type, false);
        }

        
    }
}

