namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class TableHeaderDefinition
    {
        internal List<TableColumnHeaderDefinition> columnHeaderDefinitionList = new List<TableColumnHeaderDefinition>();
        internal bool hideHeader;

        internal TableHeaderDefinition Copy()
        {
            TableHeaderDefinition definition = new TableHeaderDefinition {
                hideHeader = this.hideHeader
            };
            foreach (TableColumnHeaderDefinition definition2 in this.columnHeaderDefinitionList)
            {
                definition.columnHeaderDefinitionList.Add(definition2);
            }
            return definition;
        }
    }
}

