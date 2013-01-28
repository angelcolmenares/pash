namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Xml;

    internal class MamlCommandHelpInfo : BaseCommandHelpInfo
    {
        private string _component;
        private PSObject _fullHelpObject;
        private string _functionality;
        private string _role;

        internal MamlCommandHelpInfo(PSObject helpObject, HelpCategory helpCategory) : base(helpCategory)
        {
            this._fullHelpObject = helpObject;
            base.ForwardHelpCategory = HelpCategory.Provider;
            base.AddCommonHelpProperties();
            if (helpObject.Properties["Component"] != null)
            {
                this._component = helpObject.Properties["Component"].Value as string;
            }
            if (helpObject.Properties["Role"] != null)
            {
                this._role = helpObject.Properties["Role"].Value as string;
            }
            if (helpObject.Properties["Functionality"] != null)
            {
                this._functionality = helpObject.Properties["Functionality"].Value as string;
            }
        }

        private MamlCommandHelpInfo(System.Xml.XmlNode xmlNode, HelpCategory helpCategory) : base(helpCategory)
        {
            MamlNode node = new MamlNode(xmlNode);
            this._fullHelpObject = node.PSObject;
            base.Errors = node.Errors;
            this._fullHelpObject.TypeNames.Clear();
            this._fullHelpObject.TypeNames.Add("MamlCommandHelpInfo");
            this._fullHelpObject.TypeNames.Add("HelpInfo");
            base.ForwardHelpCategory = HelpCategory.Provider;
        }

        internal void AddUserDefinedData(UserDefinedHelpData userDefinedData)
        {
            if (userDefinedData != null)
            {
                if (userDefinedData.Properties.ContainsKey("component"))
                {
                    this._component = userDefinedData.Properties["component"];
                }
                if (userDefinedData.Properties.ContainsKey("role"))
                {
                    this._role = userDefinedData.Properties["role"];
                }
                if (userDefinedData.Properties.ContainsKey("functionality"))
                {
                    this._functionality = userDefinedData.Properties["functionality"];
                }
                base.UpdateUserDefinedDataProperties();
            }
        }

        internal MamlCommandHelpInfo Copy()
        {
            return new MamlCommandHelpInfo(this._fullHelpObject.Copy(), this.HelpCategory);
        }

        internal MamlCommandHelpInfo Copy(HelpCategory newCategoryToUse)
        {
            MamlCommandHelpInfo info = new MamlCommandHelpInfo(this._fullHelpObject.Copy(), newCategoryToUse);
            info.FullHelp.Properties["Category"].Value = newCategoryToUse;
            return info;
        }

        private string ExtractText(PSObject psObject)
        {
            if (psObject == null)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder(400);
            foreach (PSPropertyInfo info in psObject.Properties)
            {
                PSObject[] objArray;
                string str2 = info.TypeNameOfValue.ToLowerInvariant();
                if (str2 == null)
                {
                    goto Label_0127;
                }
                if ((str2 != "system.boolean") && (str2 != "system.int32"))
                {
                    if (!(str2 == "system.string"))
                    {
                        if (str2 == "system.management.automation.psobject[]")
                        {
                            goto Label_00BF;
                        }
                        if (str2 == "system.management.automation.psobject")
                        {
                            goto Label_010D;
                        }
                        goto Label_0127;
                    }
                    builder.Append((string) LanguagePrimitives.ConvertTo(info.Value, typeof(string), CultureInfo.InvariantCulture));
                }
                continue;
            Label_00BF:
                objArray = (PSObject[]) LanguagePrimitives.ConvertTo(info.Value, typeof(PSObject[]), CultureInfo.InvariantCulture);
                foreach (PSObject obj2 in objArray)
                {
                    builder.Append(this.ExtractText(obj2));
                }
                continue;
            Label_010D:
                builder.Append(this.ExtractText(PSObject.AsPSObject(info.Value)));
                continue;
            Label_0127:
                builder.Append(this.ExtractText(PSObject.AsPSObject(info.Value)));
            }
            return builder.ToString();
        }

        internal static MamlCommandHelpInfo Load(System.Xml.XmlNode xmlNode, HelpCategory helpCategory)
        {
            MamlCommandHelpInfo info = new MamlCommandHelpInfo(xmlNode, helpCategory);
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
            if (!string.IsNullOrEmpty(synopsis) && pattern.IsMatch(synopsis))
            {
                return true;
            }
            string detailedDescription = base.DetailedDescription;
            if (!string.IsNullOrEmpty(detailedDescription) && pattern.IsMatch(detailedDescription))
            {
                return true;
            }
            string examples = this.Examples;
            if (!string.IsNullOrEmpty(examples) && pattern.IsMatch(examples))
            {
                return true;
            }
            string notes = this.Notes;
            return (!string.IsNullOrEmpty(notes) && pattern.IsMatch(notes));
        }

        internal void OverrideProviderSpecificHelpWithGenericHelp(HelpInfo genericHelpInfo)
        {
            PSObject fullHelp = genericHelpInfo.FullHelp;
            MamlUtil.OverrideName(this._fullHelpObject, fullHelp);
            MamlUtil.OverridePSTypeNames(this._fullHelpObject, fullHelp);
            MamlUtil.PrependSyntax(this._fullHelpObject, fullHelp);
            MamlUtil.PrependDetailedDescription(this._fullHelpObject, fullHelp);
            MamlUtil.OverrideParameters(this._fullHelpObject, fullHelp);
            MamlUtil.PrependNotes(this._fullHelpObject, fullHelp);
            MamlUtil.AddCommonProperties(this._fullHelpObject, fullHelp);
        }

        internal void SetAdditionalDataFromHelpComment(string component, string functionality, string role)
        {
            this._component = component;
            this._functionality = functionality;
            this._role = role;
            base.UpdateUserDefinedDataProperties();
        }

        internal override string Component
        {
            get
            {
                return this._component;
            }
        }

        private string Examples
        {
            get
            {
                if ((this.FullHelp != null) && ((this.FullHelp.Properties["Examples"] != null) && (this.FullHelp.Properties["Examples"].Value != null)))
                {
                    return this.ExtractText(PSObject.AsPSObject(this.FullHelp.Properties["Examples"].Value));
                }
                return string.Empty;
            }
        }

        internal override PSObject FullHelp
        {
            get
            {
                return this._fullHelpObject;
            }
        }

        internal override string Functionality
        {
            get
            {
                return this._functionality;
            }
        }

        private string Notes
        {
            get
            {
                if ((this.FullHelp != null) && ((this.FullHelp.Properties["alertset"] != null) && (this.FullHelp.Properties["alertset"].Value != null)))
                {
                    return this.ExtractText(PSObject.AsPSObject(this.FullHelp.Properties["alertset"].Value));
                }
                return string.Empty;
            }
        }

        internal override string Role
        {
            get
            {
                return this._role;
            }
        }
    }
}

