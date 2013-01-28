namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal sealed class OutputManagerInner : ImplementationCommandBase
    {
        private bool isStopped;
        private Microsoft.PowerShell.Commands.Internal.Format.LineOutput lo;
        private SubPipelineManager mgr;
        private object syncRoot = new object();
        [TraceSource("format_out_OutputManagerInner", "OutputManagerInner")]
        internal static PSTraceSource tracer = PSTraceSource.GetTracer("format_out_OutputManagerInner", "OutputManagerInner");

        internal override void EndProcessing()
        {
            if (this.mgr != null)
            {
                this.mgr.ShutDown();
            }
        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            if (this.mgr != null)
            {
                this.mgr.Dispose();
                this.mgr = null;
            }
        }

        internal override void ProcessRecord()
        {
            PSObject so = this.ReadObject();
            if ((so != null) && (so != AutomationNull.Value))
            {
                if (this.mgr == null)
                {
                    this.mgr = new SubPipelineManager();
                    this.mgr.Initialize(this.lo, this.OuterCmdlet().Context);
                }
                this.mgr.Process(so);
            }
        }

        internal override void StopProcessing()
        {
            lock (this.syncRoot)
            {
                if (this.lo != null)
                {
                    this.lo.StopProcessing();
                }
                this.isStopped = true;
            }
        }

        internal Microsoft.PowerShell.Commands.Internal.Format.LineOutput LineOutput
        {
            set
            {
                lock (this.syncRoot)
                {
                    this.lo = value;
                    if (this.isStopped)
                    {
                        this.lo.StopProcessing();
                    }
                }
            }
        }
    }
}

