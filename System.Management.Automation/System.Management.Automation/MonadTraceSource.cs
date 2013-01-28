namespace System.Management.Automation
{
    using System;
    using System.Diagnostics;

    internal class MonadTraceSource : TraceSource
    {
        internal MonadTraceSource(string name) : base(name)
        {
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "Options" };
        }
    }
}

