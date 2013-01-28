namespace System.Management.Automation.Help
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Text;

    internal class UpdatableHelpInfo
    {
        private Collection<UpdatableHelpUri> _helpContentUriCollection;
        private string _unresolvedUri;
        private CultureSpecificUpdatableHelp[] _updatableHelpItems;

        internal UpdatableHelpInfo(string unresolvedUri, CultureSpecificUpdatableHelp[] cultures)
        {
            this._unresolvedUri = unresolvedUri;
            this._helpContentUriCollection = new Collection<UpdatableHelpUri>();
            this._updatableHelpItems = cultures;
        }

        internal Version GetCultureVersion(CultureInfo culture)
        {
            foreach (CultureSpecificUpdatableHelp help in this._updatableHelpItems)
            {
                if (string.Compare(help.Culture.Name, culture.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return help.Version;
                }
            }
            return null;
        }

        internal string GetSupportedCultures()
        {
            if (this._updatableHelpItems.Length == 0)
            {
                return StringUtil.Format(HelpDisplayStrings.None, new object[0]);
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this._updatableHelpItems.Length; i++)
            {
                builder.Append(this._updatableHelpItems[i].Culture.Name);
                if (i != (this._updatableHelpItems.Length - 1))
                {
                    builder.Append(" | ");
                }
            }
            return builder.ToString();
        }

        internal bool IsCultureSupported(CultureInfo culture)
        {
            foreach (CultureSpecificUpdatableHelp help in this._updatableHelpItems)
            {
                if (string.Compare(help.Culture.Name, culture.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsNewerVersion(UpdatableHelpInfo helpInfo, CultureInfo culture)
        {
            Version cultureVersion = helpInfo.GetCultureVersion(culture);
            Version version2 = this.GetCultureVersion(culture);
            return ((version2 == null) || (cultureVersion > version2));
        }

        internal Collection<UpdatableHelpUri> HelpContentUriCollection
        {
            get
            {
                return this._helpContentUriCollection;
            }
        }

        internal string UnresolvedUri
        {
            get
            {
                return this._unresolvedUri;
            }
        }

        internal CultureSpecificUpdatableHelp[] UpdatableHelpItems
        {
            get
            {
                return this._updatableHelpItems;
            }
        }
    }
}

