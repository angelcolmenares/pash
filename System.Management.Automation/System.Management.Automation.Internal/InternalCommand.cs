namespace System.Management.Automation.Internal
{
    using System;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;

    [DebuggerDisplay("Command = {commandInfo}")]
    public abstract class InternalCommand
    {
        private PSHost CBhost;
        private System.Management.Automation.CommandInfo commandInfo;
        internal System.Management.Automation.CommandOrigin CommandOriginInternal = System.Management.Automation.CommandOrigin.Internal;
        internal ICommandRuntime commandRuntime;
        private ExecutionContext context;
        internal PSObject currentObjectInPipeline = AutomationNull.Value;
        private InvocationInfo myInvocation;
        private SessionState state;

        internal InternalCommand()
        {
            this.CommandInfo = null;
        }

        internal virtual void DoBeginProcessing()
        {
        }

        internal virtual void DoEndProcessing()
        {
        }

        internal virtual void DoProcessRecord()
        {
        }

        internal virtual void DoStopProcessing()
        {
        }

        internal void InternalDispose(bool isDisposing)
        {
            this.myInvocation = null;
            this.state = null;
            this.commandInfo = null;
            this.context = null;
        }

        internal void ThrowIfStopping()
        {
            if (this.IsStopping)
            {
                throw new PipelineStoppedException();
            }
        }

        internal System.Management.Automation.CommandInfo CommandInfo
        {
            get
            {
                return this.commandInfo;
            }
            set
            {
                this.commandInfo = value;
            }
        }

        public System.Management.Automation.CommandOrigin CommandOrigin
        {
            get
            {
                return this.CommandOriginInternal;
            }
        }

        internal ExecutionContext Context
        {
            get
            {
                return this.context;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("Context");
                }
                this.context = value;
                this.CBhost = this.context.EngineHostInterface;
                this.state = new SessionState(this.context.EngineSessionState);
            }
        }

        internal PSObject CurrentPipelineObject
        {
            get
            {
                return this.currentObjectInPipeline;
            }
            set
            {
                this.currentObjectInPipeline = value;
            }
        }

        internal SessionState InternalState
        {
            get
            {
                return this.state;
            }
        }

        internal IScriptExtent InvocationExtent { get; set; }

        internal bool IsStopping
        {
            get
            {
                MshCommandRuntime commandRuntime = this.commandRuntime as MshCommandRuntime;
                return ((commandRuntime != null) && commandRuntime.IsStopping);
            }
        }

        internal InvocationInfo MyInvocation
        {
            get
            {
                if (this.myInvocation == null)
                {
                    this.myInvocation = new InvocationInfo(this);
                }
                return this.myInvocation;
            }
        }

        internal PSHost PSHostInternal
        {
            get
            {
                return this.CBhost;
            }
        }
    }
}

