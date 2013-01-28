namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    internal class FaqHelpInfo : HelpInfo
    {
        private PSObject _fullHelpObject;

        protected FaqHelpInfo(System.Xml.XmlNode xmlNode)
        {
            MamlNode node = new MamlNode(xmlNode);
            this._fullHelpObject = node.PSObject;
            base.Errors = node.Errors;
            this._fullHelpObject.TypeNames.Clear();
            this._fullHelpObject.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "FaqHelpInfo#{0}", new object[] { this.Name }));
            this._fullHelpObject.TypeNames.Add("FaqHelpInfo");
            this._fullHelpObject.TypeNames.Add("HelpInfo");
        }

        internal static FaqHelpInfo Load(System.Xml.XmlNode xmlNode)
        {
            FaqHelpInfo info = new FaqHelpInfo(xmlNode);
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
            string answers = this.Answers;
            if (synopsis == null)
            {
                synopsis = string.Empty;
            }
            if (this.Answers == null)
            {
                answers = string.Empty;
            }
            if (!pattern.IsMatch(synopsis))
            {
                return pattern.IsMatch(answers);
            }
            return true;
        }

        private string Answers
        {
            get
            {
                if (this.FullHelp == null)
                {
                    return "";
                }
                if ((this.FullHelp.Properties["answer"] == null) || (this.FullHelp.Properties["answer"].Value == null))
                {
                    return "";
                }
                IList list = this.FullHelp.Properties["answer"].Value as IList;
                if ((list == null) || (list.Count == 0))
                {
                    return "";
                }
                StringBuilder builder = new StringBuilder();
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
                return System.Management.Automation.HelpCategory.FAQ;
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
                if (this._fullHelpObject.Properties["Title"] == null)
                {
                    return "";
                }
                if (this._fullHelpObject.Properties["Title"].Value == null)
                {
                    return "";
                }
                string str = this._fullHelpObject.Properties["Title"].Value.ToString();
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
                if (this._fullHelpObject.Properties["question"] == null)
                {
                    return "";
                }
                if (this._fullHelpObject.Properties["question"].Value == null)
                {
                    return "";
                }
                string str = this._fullHelpObject.Properties["question"].Value.ToString();
                if (str == null)
                {
                    return "";
                }
                return str.Trim();
            }
        }
    }
}

