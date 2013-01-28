namespace System.Management.Automation
{
    using System;

    internal class SyntaxHelpInfo : BaseCommandHelpInfo
    {
        private PSObject _fullHelpObject;
        private string _name;
        private string _synopsis;

        private SyntaxHelpInfo(string name, string text, HelpCategory category) : base(category)
        {
            this._name = "";
            this._synopsis = "";
            this._fullHelpObject = PSObject.AsPSObject(text);
            this._name = name;
            this._synopsis = text;
        }

        internal static SyntaxHelpInfo GetHelpInfo(string name, string text, HelpCategory category)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            SyntaxHelpInfo info = new SyntaxHelpInfo(name, text, category);
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

