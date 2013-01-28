namespace Microsoft.PowerShell.Cmdletization
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;

    public abstract class CmdletAdapter<TObjectInstance> where TObjectInstance: class
    {
        private string className;
        private string classVersion;
        private PSCmdlet cmdlet;
        private Version moduleVersion;
        private IDictionary<string, string> privateData;

        protected CmdletAdapter()
        {
        }

        public virtual void BeginProcessing()
        {
        }

        public virtual void EndProcessing()
        {
        }

        public virtual QueryBuilder GetQueryBuilder()
        {
            throw new NotImplementedException();
        }

        internal void Initialize(PSCmdlet cmdlet, string className, string classVersion, IDictionary<string, string> privateData)
        {
            EventHandler handler = null;
            EventHandler handler2 = null;
            if (cmdlet == null)
            {
                throw new ArgumentNullException("cmdlet");
            }
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentNullException("className");
            }
            if (classVersion == null)
            {
                throw new ArgumentNullException("classVersion");
            }
            if (privateData == null)
            {
                throw new ArgumentNullException("privateData");
            }
            this.cmdlet = cmdlet;
            this.className = className;
            this.classVersion = classVersion;
            this.privateData = privateData;
            PSScriptCmdlet cmdlet2 = this.Cmdlet as PSScriptCmdlet;
            if (cmdlet2 != null)
            {
                if (handler == null)
                {
                    handler = (param0, param1) => this.StopProcessing();
                }
                cmdlet2.StoppingEvent += handler;
                if (handler2 == null)
                {
                    handler2 = delegate (object param0, EventArgs param1) {
                        IDisposable disposable = this as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    };
                }
                cmdlet2.DisposingEvent += handler2;
            }
        }

        public void Initialize(PSCmdlet cmdlet, string className, string classVersion, Version moduleVersion, IDictionary<string, string> privateData)
        {
            this.moduleVersion = moduleVersion;
            this.Initialize(cmdlet, className, classVersion, privateData);
        }

        public virtual void ProcessRecord(MethodInvocationInfo methodInvocationInfo)
        {
            throw new NotImplementedException();
        }

        public virtual void ProcessRecord(QueryBuilder query)
        {
            throw new NotImplementedException();
        }

        public virtual void ProcessRecord(QueryBuilder query, MethodInvocationInfo methodInvocationInfo, bool passThru)
        {
            throw new NotImplementedException();
        }

        public virtual void ProcessRecord(TObjectInstance objectInstance, MethodInvocationInfo methodInvocationInfo, bool passThru)
        {
            throw new NotImplementedException();
        }

        public virtual void StopProcessing()
        {
        }

        public string ClassName
        {
            get
            {
                return this.className;
            }
        }

        public string ClassVersion
        {
            get
            {
                return this.classVersion;
            }
        }

        public PSCmdlet Cmdlet
        {
            get
            {
                return this.cmdlet;
            }
        }

        public Version ModuleVersion
        {
            get
            {
                return this.moduleVersion;
            }
        }

        public IDictionary<string, string> PrivateData
        {
            get
            {
                return this.privateData;
            }
        }
    }
}

