namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.ComponentModel;
    using System.Management.Automation;

    [RunInstaller(true)]
    public class GetEventPSSnapIn : PSSnapIn
    {
        private string[] _formats = new string[] { "Event.format.ps1xml" };
        private string[] _types = new string[] { "getevent.types.ps1xml" };

        public override string Description
        {
            get
            {
                return "This PS snap-in contains Get-WinEvent cmdlet used to read Windows event log data and configuration.";
            }
        }

        public override string DescriptionResource
        {
            get
            {
                return "GetEventResources,Description";
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
                return "Microsoft.Powershell.GetEvent";
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
                return "GetEventResources,Vendor";
            }
        }
    }
}

