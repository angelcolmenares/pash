namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public class PSTypeNameAttribute : Attribute
    {
        public PSTypeNameAttribute(string psTypeName)
        {
            if (string.IsNullOrEmpty(psTypeName))
            {
                throw PSTraceSource.NewArgumentException("psTypeName");
            }
            this.PSTypeName = psTypeName;
        }

        public string PSTypeName { get; private set; }
    }
}

