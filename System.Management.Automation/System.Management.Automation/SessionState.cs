namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Runspaces;

    public sealed class SessionState
    {
        private DriveManagementIntrinsics drive;
        private PathIntrinsics path;
        private CmdletProviderManagementIntrinsics provider;
        private SessionStateInternal sessionState;
        private PSVariableIntrinsics variable;

        public SessionState()
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS == null)
            {
                throw new InvalidOperationException("ExecutionContext");
            }
            this.sessionState = new SessionStateInternal(executionContextFromTLS);
            this.sessionState.PublicSessionState = this;
        }

        internal SessionState(SessionStateInternal sessionState)
        {
            if (sessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("sessionState");
            }
            this.sessionState = sessionState;
        }

        internal SessionState(ExecutionContext context, bool createAsChild, bool linkToGlobal)
        {
            if (context == null)
            {
                throw new InvalidOperationException("ExecutionContext");
            }
            if (createAsChild)
            {
                this.sessionState = new SessionStateInternal(context.EngineSessionState, linkToGlobal, context);
            }
            else
            {
                this.sessionState = new SessionStateInternal(context);
            }
            this.sessionState.PublicSessionState = this;
        }

        public static bool IsVisible(CommandOrigin origin, CommandInfo commandInfo)
        {
            if (origin == CommandOrigin.Internal)
            {
                return true;
            }
            if (commandInfo == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandInfo");
            }
            return (commandInfo.Visibility == SessionStateEntryVisibility.Public);
        }

        public static bool IsVisible(CommandOrigin origin, System.Management.Automation.PSVariable variable)
        {
            if (origin == CommandOrigin.Internal)
            {
                return true;
            }
            if (variable == null)
            {
                throw PSTraceSource.NewArgumentNullException("variable");
            }
            return (variable.Visibility == SessionStateEntryVisibility.Public);
        }

        public static bool IsVisible(CommandOrigin origin, object valueToCheck)
        {
            if (origin != CommandOrigin.Internal)
            {
                IHasSessionStateEntryVisibility visibility = valueToCheck as IHasSessionStateEntryVisibility;
                if (visibility != null)
                {
                    return (visibility.Visibility == SessionStateEntryVisibility.Public);
                }
            }
            return true;
        }

        public static void ThrowIfNotVisible(CommandOrigin origin, object valueToCheck)
        {
            if (!IsVisible(origin, valueToCheck))
            {
                SessionStateException exception;
                System.Management.Automation.PSVariable variable = valueToCheck as System.Management.Automation.PSVariable;
                if (variable != null)
                {
                    exception = new SessionStateException(variable.Name, SessionStateCategory.Variable, "VariableIsPrivate", SessionStateStrings.VariableIsPrivate, ErrorCategory.PermissionDenied, new object[0]);
                    throw exception;
                }
                CommandInfo info = valueToCheck as CommandInfo;
                if (info != null)
                {
                    string itemName = null;
                    if (info != null)
                    {
                        itemName = info.Name;
                    }
                    if (itemName != null)
                    {
                        exception = new SessionStateException(itemName, SessionStateCategory.Command, "NamedCommandIsPrivate", SessionStateStrings.NamedCommandIsPrivate, ErrorCategory.PermissionDenied, new object[0]);
                    }
                    else
                    {
                        exception = new SessionStateException("", SessionStateCategory.Command, "CommandIsPrivate", SessionStateStrings.CommandIsPrivate, ErrorCategory.PermissionDenied, new object[0]);
                    }
                    throw exception;
                }
                exception = new SessionStateException(null, SessionStateCategory.Resource, "ResourceIsPrivate", SessionStateStrings.ResourceIsPrivate, ErrorCategory.PermissionDenied, new object[0]);
                throw exception;
            }
        }

        public List<string> Applications
        {
            get
            {
                return this.sessionState.Applications;
            }
        }

        public DriveManagementIntrinsics Drive
        {
            get
            {
                if (this.drive == null)
                {
                    this.drive = new DriveManagementIntrinsics(this.sessionState);
                }
                return this.drive;
            }
        }

        internal SessionStateInternal Internal
        {
            get
            {
                return this.sessionState;
            }
        }

        public CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                return this.sessionState.ExecutionContext.EngineIntrinsics.InvokeCommand;
            }
        }

        public ProviderIntrinsics InvokeProvider
        {
            get
            {
                return this.sessionState.InvokeProvider;
            }
        }

        public PSLanguageMode LanguageMode
        {
            get
            {
                return this.sessionState.LanguageMode;
            }
            set
            {
                this.sessionState.LanguageMode = value;
            }
        }

        public PSModuleInfo Module
        {
            get
            {
                return this.sessionState.Module;
            }
        }

        public PathIntrinsics Path
        {
            get
            {
                if (this.path == null)
                {
                    this.path = new PathIntrinsics(this.sessionState);
                }
                return this.path;
            }
        }

        public CmdletProviderManagementIntrinsics Provider
        {
            get
            {
                if (this.provider == null)
                {
                    this.provider = new CmdletProviderManagementIntrinsics(this.sessionState);
                }
                return this.provider;
            }
        }

        public PSVariableIntrinsics PSVariable
        {
            get
            {
                if (this.variable == null)
                {
                    this.variable = new PSVariableIntrinsics(this.sessionState);
                }
                return this.variable;
            }
        }

        public List<string> Scripts
        {
            get
            {
                return this.sessionState.Scripts;
            }
        }

        public bool UseFullLanguageModeInDebugger
        {
            get
            {
                return this.sessionState.UseFullLanguageModeInDebugger;
            }
        }
    }
}

