namespace Microsoft.PowerShell
{
    using System;
    using System.ComponentModel;
    using System.Management.Automation;

    [RunInstaller(true)]
    public sealed class PSCorePSSnapIn : PSSnapIn
    {
        private string[] _formats = new string[] { "Certificate.format.ps1xml", "DotNetTypes.format.ps1xml", "FileSystem.format.ps1xml", "Help.format.ps1xml", "HelpV3.format.ps1xml", "PowerShellCore.format.ps1xml", "PowerShellTrace.format.ps1xml", "Registry.format.ps1xml" };
        private string[] _types = new string[] { "types.ps1xml" };

        public override string Description
        {
            get
            {
                return "This PSSnapIn contains MSH management cmdlets used to manage components affecting the MSH engine.";
            }
        }

        public override string DescriptionResource
        {
            get
            {
                return "CoreMshSnapInResources,Description";
            }
        }

        public override string[] Formats
        {
            get
            {
                return this._formats;
            }
        }

        public override string Name
        {
            get
            {
                return "Microsoft.PowerShell.Core";
            }
        }

        public override string[] Types
        {
            get
            {
                return this._types;
            }
        }

        public override string Vendor
        {
            get
            {
                return "Microsoft";
            }
        }

        public override string VendorResource
        {
            get
            {
                return "CoreMshSnapInResources,Vendor";
            }
        }
    }
}

