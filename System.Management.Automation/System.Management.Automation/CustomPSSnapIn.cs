namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Runspaces;

    public abstract class CustomPSSnapIn : PSSnapInInstaller
    {
        private Collection<CmdletConfigurationEntry> _cmdlets;
        private Collection<FormatConfigurationEntry> _formats;
        private Collection<ProviderConfigurationEntry> _providers;
        private Dictionary<string, object> _regValues;
        private Collection<TypeConfigurationEntry> _types;

        protected CustomPSSnapIn()
        {
        }

        public virtual Collection<CmdletConfigurationEntry> Cmdlets
        {
            get
            {
                if (this._cmdlets == null)
                {
                    this._cmdlets = new Collection<CmdletConfigurationEntry>();
                }
                return this._cmdlets;
            }
        }

        internal string CustomPSSnapInType
        {
            get
            {
                return base.GetType().FullName;
            }
        }

        public virtual Collection<FormatConfigurationEntry> Formats
        {
            get
            {
                if (this._formats == null)
                {
                    this._formats = new Collection<FormatConfigurationEntry>();
                }
                return this._formats;
            }
        }

        public virtual Collection<ProviderConfigurationEntry> Providers
        {
            get
            {
                if (this._providers == null)
                {
                    this._providers = new Collection<ProviderConfigurationEntry>();
                }
                return this._providers;
            }
        }

        internal override Dictionary<string, object> RegValues
        {
            get
            {
                if (this._regValues == null)
                {
                    this._regValues = base.RegValues;
                    if (!string.IsNullOrEmpty(this.CustomPSSnapInType))
                    {
                        this._regValues["CustomPSSnapInType"] = this.CustomPSSnapInType;
                    }
                }
                return this._regValues;
            }
        }

        public virtual Collection<TypeConfigurationEntry> Types
        {
            get
            {
                if (this._types == null)
                {
                    this._types = new Collection<TypeConfigurationEntry>();
                }
                return this._types;
            }
        }
    }
}

