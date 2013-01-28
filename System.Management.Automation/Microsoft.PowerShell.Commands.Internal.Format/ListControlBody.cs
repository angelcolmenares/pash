namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System.Collections.Generic;

    internal sealed class ListControlBody : ControlBody
    {
        internal ListControlEntryDefinition defaultEntryDefinition;
        internal List<ListControlEntryDefinition> optionalEntryList = new List<ListControlEntryDefinition>();

        internal override ControlBase Copy()
        {
            ListControlBody body = new ListControlBody {
                autosize = base.autosize
            };
            if (this.defaultEntryDefinition != null)
            {
                body.defaultEntryDefinition = this.defaultEntryDefinition.Copy();
            }
            foreach (ListControlEntryDefinition definition in this.optionalEntryList)
            {
                body.optionalEntryList.Add(definition);
            }
            return body;
        }
    }
}

