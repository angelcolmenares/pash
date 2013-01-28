namespace System.Management.Automation.Provider
{
    using System;
    using System.Management.Automation;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class CmdletProviderAttribute : Attribute
    {
        private char[] illegalCharacters = new char[] { ':', '\\', '[', ']', '?', '*' };
        private string provider = string.Empty;
        private System.Management.Automation.Provider.ProviderCapabilities providerCapabilities;

        public CmdletProviderAttribute(string providerName, System.Management.Automation.Provider.ProviderCapabilities providerCapabilities)
        {
            if (string.IsNullOrEmpty(providerName))
            {
                throw PSTraceSource.NewArgumentNullException("providerName");
            }
            if (providerName.IndexOfAny(this.illegalCharacters) != -1)
            {
                throw PSTraceSource.NewArgumentException("providerName", "SessionStateStrings", "ProviderNameNotValid", new object[] { providerName });
            }
            this.provider = providerName;
            this.providerCapabilities = providerCapabilities;
        }

        public System.Management.Automation.Provider.ProviderCapabilities ProviderCapabilities
        {
            get
            {
                return this.providerCapabilities;
            }
        }

        public string ProviderName
        {
            get
            {
                return this.provider;
            }
        }
    }
}

