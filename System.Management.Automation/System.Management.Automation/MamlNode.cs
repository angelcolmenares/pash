namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    internal class MamlNode
    {
        private Collection<ErrorRecord> _errors = new Collection<ErrorRecord>();
        private System.Management.Automation.PSObject _mshObject;
        private System.Xml.XmlNode _xmlNode;

        internal MamlNode(System.Xml.XmlNode xmlNode)
        {
            this._xmlNode = xmlNode;
        }

        private static void AddProperty(Hashtable properties, string name, System.Management.Automation.PSObject mshObject)
        {
            ArrayList list = (ArrayList) properties[name];
            if (list == null)
            {
                list = new ArrayList();
                properties[name] = list;
            }
            if (mshObject != null)
            {
                if ((mshObject.BaseObject is PSCustomObject) || !mshObject.BaseObject.GetType().Equals(typeof(System.Management.Automation.PSObject[])))
                {
                    list.Add(mshObject);
                }
                else
                {
                    System.Management.Automation.PSObject[] baseObject = (System.Management.Automation.PSObject[]) mshObject.BaseObject;
                    for (int i = 0; i < baseObject.Length; i++)
                    {
                        list.Add(baseObject[i]);
                    }
                }
            }
        }

        private System.Management.Automation.PSObject GetDefinitionListItemPSObject(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode == null)
            {
                return null;
            }
            if (!xmlNode.LocalName.Equals("definitionListItem", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            string str = null;
            string definitionText = null;
            foreach (System.Xml.XmlNode node in xmlNode.ChildNodes)
            {
                if (node.LocalName.Equals("term", StringComparison.OrdinalIgnoreCase))
                {
                    str = node.InnerText.Trim();
                }
                else if (node.LocalName.Equals("definition", StringComparison.OrdinalIgnoreCase))
                {
                    definitionText = this.GetDefinitionText(node);
                }
                else
                {
                    this.WriteMamlInvalidChildNodeError(xmlNode, node);
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            System.Management.Automation.PSObject obj2 = new System.Management.Automation.PSObject();
            obj2.Properties.Add(new PSNoteProperty("Term", str));
            obj2.Properties.Add(new PSNoteProperty("Definition", definitionText));
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add("MamlDefinitionTextItem");
            obj2.TypeNames.Add("MamlTextItem");
            return obj2;
        }

        private ArrayList GetDefinitionListPSObjects(System.Xml.XmlNode xmlNode)
        {
            ArrayList list = new ArrayList();
            if (xmlNode != null)
            {
                if (!xmlNode.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase))
                {
                    return list;
                }
                if ((xmlNode.ChildNodes == null) || (xmlNode.ChildNodes.Count == 0))
                {
                    return list;
                }
                foreach (System.Xml.XmlNode node in xmlNode.ChildNodes)
                {
                    if (node.LocalName.Equals("definitionListItem", StringComparison.OrdinalIgnoreCase))
                    {
                        System.Management.Automation.PSObject definitionListItemPSObject = this.GetDefinitionListItemPSObject(node);
                        if (definitionListItemPSObject != null)
                        {
                            list.Add(definitionListItemPSObject);
                        }
                    }
                    else
                    {
                        this.WriteMamlInvalidChildNodeError(xmlNode, node);
                    }
                }
            }
            return list;
        }

        private string GetDefinitionText(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode == null)
            {
                return null;
            }
            if (!xmlNode.LocalName.Equals("definition", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if ((xmlNode.ChildNodes == null) || (xmlNode.ChildNodes.Count == 0))
            {
                return "";
            }
            if (xmlNode.ChildNodes.Count > 1)
            {
                this.WriteMamlInvalidChildNodeCountError(xmlNode, "para", 1);
            }
            string str = "";
            foreach (System.Xml.XmlNode node in xmlNode.ChildNodes)
            {
                if (node.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
                {
                    str = node.InnerText.Trim();
                }
                else
                {
                    this.WriteMamlInvalidChildNodeError(xmlNode, node);
                }
            }
            return str;
        }

        private static int GetIndentation(string line)
        {
            if (IsEmptyLine(line))
            {
                return 0;
            }
            string str = line.TrimStart(new char[] { ' ' });
            return (line.Length - str.Length);
        }

        private Hashtable GetInsideProperties(System.Xml.XmlNode xmlNode)
        {
            Hashtable properties = new Hashtable(StringComparer.OrdinalIgnoreCase);
            if (xmlNode == null)
            {
                return properties;
            }
            if (xmlNode.ChildNodes != null)
            {
                foreach (System.Xml.XmlNode node in xmlNode.ChildNodes)
                {
                    AddProperty(properties, node.LocalName, this.GetPSObject(node));
                }
            }
            return SimplifyProperties(properties);
        }

        private System.Management.Automation.PSObject GetInsidePSObject(System.Xml.XmlNode xmlNode)
        {
            Hashtable insideProperties = this.GetInsideProperties(xmlNode);
            System.Management.Automation.PSObject obj2 = new System.Management.Automation.PSObject();
            IDictionaryEnumerator enumerator = insideProperties.GetEnumerator();
            while (enumerator.MoveNext())
            {
                obj2.Properties.Add(new PSNoteProperty((string) enumerator.Key, enumerator.Value));
            }
            return obj2;
        }

        private System.Management.Automation.PSObject GetListItemPSObject(System.Xml.XmlNode xmlNode, bool ordered, ref int index)
        {
            if (xmlNode == null)
            {
                return null;
            }
            if (!xmlNode.LocalName.Equals("listItem", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            string str = "";
            if (xmlNode.ChildNodes.Count > 1)
            {
                this.WriteMamlInvalidChildNodeCountError(xmlNode, "para", 1);
            }
            foreach (System.Xml.XmlNode node in xmlNode.ChildNodes)
            {
                if (node.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
                {
                    str = node.InnerText.Trim();
                }
                else
                {
                    this.WriteMamlInvalidChildNodeError(xmlNode, node);
                }
            }
            string str2 = "";
            if (ordered)
            {
                str2 = ((int) index).ToString("d2", CultureInfo.CurrentCulture) + ". ";
                index++;
            }
            else
            {
                str2 = "* ";
            }
            System.Management.Automation.PSObject obj2 = new System.Management.Automation.PSObject();
            obj2.Properties.Add(new PSNoteProperty("Text", str));
            obj2.Properties.Add(new PSNoteProperty("Tag", str2));
            obj2.TypeNames.Clear();
            if (ordered)
            {
                obj2.TypeNames.Add("MamlOrderedListTextItem");
            }
            else
            {
                obj2.TypeNames.Add("MamlUnorderedListTextItem");
            }
            obj2.TypeNames.Add("MamlTextItem");
            return obj2;
        }

        private ArrayList GetListPSObjects(System.Xml.XmlNode xmlNode)
        {
            ArrayList list = new ArrayList();
            if (xmlNode != null)
            {
                if (!xmlNode.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    return list;
                }
                if ((xmlNode.ChildNodes == null) || (xmlNode.ChildNodes.Count == 0))
                {
                    return list;
                }
                bool ordered = IsOrderedList(xmlNode);
                int index = 1;
                foreach (System.Xml.XmlNode node in xmlNode.ChildNodes)
                {
                    if (node.LocalName.Equals("listItem", StringComparison.OrdinalIgnoreCase))
                    {
                        System.Management.Automation.PSObject obj2 = this.GetListItemPSObject(node, ordered, ref index);
                        if (obj2 != null)
                        {
                            list.Add(obj2);
                        }
                    }
                    else
                    {
                        this.WriteMamlInvalidChildNodeError(xmlNode, node);
                    }
                }
            }
            return list;
        }

        private System.Management.Automation.PSObject[] GetMamlFormattingPSObjects(System.Xml.XmlNode xmlNode)
        {
            ArrayList list = new ArrayList();
            int paraMamlNodeCount = this.GetParaMamlNodeCount(xmlNode.ChildNodes);
            int num2 = 0;
            foreach (System.Xml.XmlNode node in xmlNode.ChildNodes)
            {
                if (node.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
                {
                    num2++;
                    System.Management.Automation.PSObject paraPSObject = GetParaPSObject(node, num2 != paraMamlNodeCount);
                    if (paraPSObject != null)
                    {
                        list.Add(paraPSObject);
                    }
                }
                else if (node.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    ArrayList listPSObjects = this.GetListPSObjects(node);
                    for (int i = 0; i < listPSObjects.Count; i++)
                    {
                        list.Add(listPSObjects[i]);
                    }
                }
                else if (node.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase))
                {
                    ArrayList definitionListPSObjects = this.GetDefinitionListPSObjects(node);
                    for (int j = 0; j < definitionListPSObjects.Count; j++)
                    {
                        list.Add(definitionListPSObjects[j]);
                    }
                }
                else
                {
                    this.WriteMamlInvalidChildNodeError(xmlNode, node);
                }
            }
            return (System.Management.Automation.PSObject[]) list.ToArray(typeof(System.Management.Automation.PSObject));
        }

        private static int GetMinIndentation(string[] lines)
        {
            int num = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!IsEmptyLine(lines[i]))
                {
                    int indentation = GetIndentation(lines[i]);
                    if ((num < 0) || (indentation < num))
                    {
                        num = indentation;
                    }
                }
            }
            return num;
        }

        private static string GetNodeIndex(System.Xml.XmlNode xmlNode)
        {
            if ((xmlNode != null) && (xmlNode.ParentNode != null))
            {
                int num = 0;
                int num2 = 0;
                foreach (System.Xml.XmlNode node in xmlNode.ParentNode.ChildNodes)
                {
                    if (node == xmlNode)
                    {
                        num = num2++;
                    }
                    else if (node.LocalName.Equals(xmlNode.LocalName, StringComparison.OrdinalIgnoreCase))
                    {
                        num2++;
                    }
                }
                if (num2 > 1)
                {
                    return ("[" + num.ToString("d", CultureInfo.CurrentCulture) + "]");
                }
            }
            return "";
        }

        private static string GetNodePath(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode == null)
            {
                return "";
            }
            if (xmlNode.ParentNode == null)
            {
                return (@"\" + xmlNode.LocalName);
            }
            return (GetNodePath(xmlNode.ParentNode) + @"\" + xmlNode.LocalName + GetNodeIndex(xmlNode));
        }

        private int GetParaMamlNodeCount(XmlNodeList nodes)
        {
            int num = 0;
            foreach (System.Xml.XmlNode node in nodes)
            {
                if (node.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase) && !node.InnerText.Trim().Equals(string.Empty))
                {
                    num++;
                }
            }
            return num;
        }

        private static System.Management.Automation.PSObject GetParaPSObject(System.Xml.XmlNode xmlNode, bool newLine)
        {
            if (xmlNode == null)
            {
                return null;
            }
            if (!xmlNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            System.Management.Automation.PSObject obj2 = new System.Management.Automation.PSObject();
            StringBuilder builder = new StringBuilder();
            if (newLine && !xmlNode.InnerText.Trim().Equals(string.Empty))
            {
                builder.AppendLine(xmlNode.InnerText.Trim());
            }
            else
            {
                builder.Append(xmlNode.InnerText.Trim());
            }
            obj2.Properties.Add(new PSNoteProperty("Text", builder.ToString()));
            obj2.TypeNames.Clear();
            obj2.TypeNames.Add("MamlParaTextItem");
            obj2.TypeNames.Add("MamlTextItem");
            return obj2;
        }

        private static string GetPreformattedText(string text)
        {
            string[] lines = TrimLines(text.Replace("\t", "    ").Split(new char[] { '\n' }));
            if ((lines == null) || (lines.Length == 0))
            {
                return "";
            }
            int minIndentation = GetMinIndentation(lines);
            string[] strArray3 = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                if (IsEmptyLine(lines[i]))
                {
                    strArray3[i] = lines[i];
                }
                else
                {
                    strArray3[i] = lines[i].Remove(0, minIndentation);
                }
            }
            StringBuilder builder = new StringBuilder();
            for (int j = 0; j < strArray3.Length; j++)
            {
                builder.AppendLine(strArray3[j]);
            }
            return builder.ToString();
        }

        private System.Management.Automation.PSObject GetPSObject(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode == null)
            {
                return new System.Management.Automation.PSObject();
            }
            System.Management.Automation.PSObject obj2 = null;
            if (IsAtomic(xmlNode))
            {
                obj2 = new System.Management.Automation.PSObject(string.Copy(xmlNode.InnerText.Trim()));
            }
            else if (IncludeMamlFormatting(xmlNode))
            {
                obj2 = new System.Management.Automation.PSObject(this.GetMamlFormattingPSObjects(xmlNode));
            }
            else
            {
                obj2 = new System.Management.Automation.PSObject(this.GetInsidePSObject(xmlNode));
                obj2.TypeNames.Clear();
                obj2.TypeNames.Add("MamlCommandHelpInfo#" + xmlNode.LocalName);
            }
            if (xmlNode.Attributes != null)
            {
                foreach (System.Xml.XmlNode node in xmlNode.Attributes)
                {
                    obj2.Properties.Add(new PSNoteProperty(node.Name, node.Value));
                }
            }
            return obj2;
        }

        private static bool IncludeMamlFormatting(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode != null)
            {
                if ((xmlNode.ChildNodes == null) || (xmlNode.ChildNodes.Count == 0))
                {
                    return false;
                }
                foreach (System.Xml.XmlNode node in xmlNode.ChildNodes)
                {
                    if (IsMamlFormattingNode(node))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsAtomic(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode == null)
            {
                return false;
            }
            if (xmlNode.ChildNodes == null)
            {
                return true;
            }
            if (xmlNode.ChildNodes.Count > 1)
            {
                return false;
            }
            return ((xmlNode.ChildNodes.Count == 0) || xmlNode.ChildNodes[0].GetType().Equals(typeof(XmlText)));
        }

        private static bool IsEmptyLine(string line)
        {
            return (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim()));
        }

        private static bool IsMamlFormattingNode(System.Xml.XmlNode xmlNode)
        {
            return (xmlNode.LocalName.Equals("para", StringComparison.OrdinalIgnoreCase) || (xmlNode.LocalName.Equals("list", StringComparison.OrdinalIgnoreCase) || xmlNode.LocalName.Equals("definitionList", StringComparison.OrdinalIgnoreCase)));
        }

        private static bool IsMamlFormattingPSObject(System.Management.Automation.PSObject mshObject)
        {
            Collection<string> typeNames = mshObject.TypeNames;
            return (((typeNames != null) && (typeNames.Count != 0)) && typeNames[typeNames.Count - 1].Equals("MamlTextItem", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsOrderedList(System.Xml.XmlNode xmlNode)
        {
            if (xmlNode != null)
            {
                if ((xmlNode.Attributes == null) || (xmlNode.Attributes.Count == 0))
                {
                    return false;
                }
                foreach (System.Xml.XmlNode node in xmlNode.Attributes)
                {
                    if (node.Name.Equals("class", StringComparison.OrdinalIgnoreCase) && node.Value.Equals("ordered", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void RemoveUnsupportedNodes(System.Xml.XmlNode xmlNode)
        {
            System.Xml.XmlNode firstChild = xmlNode.FirstChild;
            while (firstChild != null)
            {
                if (firstChild.NodeType == XmlNodeType.Comment)
                {
                    System.Xml.XmlNode oldChild = firstChild;
                    firstChild = firstChild.NextSibling;
                    xmlNode.RemoveChild(oldChild);
                }
                else
                {
                    this.RemoveUnsupportedNodes(firstChild);
                    firstChild = firstChild.NextSibling;
                }
            }
        }

        private static Hashtable SimplifyProperties(Hashtable properties)
        {
            if (properties == null)
            {
                return null;
            }
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            IDictionaryEnumerator enumerator = properties.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ArrayList list = (ArrayList) enumerator.Value;
                if ((list != null) && (list.Count != 0))
                {
                    if ((list.Count == 1) && !IsMamlFormattingPSObject((System.Management.Automation.PSObject) list[0]))
                    {
                        System.Management.Automation.PSObject obj2 = (System.Management.Automation.PSObject) list[0];
                        hashtable[enumerator.Key] = obj2;
                    }
                    else
                    {
                        hashtable[enumerator.Key] = list.ToArray(typeof(System.Management.Automation.PSObject));
                    }
                }
            }
            return hashtable;
        }

        private static string[] TrimLines(string[] lines)
        {
            if ((lines == null) || (lines.Length == 0))
            {
                return null;
            }
            int index = 0;
            index = 0;
            while (index < lines.Length)
            {
                if (!IsEmptyLine(lines[index]))
                {
                    break;
                }
                index++;
            }
            int num2 = index;
            if (num2 == lines.Length)
            {
                return null;
            }
            index = lines.Length - 1;
            while (index >= num2)
            {
                if (!IsEmptyLine(lines[index]))
                {
                    break;
                }
                index--;
            }
            int num3 = index;
            string[] strArray = new string[(num3 - num2) + 1];
            for (index = num2; index <= num3; index++)
            {
                strArray[index - num2] = lines[index];
            }
            return strArray;
        }

        private void WriteMamlInvalidChildNodeCountError(System.Xml.XmlNode node, string childNodeName, int count)
        {
            ErrorRecord item = new ErrorRecord(new ParentContainsErrorRecordException("MamlInvalidChildNodeCountError"), "MamlInvalidChildNodeCountError", ErrorCategory.SyntaxError, null) {
                ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "MamlInvalidChildNodeCountError", new object[] { node.LocalName, childNodeName, count, GetNodePath(node) })
            };
            this.Errors.Add(item);
        }

        private void WriteMamlInvalidChildNodeError(System.Xml.XmlNode node, System.Xml.XmlNode childNode)
        {
            ErrorRecord item = new ErrorRecord(new ParentContainsErrorRecordException("MamlInvalidChildNodeError"), "MamlInvalidChildNodeError", ErrorCategory.SyntaxError, null) {
                ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "MamlInvalidChildNodeError", new object[] { node.LocalName, childNode.LocalName, GetNodePath(node) })
            };
            this.Errors.Add(item);
        }

        internal Collection<ErrorRecord> Errors
        {
            get
            {
                return this._errors;
            }
        }

        internal System.Management.Automation.PSObject PSObject
        {
            get
            {
                if (this._mshObject == null)
                {
                    this.RemoveUnsupportedNodes(this._xmlNode);
                    this._mshObject = this.GetPSObject(this._xmlNode);
                }
                return this._mshObject;
            }
        }

        internal System.Xml.XmlNode XmlNode
        {
            get
            {
                return this._xmlNode;
            }
        }
    }
}

