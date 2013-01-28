namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Xml;

    internal static class FormatXMLHelper
    {
        private static string _tagConfiguration = "Configuration";
        private static string _tagName = "Name";
        private static string _tagTypeName = "TypeName";
        private static string _tagView = "View";
        private static string _tagViewDefinitions = "ViewDefinitions";
        private static string _tagViewSelectedBy = "ViewSelectedBy";

        internal static void WriteToXML(XmlWriter _writer, IEnumerable<ExtendedTypeDefinition> _typeDefinitions, bool exportScriptBlock)
        {
            _writer.WriteStartElement(_tagConfiguration);
            _writer.WriteStartElement(_tagViewDefinitions);
            Dictionary<Guid, List<ExtendedTypeDefinition>> dictionary = new Dictionary<Guid, List<ExtendedTypeDefinition>>();
            Dictionary<Guid, FormatViewDefinition> dictionary2 = new Dictionary<Guid, FormatViewDefinition>();
            foreach (ExtendedTypeDefinition definition in _typeDefinitions)
            {
                foreach (FormatViewDefinition definition2 in definition.FormatViewDefinition)
                {
                    if (!dictionary.ContainsKey(definition2.InstanceId))
                    {
                        dictionary.Add(definition2.InstanceId, new List<ExtendedTypeDefinition>());
                    }
                    if (!dictionary2.ContainsKey(definition2.InstanceId))
                    {
                        dictionary2.Add(definition2.InstanceId, definition2);
                    }
                    dictionary[definition2.InstanceId].Add(definition);
                }
            }
            foreach (Guid guid in dictionary2.Keys)
            {
                FormatViewDefinition definition3 = dictionary2[guid];
                _writer.WriteStartElement(_tagView);
                _writer.WriteElementString(_tagName, definition3.Name);
                _writer.WriteStartElement(_tagViewSelectedBy);
                foreach (ExtendedTypeDefinition definition4 in dictionary[guid])
                {
                    _writer.WriteElementString(_tagTypeName, definition4.TypeName);
                }
                _writer.WriteEndElement();
                definition3.Control.WriteToXML(_writer, exportScriptBlock);
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
        }
    }
}

