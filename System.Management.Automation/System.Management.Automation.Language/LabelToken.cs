namespace System.Management.Automation.Language
{
    using System;

    public class LabelToken : Token
    {
        private readonly string _labelText;

        internal LabelToken(InternalScriptExtent scriptExtent, TokenFlags tokenFlags, string labelText) : base(scriptExtent, TokenKind.Label, tokenFlags)
        {
            this._labelText = labelText;
        }

        public string LabelText
        {
            get
            {
                return this._labelText;
            }
        }
    }
}

