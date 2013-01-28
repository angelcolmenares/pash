namespace System.Management.Automation
{
    using System;

    public sealed class TableControlColumn
    {
        private System.Management.Automation.Alignment _alignment;
        private System.Management.Automation.DisplayEntry _entry;

        internal TableControlColumn()
        {
        }

        public TableControlColumn(System.Management.Automation.Alignment alignment, System.Management.Automation.DisplayEntry entry)
        {
            this._alignment = alignment;
            this._entry = entry;
        }

        internal TableControlColumn(string text, int alignment, bool isscriptblock)
        {
            this._alignment = (System.Management.Automation.Alignment) alignment;
            if (isscriptblock)
            {
                this._entry = new System.Management.Automation.DisplayEntry(text, DisplayEntryValueType.ScriptBlock);
            }
            else
            {
                this._entry = new System.Management.Automation.DisplayEntry(text, DisplayEntryValueType.Property);
            }
        }

        public override string ToString()
        {
            return this._entry.Value;
        }

        public System.Management.Automation.Alignment Alignment
        {
            get
            {
                return this._alignment;
            }
            internal set
            {
                this._alignment = value;
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
    }
}

