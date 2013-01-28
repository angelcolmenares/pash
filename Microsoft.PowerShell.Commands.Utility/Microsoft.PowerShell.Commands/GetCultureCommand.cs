namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(CultureInfo) }), Cmdlet("Get", "Culture", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113312")]
    public sealed class GetCultureCommand : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            base.WriteObject(base.Host.CurrentCulture);
        }
    }
}

