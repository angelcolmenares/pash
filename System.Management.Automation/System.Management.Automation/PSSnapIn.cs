namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    public abstract class PSSnapIn : PSSnapInInstaller
    {
        private Dictionary<string, object> _regValues;

        protected PSSnapIn()
        {
        }

        public virtual string[] Formats
        {
            get
            {
                return null;
            }
        }

        internal override Dictionary<string, object> RegValues
        {
            get
            {
                if (this._regValues == null)
                {
                    this._regValues = base.RegValues;
                    if ((this.Types != null) && (this.Types.Length > 0))
                    {
                        this._regValues["Types"] = this.Types;
                    }
                    if ((this.Formats != null) && (this.Formats.Length > 0))
                    {
                        this._regValues["Formats"] = this.Formats;
                    }
                }
                return this._regValues;
            }
        }

        public virtual string[] Types
        {
            get
            {
                return null;
            }
        }
    }
}

