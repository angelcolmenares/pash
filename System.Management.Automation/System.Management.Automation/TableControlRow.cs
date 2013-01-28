namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;

    public sealed class TableControlRow
    {
        private List<TableControlColumn> _columns;

        public TableControlRow()
        {
            this._columns = new List<TableControlColumn>();
        }

        internal TableControlRow(TableRowDefinition rowdefinition)
        {
            this._columns = new List<TableControlColumn>();
            foreach (TableRowItemDefinition definition in rowdefinition.rowItemDefinitionList)
            {
                TableControlColumn column;
                FieldPropertyToken token = definition.formatTokenList[0] as FieldPropertyToken;
                if (token != null)
                {
                    column = new TableControlColumn(token.expression.expressionValue, definition.alignment, token.expression.isScriptBlock);
                }
                else
                {
                    column = new TableControlColumn();
                }
                this._columns.Add(column);
            }
        }

        public TableControlRow(IEnumerable<TableControlColumn> columns)
        {
            this._columns = new List<TableControlColumn>();
            if (columns == null)
            {
                throw PSTraceSource.NewArgumentNullException("columns");
            }
            foreach (TableControlColumn column in columns)
            {
                this._columns.Add(column);
            }
        }

        public List<TableControlColumn> Columns
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
    }
}

