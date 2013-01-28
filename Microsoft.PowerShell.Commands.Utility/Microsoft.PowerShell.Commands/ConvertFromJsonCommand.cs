namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [Cmdlet("ConvertFrom", "Json", HelpUri="http://go.microsoft.com/fwlink/?LinkID=217031", RemotingCapability=RemotingCapability.None)]
    public class ConvertFromJsonCommand : Cmdlet
    {
        protected override void BeginProcessing()
        {
            try
            {
                Assembly.Load("System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            }
            catch (FileNotFoundException)
            {
                base.ThrowTerminatingError(new ErrorRecord(new NotSupportedException(WebCmdletStrings.ExtendedProfileRequired), "ExtendedProfileRequired", ErrorCategory.NotInstalled, null));
            }
        }

        protected override void ProcessRecord()
        {
            ErrorRecord record;
            object sendToPipeline = JsonObject.ConvertFromJson(this.InputObject, out record);
            if (record != null)
            {
                base.ThrowTerminatingError(record);
            }
            base.WriteObject(sendToPipeline);
        }

        [Parameter(Mandatory=true, Position=0, ValueFromPipeline=true), AllowEmptyString]
        public string InputObject { get; set; }
    }
}

