namespace System.Management.Automation.Help
{
    using System;
    using System.Globalization;

    internal class UpdatableHelpModuleInfo
    {
        private string _helpInfoUri;
        private string _moduleBase;
        private Guid _moduleGuid;
        private string _moduleName;
        internal static readonly string HelpContentZipName = "HelpContent.cab";
        internal static readonly string HelpIntoXmlName = "HelpInfo.xml";

        internal UpdatableHelpModuleInfo(string name, Guid guid, string path, string uri)
        {
            this._moduleName = name;
            this._moduleGuid = guid;
            this._moduleBase = path;
            this._helpInfoUri = uri;
        }

        internal string GetHelpContentName(CultureInfo culture)
        {
            return (this._moduleName + "_" + this._moduleGuid.ToString() + "_" + culture.Name + "_" + HelpContentZipName);
        }

        internal string GetHelpInfoName()
        {
            return (this._moduleName + "_" + this._moduleGuid.ToString() + "_" + HelpIntoXmlName);
        }

        internal string HelpInfoUri
        {
            get
            {
                return this._helpInfoUri;
            }
        }

        internal string ModuleBase
        {
            get
            {
                return this._moduleBase;
            }
        }

        internal Guid ModuleGuid
        {
            get
            {
                return this._moduleGuid;
            }
        }

        internal string ModuleName
        {
            get
            {
                return this._moduleName;
            }
        }
    }
}

