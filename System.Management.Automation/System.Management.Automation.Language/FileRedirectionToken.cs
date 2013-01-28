namespace System.Management.Automation.Language
{
    using System;
    using System.Runtime.CompilerServices;

    public class FileRedirectionToken : RedirectionToken
    {
        internal FileRedirectionToken(InternalScriptExtent scriptExtent, RedirectionStream from, bool append) : base(scriptExtent, TokenKind.Redirection)
        {
            this.FromStream = from;
            this.Append = append;
        }

        public bool Append { get; private set; }

        public RedirectionStream FromStream { get; private set; }
    }
}

