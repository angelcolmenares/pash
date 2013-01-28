namespace System.Management.Automation.Language
{
    using System;

    public class InputRedirectionToken : RedirectionToken
    {
        internal InputRedirectionToken(InternalScriptExtent scriptExtent) : base(scriptExtent, TokenKind.RedirectInStd)
        {
        }
    }
}

