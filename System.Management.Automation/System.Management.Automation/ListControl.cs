namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    public sealed class ListControl : PSControl
    {
        private List<ListControlEntry> _entries;
        private static string _tagEntrySelectedBy = "EntrySelectedBy";
        private static string _tagLabel = "Label";
        private static string _tagListControl = "ListControl";
        private static string _tagListEntries = "ListEntries";
        private static string _tagListEntry = "ListEntry";
        private static string _tagListItem = "ListItem";
        private static string _tagListItems = "ListItems";
        private static string _tagTypeName = "TypeName";

        public ListControl()
        {
            this._entries = new List<ListControlEntry>();
        }

        public ListControl(IEnumerable<ListControlEntry> entries)
        {
            this._entries = new List<ListControlEntry>();
            if (entries == null)
            {
                throw PSTraceSource.NewArgumentNullException("entries");
            }
            foreach (ListControlEntry entry in entries)
            {
                this._entries.Add(entry);
            }
        }

        internal ListControl(ListControlBody listcontrolbody)
        {
            this._entries = new List<ListControlEntry>();
            this._entries.Add(new ListControlEntry(listcontrolbody.defaultEntryDefinition));
            foreach (ListControlEntryDefinition definition in listcontrolbody.optionalEntryList)
            {
                this._entries.Add(new ListControlEntry(definition));
            }
        }

        internal override bool SafeForExport()
        {
            foreach (ListControlEntry entry in this._entries)
            {
                foreach (ListControlEntryItem item in entry.Items)
                {
                    if (item.DisplayEntry.ValueType == DisplayEntryValueType.ScriptBlock)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "ListControl", new object[0]);
        }

        internal override void WriteToXML(XmlWriter _writer, bool exportScriptBlock)
        {
            _writer.WriteStartElement(_tagListControl);
            _writer.WriteStartElement(_tagListEntries);
            foreach (ListControlEntry entry in this._entries)
            {
                _writer.WriteStartElement(_tagListEntry);
                if (entry.SelectedBy.Count > 0)
                {
                    _writer.WriteStartElement(_tagEntrySelectedBy);
                    foreach (string str in entry.SelectedBy)
                    {
                        _writer.WriteElementString(_tagTypeName, str);
                    }
                    _writer.WriteEndElement();
                }
                if (entry.Items.Count > 0)
                {
                    _writer.WriteStartElement(_tagListItems);
                    foreach (ListControlEntryItem item in entry.Items)
                    {
                        _writer.WriteStartElement(_tagListItem);
                        if (!string.IsNullOrEmpty(item.Label))
                        {
                            _writer.WriteElementString(_tagLabel, item.Label);
                        }
                        item.DisplayEntry.WriteToXML(_writer, exportScriptBlock);
                        _writer.WriteEndElement();
                    }
                    _writer.WriteEndElement();
                }
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
        }

        public List<ListControlEntry> Entries
        {
            get
            {
                return this._entries;
            }
            internal set
            {
                this._entries = value;
            }
        }
    }
}

