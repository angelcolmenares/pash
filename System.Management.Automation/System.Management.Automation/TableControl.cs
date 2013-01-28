namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    public sealed class TableControl : PSControl
    {
        private List<TableControlColumnHeader> _headers;
        private List<TableControlRow> _rows;
        private static string _tagAlignment = "Alignment";
        private static string _tagLabel = "Label";
        private static string _tagTableColumnHeader = "TableColumnHeader";
        private static string _tagTableColumnItem = "TableColumnItem";
        private static string _tagTableColumnItems = "TableColumnItems";
        private static string _tagTableControl = "TableControl";
        private static string _tagTableHeaders = "TableHeaders";
        private static string _tagTableRowEntries = "TableRowEntries";
        private static string _tagTableRowEntry = "TableRowEntry";
        private static string _tagWidth = "Width";

        public TableControl()
        {
            this._headers = new List<TableControlColumnHeader>();
            this._rows = new List<TableControlRow>();
        }

        internal TableControl(TableControlBody tcb)
        {
            this._headers = new List<TableControlColumnHeader>();
            this._rows = new List<TableControlRow>();
            TableControlRow item = new TableControlRow(tcb.defaultDefinition);
            this._rows.Add(item);
            foreach (TableRowDefinition definition in tcb.optionalDefinitionList)
            {
                item = new TableControlRow(definition);
                this._rows.Add(item);
            }
            foreach (TableColumnHeaderDefinition definition2 in tcb.header.columnHeaderDefinitionList)
            {
                TableControlColumnHeader header = new TableControlColumnHeader(definition2);
                this._headers.Add(header);
            }
        }

        public TableControl(TableControlRow tableControlRow)
        {
            this._headers = new List<TableControlColumnHeader>();
            this._rows = new List<TableControlRow>();
            if (tableControlRow == null)
            {
                throw PSTraceSource.NewArgumentNullException("tableControlRows");
            }
            this._rows.Add(tableControlRow);
        }

        public TableControl(TableControlRow tableControlRow, IEnumerable<TableControlColumnHeader> tableControlColumnHeaders)
        {
            this._headers = new List<TableControlColumnHeader>();
            this._rows = new List<TableControlRow>();
            if (tableControlRow == null)
            {
                throw PSTraceSource.NewArgumentNullException("tableControlRows");
            }
            if (tableControlColumnHeaders == null)
            {
                throw PSTraceSource.NewArgumentNullException("tableControlColumnHeaders");
            }
            this._rows.Add(tableControlRow);
            foreach (TableControlColumnHeader header in tableControlColumnHeaders)
            {
                this._headers.Add(header);
            }
        }

        internal override bool SafeForExport()
        {
            foreach (TableControlRow row in this._rows)
            {
                foreach (TableControlColumn column in row.Columns)
                {
                    if (column.DisplayEntry.ValueType == DisplayEntryValueType.ScriptBlock)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "TableControl", new object[0]);
        }

        internal override void WriteToXML(XmlWriter _writer, bool exportScriptBlock)
        {
            _writer.WriteStartElement(_tagTableControl);
            _writer.WriteStartElement(_tagTableHeaders);
            foreach (TableControlColumnHeader header in this._headers)
            {
                _writer.WriteStartElement(_tagTableColumnHeader);
                if (!string.IsNullOrEmpty(header.Label))
                {
                    _writer.WriteElementString(_tagLabel, header.Label);
                }
                if (header.Width > 0)
                {
                    _writer.WriteElementString(_tagWidth, header.Width.ToString(CultureInfo.InvariantCulture));
                }
                if (header.Alignment != Alignment.Undefined)
                {
                    _writer.WriteElementString(_tagAlignment, header.Alignment.ToString());
                }
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
            _writer.WriteStartElement(_tagTableRowEntries);
            foreach (TableControlRow row in this._rows)
            {
                _writer.WriteStartElement(_tagTableRowEntry);
                _writer.WriteStartElement(_tagTableColumnItems);
                foreach (TableControlColumn column in row.Columns)
                {
                    _writer.WriteStartElement(_tagTableColumnItem);
                    if (column.Alignment != Alignment.Undefined)
                    {
                        _writer.WriteElementString(_tagAlignment, column.Alignment.ToString());
                    }
                    column.DisplayEntry.WriteToXML(_writer, exportScriptBlock);
                    _writer.WriteEndElement();
                }
                _writer.WriteEndElement();
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
        }

        public List<TableControlColumnHeader> Headers
        {
            get
            {
                return this._headers;
            }
            internal set
            {
                this._headers = value;
            }
        }

        public List<TableControlRow> Rows
        {
            get
            {
                return this._rows;
            }
            internal set
            {
                this._rows = value;
            }
        }
    }
}

