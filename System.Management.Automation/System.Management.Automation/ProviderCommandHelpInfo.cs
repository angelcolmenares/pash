namespace System.Management.Automation
{
    using System;

    internal class ProviderCommandHelpInfo : HelpInfo
    {
        private HelpInfo _helpInfo;

        internal ProviderCommandHelpInfo(HelpInfo genericHelpInfo, ProviderContext providerContext)
        {
            base.ForwardHelpCategory = System.Management.Automation.HelpCategory.None;
            MamlCommandHelpInfo providerSpecificHelpInfo = providerContext.GetProviderSpecificHelpInfo(genericHelpInfo.Name);
            if (providerSpecificHelpInfo == null)
            {
                this._helpInfo = genericHelpInfo;
            }
            else
            {
                providerSpecificHelpInfo.OverrideProviderSpecificHelpWithGenericHelp(genericHelpInfo);
                this._helpInfo = providerSpecificHelpInfo;
            }
        }

        internal override PSObject[] GetParameter(string pattern)
        {
            return this._helpInfo.GetParameter(pattern);
        }

        internal override Uri GetUriForOnlineHelp()
        {
            return this._helpInfo.GetUriForOnlineHelp();
        }

        internal override string Component
        {
            get
            {
                return this._helpInfo.Component;
            }
        }

        internal override PSObject FullHelp
        {
            get
            {
                return this._helpInfo.FullHelp;
            }
        }

        internal override string Functionality
        {
            get
            {
                return this._helpInfo.Functionality;
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return this._helpInfo.HelpCategory;
            }
        }

        internal override string Name
        {
            get
            {
                return this._helpInfo.Name;
            }
        }

        internal override string Role
        {
            get
            {
                return this._helpInfo.Role;
            }
        }

        internal override string Synopsis
        {
            get
            {
                return this._helpInfo.Synopsis;
            }
        }
    }
}

