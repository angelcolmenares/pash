namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Host;

    internal class ExecutionContextForStepping : IDisposable
    {
        private ExecutionContext executionContext;
        private PSHost originalHost;
        private PSInformationalBuffers originalInformationalBuffers;

        private ExecutionContextForStepping(ExecutionContext ctxt)
        {
            this.executionContext = ctxt;
        }

        internal static ExecutionContextForStepping PrepareExecutionContext(ExecutionContext ctxt, PSInformationalBuffers newBuffers, PSHost newHost)
        {
            ExecutionContextForStepping stepping = new ExecutionContextForStepping(ctxt) {
                originalInformationalBuffers = ctxt.InternalHost.InternalUI.GetInformationalMessageBuffers(),
                originalHost = ctxt.InternalHost.ExternalHost
            };
            ctxt.InternalHost.InternalUI.SetInformationalMessageBuffers(newBuffers);
            ctxt.InternalHost.SetHostRef(newHost);
            return stepping;
        }

        void IDisposable.Dispose()
        {
            this.executionContext.InternalHost.InternalUI.SetInformationalMessageBuffers(this.originalInformationalBuffers);
            this.executionContext.InternalHost.SetHostRef(this.originalHost);
            GC.SuppressFinalize(this);
        }
    }
}

