namespace System.Management.Automation
{
    using System;
    using System.IO;

    internal class HelpFileHelpInfo : HelpInfo
    {
        private string _filename = "";
        private PSObject _fullHelpObject;
        private string _name = "";
        private string _synopsis = "";

        private HelpFileHelpInfo(string name, string text, string filename)
        {
            this._fullHelpObject = PSObject.AsPSObject(text);
            this._name = name;
            this._synopsis = GetLine(text, 5);
            if (this._synopsis != null)
            {
                this._synopsis = this._synopsis.Trim();
            }
            else
            {
                this._synopsis = "";
            }
            this._filename = filename;
        }

        internal static HelpFileHelpInfo GetHelpInfo(string name, string text, string filename)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            HelpFileHelpInfo info = new HelpFileHelpInfo(name, text, filename);
            if (string.IsNullOrEmpty(info.Name))
            {
                return null;
            }
            info.AddCommonHelpProperties();
            return info;
        }

        private static string GetLine(string text, int line)
        {
            StringReader reader = new StringReader(text);
            string str = null;
            for (int i = 0; i < line; i++)
            {
                str = reader.ReadLine();
                if (str == null)
                {
                    return null;
                }
            }
            return str;
        }

        internal override bool MatchPatternInContent(WildcardPattern pattern)
        {
            string result = string.Empty;
            LanguagePrimitives.TryConvertTo<string>(this.FullHelp, out result);
            return pattern.IsMatch(result);
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
                return System.Management.Automation.HelpCategory.HelpFile;
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

