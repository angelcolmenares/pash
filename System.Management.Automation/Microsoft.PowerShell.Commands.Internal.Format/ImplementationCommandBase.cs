namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal class ImplementationCommandBase : IDisposable
    {
        private Microsoft.PowerShell.Commands.Internal.Format.TerminatingErrorContext _terminatingErrorContext;
        internal InputObjectCallback InputObjectCall;
        internal OuterCmdletCallback OuterCmdletCall;
        internal WriteObjectCallback WriteObjectCall;

        internal virtual void BeginProcessing()
        {
        }

        internal void CreateTerminatingErrorContext()
        {
            this._terminatingErrorContext = new Microsoft.PowerShell.Commands.Internal.Format.TerminatingErrorContext(this.OuterCmdlet());
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.InternalDispose();
            }
        }

        internal virtual void EndProcessing()
        {
        }

        protected virtual void InternalDispose()
        {
        }

        internal virtual PSCmdlet OuterCmdlet()
        {
            return this.OuterCmdletCall();
        }

        internal virtual void ProcessRecord()
        {
        }

        internal virtual PSObject ReadObject()
        {
            return this.InputObjectCall();
        }

        internal virtual void StopProcessing()
        {
        }

        internal virtual void WriteObject(object o)
        {
            this.WriteObjectCall(o);
        }

        protected Microsoft.PowerShell.Commands.Internal.Format.TerminatingErrorContext TerminatingErrorContext
        {
            get
            {
                return this._terminatingErrorContext;
            }
        }

        internal delegate PSObject InputObjectCallback();

        internal delegate PSCmdlet OuterCmdletCallback();

        internal delegate void WriteObjectCallback(object o);
    }
}

