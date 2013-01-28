namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Host;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    public abstract class PSCmdlet : Cmdlet
    {
        private CommandInvocationIntrinsics _invokeCommand;
        private ProviderIntrinsics invokeProvider;
        private System.Management.Automation.PagingParameters pagingParameters;

        protected PSCmdlet()
        {
        }

        public PathInfo CurrentProviderLocation(string providerId)
        {
            if (providerId == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerId");
            }
            return this.SessionState.Path.CurrentProviderLocation(providerId);
        }

        public Collection<string> GetResolvedProviderPathFromPSPath(string path, out ProviderInfo provider)
        {
            return this.SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider);
        }

        public string GetUnresolvedProviderPathFromPSPath(string path)
        {
            return this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(path);
        }

        public object GetVariableValue(string name)
        {
            return this.SessionState.PSVariable.GetValue(name);
        }

        public object GetVariableValue(string name, object defaultValue)
        {
            return this.SessionState.PSVariable.GetValue(name, defaultValue);
        }

        public PSEventManager Events
        {
            get
            {
                return base.Context.Events;
            }
        }

        internal bool HasDynamicParameters
        {
            get
            {
                return (this is IDynamicParameters);
            }
        }

        public PSHost Host
        {
            get
            {
                return base.PSHostInternal;
            }
        }

        public CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                if (this._invokeCommand == null)
                {
                    this._invokeCommand = new CommandInvocationIntrinsics(base.Context, this);
                }
                return this._invokeCommand;
            }
        }

        public ProviderIntrinsics InvokeProvider
        {
            get
            {
                if (this.invokeProvider == null)
                {
                    this.invokeProvider = new ProviderIntrinsics(this);
                }
                return this.invokeProvider;
            }
        }

        public System.Management.Automation.JobManager JobManager
        {
            get
            {
                return ((LocalRunspace) base.Context.CurrentRunspace).JobManager;
            }
        }

        public System.Management.Automation.JobRepository JobRepository
        {
            get
            {
                return ((LocalRunspace) base.Context.CurrentRunspace).JobRepository;
            }
        }

        public InvocationInfo MyInvocation
        {
            get
            {
                return base.MyInvocation;
            }
        }

        public System.Management.Automation.PagingParameters PagingParameters
        {
            get
            {
                if (!base.CommandInfo.CommandMetadata.SupportsPaging)
                {
                    return null;
                }
                if (this.pagingParameters == null)
                {
                    MshCommandRuntime commandRuntime = base.CommandRuntime as MshCommandRuntime;
                    if (commandRuntime != null)
                    {
                        this.pagingParameters = commandRuntime.PagingParameters ?? new System.Management.Automation.PagingParameters(commandRuntime);
                    }
                }
                return this.pagingParameters;
            }
        }

        public string ParameterSetName
        {
            get
            {
                return base._ParameterSetName;
            }
        }

        internal System.Management.Automation.RunspaceRepository RunspaceRepository
        {
            get
            {
                return ((LocalRunspace) base.Context.CurrentRunspace).RunspaceRepository;
            }
        }

        public System.Management.Automation.SessionState SessionState
        {
            get
            {
                return base.InternalState;
            }
        }
    }
}

