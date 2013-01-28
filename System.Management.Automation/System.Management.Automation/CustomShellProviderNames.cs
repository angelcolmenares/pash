namespace System.Management.Automation
{
    using System;

    internal class CustomShellProviderNames : ProviderNames
    {
        internal override string Alias
        {
            get
            {
                return "Alias";
            }
        }

        internal override string Certificate
        {
            get
            {
                return "Certificate";
            }
        }

        internal override string Environment
        {
            get
            {
                return "Environment";
            }
        }

        internal override string FileSystem
        {
            get
            {
                return "FileSystem";
            }
        }

        internal override string Function
        {
            get
            {
                return "Function";
            }
        }

        internal override string Registry
        {
            get
            {
                return "Registry";
            }
        }

        internal override string Variable
        {
            get
            {
                return "Variable";
            }
        }
    }
}

