namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class FileSystemProviderGetItemDynamicParameters
    {
        [ValidateNotNullOrEmpty, Parameter]
        public string[] Stream { get; set; }
    }
}

