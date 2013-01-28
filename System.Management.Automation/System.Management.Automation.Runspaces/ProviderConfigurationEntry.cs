namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class ProviderConfigurationEntry : RunspaceConfigurationEntry
    {
        private string _helpFileName;
        private Type _type;

        public ProviderConfigurationEntry(string name, Type implementingType, string helpFileName) : base(name)
        {
            if (implementingType == null)
            {
                throw PSTraceSource.NewArgumentNullException("implementingType");
            }
            this._type = implementingType;
            if (!string.IsNullOrEmpty(helpFileName))
            {
                this._helpFileName = helpFileName.Trim();
            }
            else
            {
                this._helpFileName = helpFileName;
            }
        }

        internal ProviderConfigurationEntry(string name, Type implementingType, string helpFileName, PSSnapInInfo psSnapinInfo) : base(name, psSnapinInfo)
        {
            if (implementingType == null)
            {
                throw PSTraceSource.NewArgumentNullException("implementingType");
            }
            this._type = implementingType;
            if (!string.IsNullOrEmpty(helpFileName))
            {
                this._helpFileName = helpFileName.Trim();
            }
            else
            {
                this._helpFileName = helpFileName;
            }
        }

        public string HelpFileName
        {
            get
            {
                return this._helpFileName;
            }
        }

        public Type ImplementingType
        {
            get
            {
                return this._type;
            }
        }
    }
}

