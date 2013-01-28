namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class PSDefaultValueAttribute : ParsingBaseAttribute
    {
        public string Help { get; set; }

        public object Value { get; set; }
    }
}

