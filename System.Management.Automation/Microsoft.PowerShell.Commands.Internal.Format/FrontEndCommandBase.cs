namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public abstract class FrontEndCommandBase : PSCmdlet, IDisposable
    {
        private PSObject _inputObject = AutomationNull.Value;
        internal ImplementationCommandBase implementation;

        protected FrontEndCommandBase()
        {
        }

        protected override void BeginProcessing()
        {
            this.implementation.OuterCmdletCall = new ImplementationCommandBase.OuterCmdletCallback(this.OuterCmdletCall);
            this.implementation.InputObjectCall = new ImplementationCommandBase.InputObjectCallback(this.InputObjectCall);
            this.implementation.WriteObjectCall = new ImplementationCommandBase.WriteObjectCallback(this.WriteObjectCall);
            this.implementation.CreateTerminatingErrorContext();
            this.implementation.BeginProcessing();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.InternalDispose();
            }
        }

        protected override void EndProcessing()
        {
            this.implementation.EndProcessing();
        }

        protected virtual PSObject InputObjectCall()
        {
            return this.InputObject;
        }

        protected virtual void InternalDispose()
        {
            if (this.implementation != null)
            {
                this.implementation.Dispose();
                this.implementation = null;
            }
        }

        protected virtual PSCmdlet OuterCmdletCall()
        {
            return this;
        }

        protected override void ProcessRecord()
        {
            this.implementation.ProcessRecord();
        }

        protected override void StopProcessing()
        {
            this.implementation.StopProcessing();
        }

        protected virtual void WriteObjectCall(object value)
        {
            base.WriteObject(value);
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this._inputObject;
            }
            set
            {
                this._inputObject = value;
            }
        }
    }
}

