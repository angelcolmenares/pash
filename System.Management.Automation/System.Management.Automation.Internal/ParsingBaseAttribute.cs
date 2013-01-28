namespace System.Management.Automation.Internal
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public abstract class ParsingBaseAttribute : CmdletMetadataAttribute
    {
        internal ParsingBaseAttribute()
        {
        }
    }
}

