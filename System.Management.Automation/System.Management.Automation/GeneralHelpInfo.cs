namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Xml;

    internal class GeneralHelpInfo : HelpInfo
    {
        private PSObject _fullHelpObject;

        protected GeneralHelpInfo(System.Xml.XmlNode xmlNode)
        {
            MamlNode node = new MamlNode(xmlNode);
            this._fullHelpObject = node.PSObject;
            base.Errors = node.Errors;
            this._fullHelpObject.TypeNames.Clear();
            this._fullHelpObject.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "GeneralHelpInfo#{0}", new object[] { this.Name }));
            this._fullHelpObject.TypeNames.Add("GeneralHelpInfo");
            this._fullHelpObject.TypeNames.Add("HelpInfo");
        }

        internal static GeneralHelpInfo Load(System.Xml.XmlNode xmlNode)
        {
            GeneralHelpInfo info = new GeneralHelpInfo(xmlNode);
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
                return System.Management.Automation.HelpCategory.General;
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
                return "";
            }
        }
    }
}

