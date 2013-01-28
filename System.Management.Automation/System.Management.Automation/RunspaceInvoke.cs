namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    public class RunspaceInvoke : IDisposable
    {
        private bool _disposed;
        private Runspace _runspace;

        public RunspaceInvoke()
        {
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();
            this._runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            this._runspace.Open();
            if (Runspace.DefaultRunspace == null)
            {
                Runspace.DefaultRunspace = this._runspace;
            }
        }

        public RunspaceInvoke(Runspace runspace)
        {
            if (runspace == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspace");
            }
            this._runspace = runspace;
            if (Runspace.DefaultRunspace == null)
            {
                Runspace.DefaultRunspace = this._runspace;
            }
        }

        public RunspaceInvoke(RunspaceConfiguration runspaceConfiguration)
        {
            if (runspaceConfiguration == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspaceConfiguration");
            }
            this._runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            this._runspace.Open();
            if (Runspace.DefaultRunspace == null)
            {
                Runspace.DefaultRunspace = this._runspace;
            }
        }

        public RunspaceInvoke(string consoleFilePath)
        {
            PSConsoleLoadException exception;
            if (consoleFilePath == null)
            {
                throw PSTraceSource.NewArgumentNullException("consoleFilePath");
            }
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create(consoleFilePath, out exception);
            if (exception != null)
            {
                throw exception;
            }
            this._runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            this._runspace.Open();
            if (Runspace.DefaultRunspace == null)
            {
                Runspace.DefaultRunspace = this._runspace;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed && disposing)
            {
                this._runspace.Close();
                this._runspace = null;
            }
            this._disposed = true;
        }

        public Collection<PSObject> Invoke(string script)
        {
            return this.Invoke(script, null);
        }

        public Collection<PSObject> Invoke(string script, IEnumerable input)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("runspace");
            }
            if (script == null)
            {
                throw PSTraceSource.NewArgumentNullException("script");
            }
            return this._runspace.CreatePipeline(script).Invoke(input);
        }

        public Collection<PSObject> Invoke(string script, IEnumerable input, out IList errors)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewObjectDisposedException("runspace");
            }
            if (script == null)
            {
                throw PSTraceSource.NewArgumentNullException("script");
            }
            Pipeline pipeline = this._runspace.CreatePipeline(script);
            Collection<PSObject> collection = pipeline.Invoke(input);
            errors = pipeline.Error.NonBlockingRead();
            return collection;
        }
    }
}

