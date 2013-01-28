namespace System.Management.Automation.Internal.Host
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;
    using System.Runtime.InteropServices;

    internal class InternalHost : PSHost, IHostSupportsInteractiveSession
    {
        private Stack<PromptContextData> contextStack = new Stack<PromptContextData>();
        private const string EnterExitNestedPromptOutOfSyncResource = "EnterExitNestedPromptOutOfSync";
        private ExecutionContext executionContext;
        private const string ExitNonExistentNestedPromptErrorResource = "ExitNonExistentNestedPromptError";
        private ObjectRef<PSHost> externalHostRef;
        private Guid idResult;
        private ObjectRef<InternalHostUserInterface> internalUIRef;
        private string nameResult;
        private int nestedPromptCount;
        private const string StringsBaseName = "InternalHostStrings";
        private System.Version versionResult;
        private readonly Guid zeroGuid;

        internal InternalHost(PSHost externalHost, ExecutionContext executionContext)
        {
            this.externalHostRef = new ObjectRef<PSHost>(externalHost);
            this.executionContext = executionContext;
            PSHostUserInterface uI = externalHost.UI;
            this.internalUIRef = new ObjectRef<InternalHostUserInterface>(new InternalHostUserInterface(uI, this));
            this.zeroGuid = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            this.idResult = this.zeroGuid;
        }

        public override void EnterNestedPrompt()
        {
            this.EnterNestedPrompt(null);
        }

        internal void EnterNestedPrompt(InternalCommand callingCommand)
        {
            LocalRunspace runspace = null;
            try
            {
                runspace = this.Runspace as LocalRunspace;
            }
            catch (PSNotImplementedException)
            {
            }
            if (runspace != null)
            {
                Pipeline currentlyRunningPipeline = this.Runspace.GetCurrentlyRunningPipeline();
                if ((currentlyRunningPipeline != null) && (currentlyRunningPipeline == runspace.PulsePipeline))
                {
                    throw new InvalidOperationException();
                }
            }
            if (this.nestedPromptCount < 0)
            {
                throw PSTraceSource.NewInvalidOperationException("InternalHostStrings", "EnterExitNestedPromptOutOfSync", new object[0]);
            }
            this.nestedPromptCount++;
            this.executionContext.SetVariable(SpecialVariables.NestedPromptCounterVarPath, this.nestedPromptCount);
            PromptContextData item = new PromptContextData {
                SavedContextData = this.executionContext.SaveContextData(),
                SavedCurrentlyExecutingCommandVarValue = this.executionContext.GetVariableValue(SpecialVariables.CurrentlyExecutingCommandVarPath),
                SavedPSBoundParametersVarValue = this.executionContext.GetVariableValue(SpecialVariables.PSBoundParametersVarPath),
                RunspaceAvailability = this.Context.CurrentRunspace.RunspaceAvailability,
                LanguageMode = this.executionContext.LanguageMode
            };
            PSPropertyInfo info = null;
            PSPropertyInfo info2 = null;
            object obj2 = null;
            object obj3 = null;
            if (callingCommand != null)
            {
                PSObject newValue = PSObject.AsPSObject(callingCommand);
                info = newValue.Properties["CommandInfo"];
                if (info == null)
                {
                    newValue.Properties.Add(new PSNoteProperty("CommandInfo", callingCommand.CommandInfo));
                }
                else
                {
                    obj2 = info.Value;
                    info.Value = callingCommand.CommandInfo;
                }
                info2 = newValue.Properties["StackTrace"];
                if (info2 == null)
                {
                    newValue.Properties.Add(new PSNoteProperty("StackTrace", new StackTrace()));
                }
                else
                {
                    obj3 = info2.Value;
                    info2.Value = new StackTrace();
                }
                this.executionContext.SetVariable(SpecialVariables.CurrentlyExecutingCommandVarPath, newValue);
            }
            this.contextStack.Push(item);
            this.executionContext.PSDebugTraceStep = false;
            this.executionContext.PSDebugTraceLevel = 0;
            this.executionContext.ResetShellFunctionErrorOutputPipe();
            if (this.executionContext.HasRunspaceEverUsedConstrainedLanguageMode)
            {
                this.executionContext.LanguageMode = PSLanguageMode.ConstrainedLanguage;
            }
            this.Context.CurrentRunspace.UpdateRunspaceAvailability(RunspaceAvailability.AvailableForNestedCommand, true);
            try
            {
                this.externalHostRef.Value.EnterNestedPrompt();
            }
            catch
            {
                this.ExitNestedPromptHelper();
                throw;
            }
            finally
            {
                if (info != null)
                {
                    info.Value = obj2;
                }
                if (info2 != null)
                {
                    info2.Value = obj3;
                }
            }
        }

        public override void ExitNestedPrompt()
        {
            if (this.nestedPromptCount != 0)
            {
                try
                {
                    this.externalHostRef.Value.ExitNestedPrompt();
                }
                finally
                {
                    this.ExitNestedPromptHelper();
                }
                ExitNestedPromptException exception = new ExitNestedPromptException();
                throw exception;
            }
        }

        private void ExitNestedPromptHelper()
        {
            this.nestedPromptCount--;
            this.executionContext.SetVariable(SpecialVariables.NestedPromptCounterVarPath, this.nestedPromptCount);
            if (this.contextStack.Count > 0)
            {
                PromptContextData data = this.contextStack.Pop();
                data.SavedContextData.RestoreContextData(this.executionContext);
                this.executionContext.LanguageMode = data.LanguageMode;
                this.executionContext.SetVariable(SpecialVariables.CurrentlyExecutingCommandVarPath, data.SavedCurrentlyExecutingCommandVarValue);
                this.executionContext.SetVariable(SpecialVariables.PSBoundParametersVarPath, data.SavedPSBoundParametersVarValue);
                this.Context.CurrentRunspace.UpdateRunspaceAvailability(data.RunspaceAvailability, true);
            }
        }

        private IHostSupportsInteractiveSession GetIHostSupportsInteractiveSession()
        {
            IHostSupportsInteractiveSession session = this.externalHostRef.Value as IHostSupportsInteractiveSession;
            if (session == null)
            {
                throw new PSNotImplementedException();
            }
            return session;
        }

        internal bool HostInNestedPrompt()
        {
            return (this.nestedPromptCount > 0);
        }

        public override void NotifyBeginApplication()
        {
            this.externalHostRef.Value.NotifyBeginApplication();
        }

        public override void NotifyEndApplication()
        {
            this.externalHostRef.Value.NotifyEndApplication();
        }

        public void PopRunspace()
        {
            this.GetIHostSupportsInteractiveSession().PopRunspace();
        }

        public void PushRunspace(System.Management.Automation.Runspaces.Runspace runspace)
        {
            this.GetIHostSupportsInteractiveSession().PushRunspace(runspace);
        }

        internal void RevertHostRef()
        {
            if (this.IsHostRefSet)
            {
                this.externalHostRef.Revert();
                this.internalUIRef.Revert();
            }
        }

        internal void SetHostRef(PSHost psHost)
        {
            this.externalHostRef.Override(psHost);
            this.internalUIRef.Override(new InternalHostUserInterface(psHost.UI, this));
        }

        public override void SetShouldExit(int exitCode)
        {
            this.externalHostRef.Value.SetShouldExit(exitCode);
        }

        internal ExecutionContext Context
        {
            get
            {
                return this.executionContext;
            }
        }

        public override CultureInfo CurrentCulture
        {
            get
            {
                CultureInfo currentCulture = this.externalHostRef.Value.CurrentCulture;
                if (currentCulture == null)
                {
                    currentCulture = CultureInfo.InvariantCulture;
                }
                return currentCulture;
            }
        }

        public override CultureInfo CurrentUICulture
        {
            get
            {
                CultureInfo currentUICulture = this.externalHostRef.Value.CurrentUICulture;
                if (currentUICulture == null)
                {
                    currentUICulture = CultureInfo.InstalledUICulture;
                }
                return currentUICulture;
            }
        }

        internal PSHost ExternalHost
        {
            get
            {
                return this.externalHostRef.Value;
            }
        }

        public override Guid InstanceId
        {
            get
            {
                if (this.idResult == this.zeroGuid)
                {
                    this.idResult = this.externalHostRef.Value.InstanceId;
                    if (this.idResult == this.zeroGuid)
                    {
                        throw PSTraceSource.NewNotImplementedException();
                    }
                }
                return this.idResult;
            }
        }

        internal InternalHostUserInterface InternalUI
        {
            get
            {
                return this.internalUIRef.Value;
            }
        }

        internal bool IsHostRefSet
        {
            get
            {
                return this.externalHostRef.IsOverridden;
            }
        }

        public bool IsRunspacePushed
        {
            get
            {
                return this.GetIHostSupportsInteractiveSession().IsRunspacePushed;
            }
        }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this.nameResult))
                {
                    this.nameResult = this.externalHostRef.Value.Name;
                    if (string.IsNullOrEmpty(this.nameResult))
                    {
                        throw PSTraceSource.NewNotImplementedException();
                    }
                }
                return this.nameResult;
            }
        }

        internal int NestedPromptCount
        {
            get
            {
                return this.nestedPromptCount;
            }
        }

        public override PSObject PrivateData
        {
            get
            {
                return this.externalHostRef.Value.PrivateData;
            }
        }

        public System.Management.Automation.Runspaces.Runspace Runspace
        {
            get
            {
                return this.GetIHostSupportsInteractiveSession().Runspace;
            }
        }

        public override PSHostUserInterface UI
        {
            get
            {
                return this.internalUIRef.Value;
            }
        }

        public override System.Version Version
        {
            get
            {
                if (this.versionResult == null)
                {
                    this.versionResult = this.externalHostRef.Value.Version;
                    if (this.versionResult == null)
                    {
                        throw PSTraceSource.NewNotImplementedException();
                    }
                }
                return this.versionResult;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PromptContextData
        {
            public object SavedCurrentlyExecutingCommandVarValue;
            public object SavedPSBoundParametersVarValue;
            public System.Management.Automation.ExecutionContext.SavedContextData SavedContextData;
            public System.Management.Automation.Runspaces.RunspaceAvailability RunspaceAvailability;
            public PSLanguageMode LanguageMode;
        }
    }
}

