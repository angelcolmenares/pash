namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    public class SessionStateProxy
    {
        private RunspaceBase _runspace;

        internal SessionStateProxy()
        {
        }

        internal SessionStateProxy(RunspaceBase runspace)
        {
            this._runspace = runspace;
        }

        public virtual object GetVariable(string name)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (name.Equals(string.Empty))
            {
                return null;
            }
            return this._runspace.GetVariable(name);
        }

        public virtual void SetVariable(string name, object value)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            this._runspace.SetVariable(name, value);
        }

        public virtual List<string> Applications
        {
            get
            {
                return this._runspace.Applications;
            }
        }

        public virtual DriveManagementIntrinsics Drive
        {
            get
            {
                return this._runspace.Drive;
            }
        }

        public virtual CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                return this._runspace.InvokeCommand;
            }
        }

        public virtual ProviderIntrinsics InvokeProvider
        {
            get
            {
                return this._runspace.InvokeProvider;
            }
        }

        public virtual PSLanguageMode LanguageMode
        {
            get
            {
                return this._runspace.LanguageMode;
            }
            set
            {
                this._runspace.LanguageMode = value;
            }
        }

        public virtual PSModuleInfo Module
        {
            get
            {
                return this._runspace.Module;
            }
        }

        public virtual PathIntrinsics Path
        {
            get
            {
                return this._runspace.PathIntrinsics;
            }
        }

        public virtual CmdletProviderManagementIntrinsics Provider
        {
            get
            {
                return this._runspace.Provider;
            }
        }

        public virtual PSVariableIntrinsics PSVariable
        {
            get
            {
                return this._runspace.PSVariable;
            }
        }

        public virtual List<string> Scripts
        {
            get
            {
                return this._runspace.Scripts;
            }
        }
    }
}

