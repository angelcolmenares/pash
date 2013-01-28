namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public class FileSystemItemProviderDynamicParameters
    {
        [Parameter]
        public DateTime? NewerThan { get; set; }

        [Parameter]
        public DateTime? OlderThan { get; set; }
    }
}

