namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    internal class GlossaryHelpInfo : HelpInfo
    {
        private PSObject _fullHelpObject;
        private string _name = "";

        protected GlossaryHelpInfo(System.Xml.XmlNode xmlNode)
        {
            MamlNode node = new MamlNode(xmlNode);
            this._fullHelpObject = node.PSObject;
            base.Errors = node.Errors;
            this._name = this.GetTerm();
            this._fullHelpObject.TypeNames.Clear();
            this._fullHelpObject.TypeNames.Add(string.Format(CultureInfo.InvariantCulture, "GlossaryHelpInfo#{0}", new object[] { this.Name }));
            this._fullHelpObject.TypeNames.Add("GlossaryHelpInfo");
            this._fullHelpObject.TypeNames.Add("HelpInfo");
        }

        private string GetTerm()
        {
            if (this._fullHelpObject == null)
            {
                return "";
            }
            if (this._fullHelpObject.Properties["Terms"] == null)
            {
                return "";
            }
            if (this._fullHelpObject.Properties["Terms"].Value == null)
            {
                return "";
            }
            PSObject obj2 = (PSObject) this._fullHelpObject.Properties["Terms"].Value;
            if (obj2.Properties["Term"] == null)
            {
                return "";
            }
            if (obj2.Properties["Term"].Value == null)
            {
                return "";
            }
            if (obj2.Properties["Term"].Value.GetType().Equals(typeof(PSObject)))
            {
                PSObject obj3 = (PSObject) obj2.Properties["Term"].Value;
                return obj3.ToString();
            }
            if (!obj2.Properties["Term"].Value.GetType().Equals(typeof(PSObject[])))
            {
                return "";
            }
            PSObject[] objArray = (PSObject[]) obj2.Properties["Term"].Value;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < objArray.Length; i++)
            {
                string str = objArray[i].ToString();
                if (str != null)
                {
                    str = str.Trim();
                    if (!string.IsNullOrEmpty(str))
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(", ");
                        }
                        builder.Append(str);
                    }
                }
            }
            return builder.ToString();
        }

        internal static GlossaryHelpInfo Load(System.Xml.XmlNode xmlNode)
        {
            GlossaryHelpInfo info = new GlossaryHelpInfo(xmlNode);
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
                return System.Management.Automation.HelpCategory.Glossary;
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
                return "";
            }
        }
    }
}

