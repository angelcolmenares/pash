namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;

    public sealed class ListControlEntryItem
    {
        private System.Management.Automation.DisplayEntry _entry;
        private string _label;

        internal ListControlEntryItem()
        {
        }

        internal ListControlEntryItem(ListControlItemDefinition definition)
        {
            if (definition.label != null)
            {
                this._label = definition.label.text;
            }
            FieldPropertyToken token = definition.formatTokenList[0] as FieldPropertyToken;
            if (token != null)
            {
                if (token.expression.isScriptBlock)
                {
                    this._entry = new System.Management.Automation.DisplayEntry(token.expression.expressionValue, DisplayEntryValueType.ScriptBlock);
                }
                else
                {
                    this._entry = new System.Management.Automation.DisplayEntry(token.expression.expressionValue, DisplayEntryValueType.Property);
                }
            }
        }

        public ListControlEntryItem(string label, System.Management.Automation.DisplayEntry entry)
        {
            this._label = label;
            this._entry = entry;
        }

        public System.Management.Automation.DisplayEntry DisplayEntry
        {
            get
            {
                return this._entry;
            }
            internal set
            {
                this._entry = value;
            }
        }

        public string Label
        {
            get
            {
                return this._label;
            }
            internal set
            {
                this._label = value;
            }
        }
    }
}

