namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class TableRowDefinition
    {
        internal AppliesTo appliesTo;
        internal bool multiLine;
        internal List<TableRowItemDefinition> rowItemDefinitionList = new List<TableRowItemDefinition>();

        internal TableRowDefinition Copy()
        {
            TableRowDefinition definition = new TableRowDefinition {
                appliesTo = this.appliesTo,
                multiLine = this.multiLine
            };
            foreach (TableRowItemDefinition definition2 in this.rowItemDefinitionList)
            {
                definition.rowItemDefinitionList.Add(definition2);
            }
            return definition;
        }
    }
}

