namespace System.Management.Automation
{
    using System;

    internal class NullVariable : PSVariable
    {
        private string description;

        internal NullVariable() : base("null", null, ScopedItemOptions.AllScope | ScopedItemOptions.Constant)
        {
        }

        public override string Description
        {
            get
            {
                if (this.description == null)
                {
                    this.description = SessionStateStrings.DollarNullDescription;
                }
                return this.description;
            }
            set
            {
            }
        }

        public override ScopedItemOptions Options
        {
            get
            {
                return ScopedItemOptions.None;
            }
            set
            {
            }
        }

        public override object Value
        {
            get
            {
                return null;
            }
            set
            {
            }
        }
    }
}

