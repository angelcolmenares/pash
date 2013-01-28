namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal class UserDefinedHelpData
    {
        private string _name;
        private readonly Dictionary<string, string> _properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private UserDefinedHelpData()
        {
        }

        internal static UserDefinedHelpData Load(System.Xml.XmlNode dataNode)
        {
            if (dataNode == null)
            {
                return null;
            }
            UserDefinedHelpData data = new UserDefinedHelpData();
            for (int i = 0; i < dataNode.ChildNodes.Count; i++)
            {
                System.Xml.XmlNode node = dataNode.ChildNodes[i];
                if (node.NodeType == XmlNodeType.Element)
                {
                    data.Properties[node.Name] = node.InnerText.Trim();
                }
            }
            if (!data.Properties.ContainsKey("name"))
            {
                return null;
            }
            data._name = data.Properties["name"];
            if (string.IsNullOrEmpty(data.Name))
            {
                return null;
            }
            return data;
        }

        internal string Name
        {
            get
            {
                return this._name;
            }
        }

        internal Dictionary<string, string> Properties
        {
            get
            {
                return this._properties;
            }
        }
    }
}

