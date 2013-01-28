namespace System.Management.Automation.Language
{
    using System;

    public class StringLiteralToken : StringToken
    {
        internal StringLiteralToken(InternalScriptExtent scriptExtent, TokenFlags flags, TokenKind tokenKind, string value) : base(scriptExtent, tokenKind, flags, value)
        {
        }
    }
}

