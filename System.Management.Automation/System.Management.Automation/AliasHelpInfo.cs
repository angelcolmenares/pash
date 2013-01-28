namespace System.Management.Automation
{
    using System;
    using System.Globalization;

    internal class AliasHelpInfo : HelpInfo
    {
        private PSObject _fullHelpObject = new PSObject();
        private string _name = "";
        private string _synopsis = "";

        private AliasHelpInfo(AliasInfo aliasInfo)
        {
            string str = (aliasInfo.ResolvedCommand == null) ? aliasInfo.UnresolvedCommandName : aliasInfo.ResolvedCommand.Name;
            base.ForwardTarget = str;
            base.ForwardHelpCategory = System.Management.Automation.HelpCategory.Workflow | System.Management.Automation.HelpCategory.ExternalScript | System.Management.Automation.HelpCategory.Filter | System.Management.Automation.HelpCategory.Function | System.Management.Automation.HelpCategory.ScriptCommand | System.Management.Automation.HelpCategory.Cmdlet;
            if (!string.IsNullOrEmpty(aliasInfo.Name))
            {
                this._name = aliasInfo.Name.Trim();
            }
            if (!string.IsNullOrEmpty(str))
            {
                this._synopsis = str.Trim();
            }
            this._fullHelpObject.TypeNames.Clear();
            this._fullHelpObject.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "AliasHelpInfo#{0}", new object[] { this.Name }));
            this._fullHelpObject.TypeNames.Add("AliasHelpInfo");
            this._fullHelpObject.TypeNames.Add("HelpInfo");
        }

        internal static AliasHelpInfo GetHelpInfo(AliasInfo aliasInfo)
        {
            if (aliasInfo == null)
            {
                return null;
            }
            if ((aliasInfo.ResolvedCommand == null) && (aliasInfo.UnresolvedCommandName == null))
            {
                return null;
            }
            AliasHelpInfo info = new AliasHelpInfo(aliasInfo);
            if (string.IsNullOrEmpty(info.Name))
            {
                return null;
            }
            info.AddCommonHelpProperties();
            return info;
        }

        internal override PSObject FullHelp
        {
            get
            {
                return this._fullHelpObject;
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.Alias;
            }
        }

        internal override string Name
        {
            get
            {
                return this._name;
            }
        }

        internal override string Synopsis
        {
            get
            {
                return this._synopsis;
            }
        }
    }
}

