namespace System.Management.Automation
{
    using System;

    internal class SingleShellProviderNames : ProviderNames
    {
        internal override string Alias
        {
            get
            {
                return @"Microsoft.PowerShell.Core\Alias";
            }
        }

        internal override string Certificate
        {
            get
            {
                return @"Microsoft.PowerShell.Security\Certificate";
            }
        }

        internal override string Environment
        {
            get
            {
                return @"Microsoft.PowerShell.Core\Environment";
            }
        }

        internal override string FileSystem
        {
            get
            {
                return @"Microsoft.PowerShell.Core\FileSystem";
            }
        }

        internal override string Function
        {
            get
            {
                return @"Microsoft.PowerShell.Core\Function";
            }
        }

        internal override string Registry
        {
            get
            {
                return @"Microsoft.PowerShell.Core\Registry";
            }
        }

        internal override string Variable
        {
            get
            {
                return @"Microsoft.PowerShell.Core\Variable";
            }
        }
    }
}

