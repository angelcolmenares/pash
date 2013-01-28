namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;

    [Cmdlet("Register", "ObjectEvent", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135244"), OutputType(new Type[] { typeof(PSEventJob) })]
    public class RegisterObjectEventCommand : ObjectEventRegistrationBase
    {
        private string eventName;
        private PSObject inputObject;

        protected override object GetSourceObject()
        {
            return this.inputObject;
        }

        protected override string GetSourceObjectEventName()
        {
            return this.eventName;
        }

        [Parameter(Mandatory=true, Position=1)]
        public string EventName
        {
            get
            {
                return this.eventName;
            }
            set
            {
                this.eventName = value;
            }
        }

        [Parameter(Mandatory=true, Position=0)]
        public PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }
    }
}

