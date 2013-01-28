namespace Microsoft.PowerShell.Commands
{
    using Microsoft.PowerShell.Commands.Utility;
    using System;
    using System.Management.Automation;

    [OutputType(new Type[] { typeof(PSEventJob) }), Cmdlet("Register", "EngineEvent", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135243")]
    public class RegisterEngineEventCommand : ObjectEventRegistrationBase
    {
        protected override object GetSourceObject()
        {
            if ((base.Action == null) && (base.Forward == 0))
            {
                ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(EventingStrings.ActionMandatoryForLocal), "ACTION_MANDATORY_FOR_LOCAL", ErrorCategory.InvalidArgument, null);
                base.ThrowTerminatingError(errorRecord);
            }
            return null;
        }

        protected override string GetSourceObjectEventName()
        {
            return null;
        }

        [Parameter(Mandatory=true, Position=100)]
        public string SourceIdentifier
        {
            get
            {
                return base.SourceIdentifier;
            }
            set
            {
                base.SourceIdentifier = value;
            }
        }
    }
}

