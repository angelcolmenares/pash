namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Xml;

    internal class ProviderHelpInfo : HelpInfo
    {
        private PSObject _fullHelpObject;

        private ProviderHelpInfo(System.Xml.XmlNode xmlNode)
        {
            MamlNode node = new MamlNode(xmlNode);
            this._fullHelpObject = node.PSObject;
            base.Errors = node.Errors;
            this._fullHelpObject.TypeNames.Clear();
            this._fullHelpObject.TypeNames.Add("ProviderHelpInfo");
            this._fullHelpObject.TypeNames.Add("HelpInfo");
        }

        internal static ProviderHelpInfo Load(System.Xml.XmlNode xmlNode)
        {
            ProviderHelpInfo info = new ProviderHelpInfo(xmlNode);
            if (string.IsNullOrEmpty(info.Name))
            {
                return null;
            }
            info.AddCommonHelpProperties();
            return info;
        }

        internal override bool MatchPatternInContent(WildcardPattern pattern)
        {
            string synopsis = this.Synopsis;
            string detailedDescription = this.DetailedDescription;
            if (synopsis == null)
            {
                synopsis = string.Empty;
            }
            if (detailedDescription == null)
            {
                detailedDescription = string.Empty;
            }
            if (!pattern.IsMatch(synopsis))
            {
                return pattern.IsMatch(detailedDescription);
            }
            return true;
        }

        internal string DetailedDescription
        {
            get
            {
                if (this.FullHelp == null)
                {
                    return "";
                }
                if ((this.FullHelp.Properties["DetailedDescription"] == null) || (this.FullHelp.Properties["DetailedDescription"].Value == null))
                {
                    return "";
                }
                IList list = this.FullHelp.Properties["DetailedDescription"].Value as IList;
                if ((list == null) || (list.Count == 0))
                {
                    return "";
                }
                StringBuilder builder = new StringBuilder(400);
                foreach (object obj2 in list)
                {
                    PSObject obj3 = PSObject.AsPSObject(obj2);
                    if (((obj3 != null) && (obj3.Properties["Text"] != null)) && (obj3.Properties["Text"].Value != null))
                    {
                        string str = obj3.Properties["Text"].Value.ToString();
                        builder.Append(str);
                        builder.Append(Environment.NewLine);
                    }
                }
                return builder.ToString().Trim();
            }
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
                return System.Management.Automation.HelpCategory.Provider;
            }
        }

        internal override string Name
        {
            get
            {
                if (this._fullHelpObject == null)
                {
                    return "";
                }
                if (this._fullHelpObject.Properties["Name"] == null)
                {
                    return "";
                }
                if (this._fullHelpObject.Properties["Name"].Value == null)
                {
                    return "";
                }
                string str = this._fullHelpObject.Properties["Name"].Value.ToString();
                if (str == null)
                {
                    return "";
                }
                return str.Trim();
            }
        }

        internal override string Synopsis
        {
            get
            {
                if (this._fullHelpObject == null)
                {
                    return "";
                }
                if (this._fullHelpObject.Properties["Synopsis"] == null)
                {
                    return "";
                }
                if (this._fullHelpObject.Properties["Synopsis"].Value == null)
                {
                    return "";
                }
                string str = this._fullHelpObject.Properties["Synopsis"].Value.ToString();
                if (str == null)
                {
                    return "";
                }
                return str.Trim();
            }
        }
    }
}

