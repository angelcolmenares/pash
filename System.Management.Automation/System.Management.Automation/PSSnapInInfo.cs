namespace System.Management.Automation
{
    using Microsoft.Win32;
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Text.RegularExpressions;

    public class PSSnapInInfo
    {
        private string _applicationBase;
        private string _assemblyName;
        private string _customPSSnapInType;
        private string _description;
        private string _descriptionFallback;
        private string _descriptionIndirect;
        private Collection<string> _formats;
        private bool _isDefault;
        private bool _logPipelineExecutionDetails;
        private string _moduleName;
        private string _name;
        private System.Version _psVersion;
        private Collection<string> _types;
        private string _vendor;
        private string _vendorFallback;
        private string _vendorIndirect;
        private System.Version _version;

        internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, System.Version psVersion, System.Version version, Collection<string> types, Collection<string> formats, string descriptionFallback, string vendorFallback, string customPSSnapInType)
        {
            this._descriptionFallback = string.Empty;
            this._vendorFallback = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(applicationBase))
            {
                throw PSTraceSource.NewArgumentNullException("applicationBase");
            }
            if (string.IsNullOrEmpty(assemblyName))
            {
                throw PSTraceSource.NewArgumentNullException("assemblyName");
            }
            if (string.IsNullOrEmpty(moduleName))
            {
                throw PSTraceSource.NewArgumentNullException("moduleName");
            }
            if (psVersion == null)
            {
                throw PSTraceSource.NewArgumentNullException("psVersion");
            }
            if (version == null)
            {
                version = new System.Version("0.0");
            }
            if (types == null)
            {
                types = new Collection<string>();
            }
            if (formats == null)
            {
                formats = new Collection<string>();
            }
            if (descriptionFallback == null)
            {
                descriptionFallback = string.Empty;
            }
            if (vendorFallback == null)
            {
                vendorFallback = string.Empty;
            }
            this._name = name;
            this._isDefault = isDefault;
            this._applicationBase = applicationBase;
            this._assemblyName = assemblyName;
            this._moduleName = moduleName;
            this._psVersion = psVersion;
            this._version = version;
            this._types = types;
            this._formats = formats;
            this._customPSSnapInType = customPSSnapInType;
            this._descriptionFallback = descriptionFallback;
            this._vendorFallback = vendorFallback;
        }

        internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, System.Version psVersion, System.Version version, Collection<string> types, Collection<string> formats, string description, string descriptionFallback, string vendor, string vendorFallback, string customPSSnapInType) : this(name, isDefault, applicationBase, assemblyName, moduleName, psVersion, version, types, formats, descriptionFallback, vendorFallback, customPSSnapInType)
        {
            this._description = description;
            this._vendor = vendor;
        }

        internal PSSnapInInfo(string name, bool isDefault, string applicationBase, string assemblyName, string moduleName, System.Version psVersion, System.Version version, Collection<string> types, Collection<string> formats, string description, string descriptionFallback, string descriptionIndirect, string vendor, string vendorFallback, string vendorIndirect, string customPSSnapInType) : this(name, isDefault, applicationBase, assemblyName, moduleName, psVersion, version, types, formats, description, descriptionFallback, vendor, vendorFallback, customPSSnapInType)
        {
            if (isDefault)
            {
                this._descriptionIndirect = descriptionIndirect;
                this._vendorIndirect = vendorIndirect;
            }
        }

        internal PSSnapInInfo Clone()
        {
            return new PSSnapInInfo(this._name, this._isDefault, this._applicationBase, this._assemblyName, this._moduleName, this._psVersion, this._version, new Collection<string>(this.Types), new Collection<string>(this.Formats), this._description, this._descriptionFallback, this._descriptionIndirect, this._vendor, this._vendorFallback, this._vendorIndirect, this._customPSSnapInType);
        }

        internal static bool IsPSSnapinIdValid(string psSnapinId)
        {
            if (string.IsNullOrEmpty(psSnapinId))
            {
                return false;
            }
            return Regex.IsMatch(psSnapinId, "^[A-Za-z0-9-_.]*$");
        }

        internal void LoadIndirectResources()
        {
            using (RegistryStringResourceIndirect indirect = RegistryStringResourceIndirect.GetResourceIndirectReader())
            {
                this.LoadIndirectResources(indirect);
            }
        }

        internal void LoadIndirectResources(RegistryStringResourceIndirect resourceReader)
        {
            if (this.IsDefault)
            {
                this._description = resourceReader.GetResourceStringIndirect(this._assemblyName, this._moduleName, this._descriptionIndirect);
                this._vendor = resourceReader.GetResourceStringIndirect(this._assemblyName, this._moduleName, this._vendorIndirect);
            }
            else
            {
                RegistryKey mshSnapinKey = this.MshSnapinKey;
                if (mshSnapinKey != null)
                {
                    this._description = resourceReader.GetResourceStringIndirect(mshSnapinKey, "DescriptionIndirect", this._assemblyName, this._moduleName);
                    this._vendor = resourceReader.GetResourceStringIndirect(mshSnapinKey, "VendorIndirect", this._assemblyName, this._moduleName);
                }
            }
            if (string.IsNullOrEmpty(this._description))
            {
                this._description = this._descriptionFallback;
            }
            if (string.IsNullOrEmpty(this._vendor))
            {
                this._vendor = this._vendorFallback;
            }
        }

        public override string ToString()
        {
            return this._name;
        }

        internal static void VerifyPSSnapInFormatThrowIfError(string psSnapinId)
        {
            if (!IsPSSnapinIdValid(psSnapinId))
            {
                throw PSTraceSource.NewArgumentException("mshSnapInId", "MshSnapInCmdletResources", "InvalidPSSnapInName", new object[] { psSnapinId });
            }
        }

        internal string AbsoluteModulePath
        {
            get
            {
                if (!string.IsNullOrEmpty(this._moduleName) && !Path.IsPathRooted(this._moduleName))
                {
                    return Path.Combine(this._applicationBase, this._moduleName);
                }
                return this._moduleName;
            }
        }

        public string ApplicationBase
        {
            get
            {
                return this._applicationBase;
            }
        }

        public string AssemblyName
        {
            get
            {
                return this._assemblyName;
            }
        }

        internal string CustomPSSnapInType
        {
            get
            {
                return this._customPSSnapInType;
            }
        }

        public string Description
        {
            get
            {
                if (this._description == null)
                {
                    this.LoadIndirectResources();
                }
                return this._description;
            }
        }

        public Collection<string> Formats
        {
            get
            {
                return this._formats;
            }
        }

        public bool IsDefault
        {
            get
            {
                return this._isDefault;
            }
        }

        public bool LogPipelineExecutionDetails
        {
            get
            {
                return this._logPipelineExecutionDetails;
            }
            set
            {
                this._logPipelineExecutionDetails = value;
            }
        }

        public string ModuleName
        {
            get
            {
                return this._moduleName;
            }
        }

        internal RegistryKey MshSnapinKey
        {
            get
            {
                RegistryKey mshSnapinKey = null;
                try
                {
                    mshSnapinKey = PSSnapInReader.GetMshSnapinKey(this._name, this._psVersion.Major.ToString(CultureInfo.InvariantCulture));
                }
                catch (ArgumentException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (IOException)
                {
                }
                return mshSnapinKey;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public System.Version PSVersion
        {
            get
            {
                return this._psVersion;
            }
        }

        public Collection<string> Types
        {
            get
            {
                return this._types;
            }
        }

        public string Vendor
        {
            get
            {
                if (this._vendor == null)
                {
                    this.LoadIndirectResources();
                }
                return this._vendor;
            }
        }

        public System.Version Version
        {
            get
            {
                return this._version;
            }
        }
    }
}

