namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public abstract class PSSnapInInstaller : PSInstaller
    {
        private string _psVersion;
        private Dictionary<string, object> _regValues;

        protected PSSnapInInstaller()
        {
        }

        public abstract string Description { get; }

        public virtual string DescriptionResource
        {
            get
            {
                return null;
            }
        }

        private string MshSnapinVersion
        {
            get
            {
                return base.GetType().Assembly.GetName().Version.ToString();
            }
        }

        public abstract string Name { get; }

        private string PSVersion
        {
            get
            {
                if (this._psVersion == null)
                {
                    this._psVersion = PSVersionInfo.FeatureVersionString;
                }
                return this._psVersion;
            }
        }

        internal sealed override string RegKey
        {
            get
            {
                PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(this.Name);
                return (@"PowerShellSnapIns\" + this.Name);
            }
        }

        internal override Dictionary<string, object> RegValues
        {
            get
            {
                if (this._regValues == null)
                {
                    this._regValues = new Dictionary<string, object>();
                    this._regValues["PowerShellVersion"] = this.PSVersion;
                    if (!string.IsNullOrEmpty(this.Vendor))
                    {
                        this._regValues["Vendor"] = this.Vendor;
                    }
                    if (!string.IsNullOrEmpty(this.Description))
                    {
                        this._regValues["Description"] = this.Description;
                    }
                    if (!string.IsNullOrEmpty(this.VendorResource))
                    {
                        this._regValues["VendorIndirect"] = this.VendorResource;
                    }
                    if (!string.IsNullOrEmpty(this.DescriptionResource))
                    {
                        this._regValues["DescriptionIndirect"] = this.DescriptionResource;
                    }
                    this._regValues["Version"] = this.MshSnapinVersion;
                    this._regValues["ApplicationBase"] = Path.GetDirectoryName(base.GetType().Assembly.Location);
                    this._regValues["AssemblyName"] = base.GetType().Assembly.FullName;
                    this._regValues["ModuleName"] = base.GetType().Assembly.Location;
                }
                return this._regValues;
            }
        }

        public abstract string Vendor { get; }

        public virtual string VendorResource
        {
            get
            {
                return null;
            }
        }
    }
}

