namespace Microsoft.PowerShell.Commands
{
    using System;

    public abstract class MeasureInfo
    {
        private string property;

        protected MeasureInfo()
        {
        }

        public string Property
        {
            get
            {
                return this.property;
            }
            set
            {
                this.property = value;
            }
        }
    }
}

