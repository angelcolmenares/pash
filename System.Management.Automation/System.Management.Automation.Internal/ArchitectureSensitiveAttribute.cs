namespace System.Management.Automation.Internal
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
    internal class ArchitectureSensitiveAttribute : Attribute
    {
        internal ArchitectureSensitiveAttribute()
        {
        }
    }
}

