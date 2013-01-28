namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class ListControlEntryDefinition
    {
        internal AppliesTo appliesTo;
        internal List<ListControlItemDefinition> itemDefinitionList = new List<ListControlItemDefinition>();

        internal ListControlEntryDefinition Copy()
        {
            ListControlEntryDefinition definition = new ListControlEntryDefinition {
                appliesTo = this.appliesTo
            };
            foreach (ListControlItemDefinition definition2 in this.itemDefinitionList)
            {
                definition.itemDefinitionList.Add(definition2);
            }
            return definition;
        }
    }
}

