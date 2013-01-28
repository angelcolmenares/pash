namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System.Collections.Generic;

    internal sealed class TableControlBody : ControlBody
    {
        internal TableRowDefinition defaultDefinition;
        internal TableHeaderDefinition header = new TableHeaderDefinition();
        internal List<TableRowDefinition> optionalDefinitionList = new List<TableRowDefinition>();

        internal override ControlBase Copy()
        {
            TableControlBody body = new TableControlBody {
                autosize = base.autosize,
                header = this.header.Copy()
            };
            if (this.defaultDefinition != null)
            {
                body.defaultDefinition = this.defaultDefinition.Copy();
            }
            foreach (TableRowDefinition definition in this.optionalDefinitionList)
            {
                body.optionalDefinitionList.Add(definition);
            }
            return body;
        }
    }
}

