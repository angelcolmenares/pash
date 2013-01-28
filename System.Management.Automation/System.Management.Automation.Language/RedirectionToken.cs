namespace System.Management.Automation.Language
{
    using System;

    public abstract class RedirectionToken : Token
    {
        internal RedirectionToken(InternalScriptExtent scriptExtent, TokenKind kind) : base(scriptExtent, kind, TokenFlags.None)
        {
        }
    }
}

