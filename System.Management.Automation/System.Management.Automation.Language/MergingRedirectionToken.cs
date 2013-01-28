namespace System.Management.Automation.Language
{
    using System;
    using System.Runtime.CompilerServices;

    public class MergingRedirectionToken : RedirectionToken
    {
        internal MergingRedirectionToken(InternalScriptExtent scriptExtent, RedirectionStream from, RedirectionStream to) : base(scriptExtent, TokenKind.Redirection)
        {
            this.FromStream = from;
            this.ToStream = to;
        }

        public RedirectionStream FromStream { get; private set; }

        public RedirectionStream ToStream { get; private set; }
    }
}

