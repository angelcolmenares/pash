namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Management.Automation.Runspaces;

    internal class RemoteSessionStateProxy : SessionStateProxy
    {
        private RemoteRunspace _runspace;
        private Exception getVariableCommandNotFoundException;
        private Exception isInNoLangugeModeException;
        private Exception setVariableCommandNotFoundException;

        internal RemoteSessionStateProxy(RemoteRunspace runspace)
        {
            this._runspace = runspace;
        }

        public override object GetVariable(string name)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (this.getVariableCommandNotFoundException != null)
            {
                throw this.getVariableCommandNotFoundException;
            }
            Pipeline pipeline = this._runspace.CreatePipeline();
            Command item = new Command(@"Microsoft.PowerShell.Utility\Get-Variable");
            item.Parameters.Add("Name", name);
            pipeline.Commands.Add(item);
            Collection<PSObject> collection = null;
            try
            {
                collection = pipeline.Invoke();
            }
            catch (RemoteException exception)
            {
                if (string.Equals("CommandNotFoundException", exception.ErrorRecord.FullyQualifiedErrorId, StringComparison.OrdinalIgnoreCase))
                {
                    this.getVariableCommandNotFoundException = new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, exception);
                    throw this.getVariableCommandNotFoundException;
                }
                throw;
            }
            if (pipeline.Error.Count > 0)
            {
                ErrorRecord record = (ErrorRecord) pipeline.Error.Read();
                if (string.Equals("CommandNotFoundException", record.FullyQualifiedErrorId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, record.Exception);
                }
                throw new PSInvalidOperationException(record.Exception.Message, record.Exception);
            }
            if (collection.Count != 1)
            {
                return null;
            }
            return collection[0].Properties["Value"].Value;
        }

        public override void SetVariable(string name, object value)
        {
            if (name == null)
            {
                throw PSTraceSource.NewArgumentNullException("name");
            }
            if (this.setVariableCommandNotFoundException != null)
            {
                throw this.setVariableCommandNotFoundException;
            }
            Pipeline pipeline = this._runspace.CreatePipeline();
            Command item = new Command(@"Microsoft.PowerShell.Utility\Set-Variable");
            item.Parameters.Add("Name", name);
            item.Parameters.Add("Value", value);
            pipeline.Commands.Add(item);
            try
            {
                pipeline.Invoke();
            }
            catch (RemoteException exception)
            {
                if (string.Equals("CommandNotFoundException", exception.ErrorRecord.FullyQualifiedErrorId, StringComparison.OrdinalIgnoreCase))
                {
                    this.setVariableCommandNotFoundException = new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, exception);
                    throw this.setVariableCommandNotFoundException;
                }
                throw;
            }
            if (pipeline.Error.Count > 0)
            {
                ErrorRecord record = (ErrorRecord) pipeline.Error.Read();
                throw new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, record.Exception);
            }
        }

        public override List<string> Applications
        {
            get
            {
                if (this.isInNoLangugeModeException != null)
                {
                    throw this.isInNoLangugeModeException;
                }
                Pipeline pipeline = this._runspace.CreatePipeline();
                pipeline.Commands.AddScript("$executionContext.SessionState.Applications");
                List<string> list = new List<string>();
                try
                {
                    foreach (PSObject obj2 in pipeline.Invoke())
                    {
                        list.Add(obj2.BaseObject as string);
                    }
                }
                catch (RemoteException exception)
                {
                    if (exception.ErrorRecord.CategoryInfo.Category == ErrorCategory.ParserError)
                    {
                        this.isInNoLangugeModeException = new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, exception);
                        throw this.isInNoLangugeModeException;
                    }
                    throw;
                }
                return list;
            }
        }

        public override DriveManagementIntrinsics Drive
        {
            get
            {
                throw new PSNotSupportedException();
            }
        }

        public override CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                throw new PSNotSupportedException();
            }
        }

        public override ProviderIntrinsics InvokeProvider
        {
            get
            {
                throw new PSNotSupportedException();
            }
        }

        public override PSLanguageMode LanguageMode
        {
            get
            {
                if (this.isInNoLangugeModeException != null)
                {
                    return PSLanguageMode.NoLanguage;
                }
                Pipeline pipeline = this._runspace.CreatePipeline();
                pipeline.Commands.AddScript("$executionContext.SessionState.LanguageMode");
                Collection<PSObject> collection = null;
                try
                {
                    collection = pipeline.Invoke();
                }
                catch (RemoteException exception)
                {
                    if (exception.ErrorRecord.CategoryInfo.Category != ErrorCategory.ParserError)
                    {
                        throw;
                    }
                    this.isInNoLangugeModeException = new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, exception);
                    return PSLanguageMode.NoLanguage;
                }
                return (PSLanguageMode) LanguagePrimitives.ConvertTo(collection[0], typeof(PSLanguageMode), CultureInfo.InvariantCulture);
            }
            set
            {
                throw new PSNotSupportedException();
            }
        }

        public override PSModuleInfo Module
        {
            get
            {
                throw new PSNotSupportedException();
            }
        }

        public override PathIntrinsics Path
        {
            get
            {
                throw new PSNotSupportedException();
            }
        }

        public override CmdletProviderManagementIntrinsics Provider
        {
            get
            {
                throw new PSNotSupportedException();
            }
        }

        public override PSVariableIntrinsics PSVariable
        {
            get
            {
                throw new PSNotSupportedException();
            }
        }

        public override List<string> Scripts
        {
            get
            {
                if (this.isInNoLangugeModeException != null)
                {
                    throw this.isInNoLangugeModeException;
                }
                Pipeline pipeline = this._runspace.CreatePipeline();
                pipeline.Commands.AddScript("$executionContext.SessionState.Scripts");
                List<string> list = new List<string>();
                try
                {
                    foreach (PSObject obj2 in pipeline.Invoke())
                    {
                        list.Add(obj2.BaseObject as string);
                    }
                }
                catch (RemoteException exception)
                {
                    if (exception.ErrorRecord.CategoryInfo.Category == ErrorCategory.ParserError)
                    {
                        this.isInNoLangugeModeException = new PSNotSupportedException(RunspaceStrings.NotSupportedOnRestrictedRunspace, exception);
                        throw this.isInNoLangugeModeException;
                    }
                    throw;
                }
                return list;
            }
        }
    }
}

