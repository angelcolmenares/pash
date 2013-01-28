namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    public sealed class WideControl : PSControl
    {
        private System.Management.Automation.Alignment _aligment;
        private int _columns;
        private List<WideControlEntryItem> _entries;
        private static string _tagAlignment = "Alignment";
        private static string _tagColumnNumber = "ColumnNumber";
        private static string _tagSelectedBy = "EntrySelectedBy";
        private static string _tagTypeName = "TypeName";
        private static string _tagWideControl = "WideControl";
        private static string _tagWideEntries = "WideEntries";
        private static string _tagWideEntry = "WideEntry";
        private static string _tagWideItem = "WideItem";

        public WideControl()
        {
            this._entries = new List<WideControlEntryItem>();
        }

        public WideControl(IEnumerable<WideControlEntryItem> wideEntries)
        {
            this._entries = new List<WideControlEntryItem>();
            if (wideEntries == null)
            {
                throw PSTraceSource.NewArgumentNullException("wideEntries");
            }
            foreach (WideControlEntryItem item in wideEntries)
            {
                this._entries.Add(item);
            }
        }

        internal WideControl(WideControlBody widecontrolbody)
        {
            this._entries = new List<WideControlEntryItem>();
            this._columns = (int) widecontrolbody.columns;
            this._aligment = (System.Management.Automation.Alignment) widecontrolbody.alignment;
            this._entries.Add(new WideControlEntryItem(widecontrolbody.defaultEntryDefinition));
            foreach (WideControlEntryDefinition definition in widecontrolbody.optionalEntryList)
            {
                this._entries.Add(new WideControlEntryItem(definition));
            }
        }

        public WideControl(int columns)
        {
            this._entries = new List<WideControlEntryItem>();
            this._columns = columns;
        }

        public WideControl(IEnumerable<WideControlEntryItem> wideEntries, int columns)
        {
            this._entries = new List<WideControlEntryItem>();
            if (wideEntries == null)
            {
                throw PSTraceSource.NewArgumentNullException("wideEntries");
            }
            foreach (WideControlEntryItem item in wideEntries)
            {
                this._entries.Add(item);
            }
            this._columns = columns;
        }

        internal override bool SafeForExport()
        {
            foreach (WideControlEntryItem item in this._entries)
            {
                if (item.DisplayEntry.ValueType == DisplayEntryValueType.ScriptBlock)
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "WideControl", new object[0]);
        }

        internal override void WriteToXML(XmlWriter _writer, bool exportScriptBlock)
        {
            _writer.WriteStartElement(_tagWideControl);
            if (this._columns > 0)
            {
                _writer.WriteElementString(_tagColumnNumber, this._columns.ToString(CultureInfo.InvariantCulture));
            }
            if (this._aligment != System.Management.Automation.Alignment.Undefined)
            {
                _writer.WriteElementString(_tagAlignment, this._aligment.ToString());
            }
            _writer.WriteStartElement(_tagWideEntries);
            foreach (WideControlEntryItem item in this._entries)
            {
                _writer.WriteStartElement(_tagWideEntry);
                if (item.SelectedBy.Count > 0)
                {
                    _writer.WriteStartElement(_tagSelectedBy);
                    foreach (string str in item.SelectedBy)
                    {
                        _writer.WriteElementString(_tagTypeName, str);
                    }
                    _writer.WriteEndElement();
                }
                _writer.WriteStartElement(_tagWideItem);
                item.DisplayEntry.WriteToXML(_writer, exportScriptBlock);
                _writer.WriteEndElement();
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
        }

        public System.Management.Automation.Alignment Alignment
        {
            get
            {
                return this._aligment;
            }
            internal set
            {
                this._aligment = value;
            }
        }

        public int Columns
        {
            get
            {
                return this._columns;
            }
            internal set
            {
                this._columns = value;
            }
        }

        public List<WideControlEntryItem> Entries
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

