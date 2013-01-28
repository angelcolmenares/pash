namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;

    internal abstract class HelpInfo
    {
        private Collection<ErrorRecord> _errors;
        private System.Management.Automation.HelpCategory _forwardHelpCategory;
        private string _forwardTarget = "";

        internal HelpInfo()
        {
        }

        protected void AddCommonHelpProperties()
        {
            if (this.FullHelp != null)
            {
                if (this.FullHelp.Properties["Name"] == null)
                {
                    this.FullHelp.Properties.Add(new PSNoteProperty("Name", this.Name.ToString()));
                }
                if (this.FullHelp.Properties["Category"] == null)
                {
                    this.FullHelp.Properties.Add(new PSNoteProperty("Category", this.HelpCategory.ToString()));
                }
                if (this.FullHelp.Properties["Synopsis"] == null)
                {
                    this.FullHelp.Properties.Add(new PSNoteProperty("Synopsis", this.Synopsis.ToString()));
                }
                if (this.FullHelp.Properties["Component"] == null)
                {
                    this.FullHelp.Properties.Add(new PSNoteProperty("Component", this.Component));
                }
                if (this.FullHelp.Properties["Role"] == null)
                {
                    this.FullHelp.Properties.Add(new PSNoteProperty("Role", this.Role));
                }
                if (this.FullHelp.Properties["Functionality"] == null)
                {
                    this.FullHelp.Properties.Add(new PSNoteProperty("Functionality", this.Functionality));
                }
            }
        }

        internal virtual PSObject[] GetParameter(string pattern)
        {
            return new PSObject[0];
        }

        internal virtual Uri GetUriForOnlineHelp()
        {
            return null;
        }

        internal virtual bool MatchPatternInContent(WildcardPattern pattern)
        {
            return false;
        }

        protected void UpdateUserDefinedDataProperties()
        {
            if (this.FullHelp != null)
            {
                this.FullHelp.Properties.Remove("Component");
                this.FullHelp.Properties.Add(new PSNoteProperty("Component", this.Component));
                this.FullHelp.Properties.Remove("Role");
                this.FullHelp.Properties.Add(new PSNoteProperty("Role", this.Role));
                this.FullHelp.Properties.Remove("Functionality");
                this.FullHelp.Properties.Add(new PSNoteProperty("Functionality", this.Functionality));
            }
        }

        internal virtual string Component
        {
            get
            {
                return string.Empty;
            }
        }

        internal Collection<ErrorRecord> Errors
        {
            get
            {
                return this._errors;
            }
            set
            {
                this._errors = value;
            }
        }

        internal System.Management.Automation.HelpCategory ForwardHelpCategory
        {
            get
            {
                return this._forwardHelpCategory;
            }
            set
            {
                this._forwardHelpCategory = value;
            }
        }

        internal string ForwardTarget
        {
            get
            {
                return this._forwardTarget;
            }
            set
            {
                this._forwardTarget = value;
            }
        }

        internal abstract PSObject FullHelp { get; }

        internal virtual string Functionality
        {
            get
            {
                return string.Empty;
            }
        }

        internal abstract System.Management.Automation.HelpCategory HelpCategory { get; }

        internal abstract string Name { get; }

        internal virtual string Role
        {
            get
            {
                return string.Empty;
            }
        }

        internal PSObject ShortHelp
        {
            get
            {
                if (this.FullHelp == null)
                {
                    return null;
                }
                PSObject obj2 = new PSObject(this.FullHelp);
                obj2.TypeNames.Clear();
                obj2.TypeNames.Add("HelpInfoShort");
                return obj2;
            }
        }

        internal abstract string Synopsis { get; }
    }
}

