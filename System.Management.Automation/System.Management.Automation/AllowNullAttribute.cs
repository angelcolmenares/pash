namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class AllowNullAttribute : CmdletMetadataAttribute
    {
    }
}

