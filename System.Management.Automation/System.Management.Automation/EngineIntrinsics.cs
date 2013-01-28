namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Host;

    public class EngineIntrinsics
    {
        private ExecutionContext _context;
        private PSHost _host;
        private CommandInvocationIntrinsics _invokeCommand;

        private EngineIntrinsics()
        {
        }

        internal EngineIntrinsics(ExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this._context = context;
            this._host = context.EngineHostInterface;
        }

        public PSEventManager Events
        {
            get
            {
                return this._context.Events;
            }
        }

        public PSHost Host
        {
            get
            {
                return this._host;
            }
        }

        public CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                if (this._invokeCommand == null)
                {
                    this._invokeCommand = new CommandInvocationIntrinsics(this._context);
                }
                return this._invokeCommand;
            }
        }

        public ProviderIntrinsics InvokeProvider
        {
            get
            {
                return this._context.EngineSessionState.InvokeProvider;
            }
        }

        public System.Management.Automation.SessionState SessionState
        {
            get
            {
                return this._context.EngineSessionState.PublicSessionState;
            }
        }
    }
}

