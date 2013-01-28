namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Globalization;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(CultureInfo) }), Cmdlet("Get", "UICulture", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113334")]
    public sealed class GetUICultureCommand : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            base.WriteObject(base.Host.CurrentUICulture);
        }
    }
}

