namespace Microsoft.PowerShell
{
    using System;
    using System.ComponentModel;
    using System.Management.Automation;

    [RunInstaller(true)]
    public sealed class PSUtilityPSSnapIn : PSSnapIn
    {
        public override string Description
        {
            get
            {
                return "This PSSnapIn contains utility cmdlets used to manipulate data.";
            }
        }

        public override string DescriptionResource
        {
            get
            {
                return "UtilityMshSnapInResources,Description";
            }
        }

        public override string Name
        {
            get
            {
                return "Microsoft.PowerShell.Utility";
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
                return "UtilityMshSnapInResources,Vendor";
            }
        }
    }
}

