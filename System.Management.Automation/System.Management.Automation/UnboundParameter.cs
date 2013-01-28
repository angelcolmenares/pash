namespace System.Management.Automation
{
    using System;

    internal sealed class UnboundParameter
    {
        private static readonly object _singletonValue = new object();

        private UnboundParameter()
        {
        }

        internal static object Value
        {
            get
            {
                return _singletonValue;
            }
        }
    }
}

