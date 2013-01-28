namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;

    public sealed class WideControlEntryItem
    {
        private System.Management.Automation.DisplayEntry _entry;
        private List<string> _selectedBy;

        internal WideControlEntryItem()
        {
            this._selectedBy = new List<string>();
        }

        internal WideControlEntryItem(WideControlEntryDefinition definition)
        {
            this._selectedBy = new List<string>();
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
            if (definition.appliesTo != null)
            {
                foreach (TypeOrGroupReference reference in definition.appliesTo.referenceList)
                {
                    this._selectedBy.Add(reference.name);
                }
            }
        }

        public WideControlEntryItem(System.Management.Automation.DisplayEntry entry)
        {
            this._selectedBy = new List<string>();
            if (entry == null)
            {
                throw PSTraceSource.NewArgumentNullException("entry");
            }
            this._entry = entry;
        }

        public WideControlEntryItem(System.Management.Automation.DisplayEntry entry, IEnumerable<string> selectedBy)
        {
            this._selectedBy = new List<string>();
            if (entry == null)
            {
                throw PSTraceSource.NewArgumentNullException("entry");
            }
            if (selectedBy == null)
            {
                throw PSTraceSource.NewArgumentNullException("selectedBy");
            }
            this._entry = entry;
            foreach (string str in selectedBy)
            {
                this._selectedBy.Add(str);
            }
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

        public List<string> SelectedBy
        {
            get
            {
                return this._selectedBy;
            }
            internal set
            {
                this._selectedBy = value;
            }
        }
    }
}

