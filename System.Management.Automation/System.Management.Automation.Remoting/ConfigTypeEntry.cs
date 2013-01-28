namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal class ConfigTypeEntry
    {
        internal string Key;
        internal TypeValidationCallback ValidationCallback;

        internal ConfigTypeEntry(string key, TypeValidationCallback callback)
        {
            this.Key = key;
            this.ValidationCallback = callback;
        }

        internal delegate bool TypeValidationCallback(string key, object obj, PSCmdlet cmdlet, string path);
    }
}

