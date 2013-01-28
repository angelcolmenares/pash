namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Internal.Host;
    using System.Management.Automation.Remoting;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal class MshCommandRuntime : ICommandRuntime
    {
        private static long _lastUsedSourceId;
        private long _sourceId;
        internal InternalHost CBhost;
        internal string CBResourcesBaseName = "CommandBaseStrings";
        private CommandInfo commandInfo;
        private bool confirmFlag;
        private ConfirmImpact confirmPreference = ConfirmImpact.High;
        private System.Management.Automation.ExecutionContext context;
        private bool debugFlag;
        private Pipe debugOutputPipe;
        private ActionPreference debugPreference;
        private ActionPreference errorAction = ActionPreference.Continue;
        private MergeDataStream errorMergeTo;
        private Pipe errorOutputPipe;
        private string errorVariable = "";
        private ArrayList errorVarList;
        private PSHost host;
        private Pipe inputPipe;
        private bool isClosed;
        private bool isConfirmFlagSet;
        private bool isConfirmPreferenceCached;
        private bool isDebugFlagSet;
        private bool isDebugPreferenceCached;
        private bool isDebugPreferenceSet;
        private bool isErrorActionPreferenceCached;
        private bool isErrorActionSet;
        private bool isProgressPreferenceCached;
        private bool isProgressPreferenceSet;
        private bool isVerboseFlagSet;
        private bool isVerbosePreferenceCached;
        private bool isWarningPreferenceCached;
        private bool isWarningPreferenceSet;
        private bool isWhatIfFlagSet;
        private bool isWhatIfPreferenceCached;
        internal ContinueStatus lastDebugContinueStatus;
        internal ContinueStatus lastErrorContinueStatus;
        internal ContinueStatus lastProgressContinueStatus;
        internal ContinueStatus lastShouldProcessContinueStatus;
        internal ContinueStatus lastVerboseContinueStatus;
        internal ContinueStatus lastWarningContinueStatus;
        private bool mergeUnclaimedPreviousErrorResults;
        private InvocationInfo myInvocation;
        private Pipe outputPipe;
        private string outVar;
        private IList outVarList;
        private System.Management.Automation.PagingParameters pagingParameters;
        private System.Management.Automation.Internal.PipelineProcessor pipelineProcessor;
        private ActionPreference progressPreference = ActionPreference.Continue;
        private bool shouldLogPipelineExecutionDetail;
        private SessionState state;
        internal static object[] StaticEmptyArray = new object[0];
        private InternalCommand thisCommand;
        internal bool UseSecurityContextRun = true;
        private bool useTransactionFlag;
        private bool useTransactionFlagSet;
        private bool verboseFlag;
        private Pipe verboseOutputPipe;
        private ActionPreference verbosePreference;
        private Pipe warningOutputPipe;
        private ActionPreference warningPreference = ActionPreference.Continue;
        private string warningVariable = "";
        private ArrayList warningVarList;
        private bool whatIfFlag;

        internal MshCommandRuntime(System.Management.Automation.ExecutionContext context, CommandInfo commandInfo, InternalCommand thisCommand)
        {
            this.context = context;
            this.host = context.EngineHostInterface;
            this.CBhost = context.EngineHostInterface;
            this.commandInfo = commandInfo;
            this.thisCommand = thisCommand;
            this.shouldLogPipelineExecutionDetail = this.InitShouldLogPipelineExecutionDetail();
        }

        internal void _WriteErrorSkipAllowCheck(ErrorRecord errorRecord, ActionPreference? actionPreference = new ActionPreference?())
        {
            this.ThrowIfStopping();
            if ((errorRecord.ErrorDetails != null) && (errorRecord.ErrorDetails.TextLookupError != null))
            {
                Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
                errorRecord.ErrorDetails.TextLookupError = null;
                MshLog.LogCommandHealthEvent(this.context, textLookupError, Severity.Warning);
            }
            this.pipelineProcessor.ExecutionFailed = true;
            if (this.shouldLogPipelineExecutionDetail)
            {
                this.pipelineProcessor.LogExecutionError(this.thisCommand.MyInvocation, errorRecord);
            }
            ActionPreference errorAction = this.ErrorAction;
            if (actionPreference.HasValue)
            {
                errorAction = actionPreference.Value;
            }
            if (ActionPreference.Ignore != errorAction)
            {
                if (errorAction == ActionPreference.SilentlyContinue)
                {
                    this.AppendErrorToVariables(errorRecord);
                }
                else
                {
                    if (ContinueStatus.YesToAll == this.lastErrorContinueStatus)
                    {
                        errorAction = ActionPreference.Continue;
                    }
                    switch (errorAction)
                    {
                        case ActionPreference.Stop:
                        {
                            ActionPreferenceStopException e = new ActionPreferenceStopException(this.MyInvocation, errorRecord, this.CBResourcesBaseName, "ErrorPreferenceStop", new object[] { "ErrorActionPreference", errorRecord.ToString() });
                            throw this.ManageException(e);
                        }
                        case ActionPreference.Inquire:
                            this.lastErrorContinueStatus = this.InquireHelper(RuntimeException.RetrieveMessage(errorRecord), null, true, false, true);
                            break;
                    }
                    this.AppendErrorToVariables(errorRecord);
                    PSObject obj2 = PSObject.AsPSObject(errorRecord);
                    if (obj2.Members["writeErrorStream"] == null)
                    {
                        PSNoteProperty member = new PSNoteProperty("writeErrorStream", true);
                        obj2.Properties.Add(member);
                    }
                    if (this.ErrorMergeTo != MergeDataStream.None)
                    {
                        this.OutputPipe.AddWithoutAppendingOutVarList(obj2);
                    }
                    else
                    {
                        this.ErrorOutputPipe.AddWithoutAppendingOutVarList(obj2);
                    }
                }
            }
        }

        internal void _WriteObjectSkipAllowCheck(object sendToPipeline)
        {
            this.ThrowIfStopping();
            if (AutomationNull.Value != sendToPipeline)
            {
                sendToPipeline = LanguagePrimitives.AsPSObjectOrNull(sendToPipeline);
                this.OutputPipe.Add(sendToPipeline);
            }
        }

        internal void _WriteObjectsSkipAllowCheck(object sendToPipeline)
        {
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(sendToPipeline);
            if (enumerable == null)
            {
                this._WriteObjectSkipAllowCheck(sendToPipeline);
            }
            else
            {
                this.ThrowIfStopping();
                ArrayList objects = new ArrayList();
                foreach (object obj2 in enumerable)
                {
                    if (AutomationNull.Value != obj2)
                    {
                        object obj3 = LanguagePrimitives.AsPSObjectOrNull(obj2);
                        objects.Add(obj3);
                    }
                }
                this.OutputPipe.AddItems(objects);
            }
        }

        internal IDisposable AllowThisCommandToWrite(bool permittedToWriteToPipeline)
        {
            return new AllowWrite(this.thisCommand, permittedToWriteToPipeline);
        }

        private void AppendDollarError(object obj)
        {
            if (!(obj is Exception) || ((this.pipelineProcessor != null) && this.pipelineProcessor.TopLevel))
            {
                this.context.AppendDollarError(obj);
            }
        }

        internal void AppendErrorToVariables(object obj)
        {
            if (obj != null)
            {
                this.AppendDollarError(obj);
                this.OutputPipe.AppendVariableList(VariableStreamKind.Error, obj);
            }
        }

        internal void AppendWarningVarList(object obj)
        {
            this.OutputPipe.AppendVariableList(VariableStreamKind.Warning, obj);
        }

        internal ShouldProcessPossibleOptimization CalculatePossibleShouldProcessOptimization()
        {
            if (this.WhatIf != 0)
            {
                return ShouldProcessPossibleOptimization.AutoNo_CanCallShouldProcessAsynchronously;
            }
            if (!this.CanShouldProcessAutoConfirm())
            {
                return ShouldProcessPossibleOptimization.NoOptimizationPossible;
            }
            if (this.Verbose)
            {
                return ShouldProcessPossibleOptimization.AutoYes_CanCallShouldProcessAsynchronously;
            }
            return ShouldProcessPossibleOptimization.AutoYes_CanSkipShouldProcessCall;
        }

        private bool CanShouldProcessAutoConfirm()
        {
            CommandMetadata commandMetadata = this.commandInfo.CommandMetadata;
            if (commandMetadata != null)
            {
                ConfirmImpact confirmImpact = commandMetadata.ConfirmImpact;
                ConfirmImpact confirmPreference = this.ConfirmPreference;
                if ((confirmPreference != ConfirmImpact.None) && (confirmPreference <= confirmImpact))
                {
                    return false;
                }
            }
            return true;
        }

        private bool DoShouldContinue(string query, string caption, bool supportsToAllOptions, ref bool yesToAll, ref bool noToAll)
        {
            this.ThrowIfStopping();
            this.ThrowIfWriteNotPermitted(false);
            if (noToAll)
            {
                return false;
            }
            if (!yesToAll)
            {
                switch (this.InquireHelper(query, caption, supportsToAllOptions, supportsToAllOptions, false))
                {
                    case ContinueStatus.No:
                        return false;

                    case ContinueStatus.YesToAll:
                        yesToAll = true;
                        break;

                    case ContinueStatus.NoToAll:
                        noToAll = true;
                        return false;
                }
            }
            return true;
        }

        private bool DoShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            this.ThrowIfStopping();
            shouldProcessReason = ShouldProcessReason.None;
            switch (this.lastShouldProcessContinueStatus)
            {
                case ContinueStatus.YesToAll:
                    return true;

                case ContinueStatus.NoToAll:
                    return false;

                default:
                    if (this.WhatIf != 0)
                    {
                        this.ThrowIfWriteNotPermitted(false);
                        shouldProcessReason = ShouldProcessReason.WhatIf;
                        string str = StringUtil.Format(CommandBaseStrings.ShouldProcessWhatIfMessage, verboseDescription);
                        this.CBhost.UI.WriteLine(str);
                        return false;
                    }
                    if (this.CanShouldProcessAutoConfirm())
                    {
                        if (this.Verbose)
                        {
                            this.ThrowIfWriteNotPermitted(false);
                            this.WriteVerbose(verboseDescription);
                        }
                        return true;
                    }
                    if (string.IsNullOrEmpty(verboseWarning))
                    {
                        verboseWarning = StringUtil.Format(CommandBaseStrings.ShouldProcessWarningFallback, verboseDescription);
                    }
                    this.ThrowIfWriteNotPermitted(false);
                    this.lastShouldProcessContinueStatus = this.InquireHelper(verboseWarning, caption, true, true, false);
                    switch (this.lastShouldProcessContinueStatus)
                    {
                        case ContinueStatus.No:
                        case ContinueStatus.NoToAll:
                            return false;
                    }
                    break;
            }
            return true;
        }

        private void DoWriteError(object obj)
        {
            KeyValuePair<ErrorRecord, ActionPreference> pair = (KeyValuePair<ErrorRecord, ActionPreference>) obj;
            ErrorRecord key = pair.Key;
            ActionPreference preference = pair.Value;
            if (key == null)
            {
                throw PSTraceSource.NewArgumentNullException("errorRecord");
            }
            if (((this.UseTransaction != 0) && (this.context.TransactionManager.RollbackPreference != RollbackSeverity.TerminatingError)) && (this.context.TransactionManager.RollbackPreference != RollbackSeverity.Never))
            {
                this.context.TransactionManager.Rollback(true);
            }
            if (key.PreserveInvocationInfoOnce)
            {
                key.PreserveInvocationInfoOnce = false;
            }
            else
            {
                key.SetInvocationInfo(this.MyInvocation);
            }
            this.ThrowIfWriteNotPermitted(true);
            this._WriteErrorSkipAllowCheck(key, new ActionPreference?(preference));
        }

        private void DoWriteObject(object sendToPipeline)
        {
            this.ThrowIfWriteNotPermitted(true);
            this._WriteObjectSkipAllowCheck(sendToPipeline);
        }

        private void DoWriteObjects(object sendToPipeline)
        {
            this.ThrowIfWriteNotPermitted(true);
            this._WriteObjectsSkipAllowCheck(sendToPipeline);
        }

        private void EnsureVariableParameterAllowed()
        {
            if ((this.context.LanguageMode == PSLanguageMode.NoLanguage) || (this.context.LanguageMode == PSLanguageMode.RestrictedLanguage))
            {
                throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "VariableReferenceNotSupportedInDataSection", ParserStrings.VariableReferenceNotSupportedInDataSection, new object[0]);
            }
        }

        internal object[] GetResultsAsArray()
        {
            if (this.outputPipe == null)
            {
                return StaticEmptyArray;
            }
            return this.outputPipe.ToArray();
        }

        private bool InitShouldLogPipelineExecutionDetail()
        {
            CmdletInfo commandInfo = this.commandInfo as CmdletInfo;
            if (commandInfo != null)
            {
                if ((commandInfo.Module == null) && (commandInfo.PSSnapIn != null))
                {
                    return commandInfo.PSSnapIn.LogPipelineExecutionDetails;
                }
                return (((commandInfo.PSSnapIn == null) && (commandInfo.Module != null)) && commandInfo.Module.LogPipelineExecutionDetails);
            }
            FunctionInfo info2 = this.commandInfo as FunctionInfo;
            return (((info2 != null) && (info2.Module != null)) && info2.Module.LogPipelineExecutionDetails);
        }

        internal ContinueStatus InquireHelper(string inquireMessage, string inquireCaption, bool allowYesToAll, bool allowNoToAll, bool replaceNoWithHalt)
        {
            Collection<ChoiceDescription> choices = new Collection<ChoiceDescription>();
            int num = 0;
            int num2 = 0x7fffffff;
            int num3 = 0x7fffffff;
            int num4 = 0x7fffffff;
            int num5 = 0x7fffffff;
            int num6 = 0x7fffffff;
            int num7 = 0x7fffffff;
            string continueOneLabel = CommandBaseStrings.ContinueOneLabel;
            string continueOneHelpMessage = CommandBaseStrings.ContinueOneHelpMessage;
            choices.Add(new ChoiceDescription(continueOneLabel, continueOneHelpMessage));
            num2 = num++;
            if (allowYesToAll)
            {
                string continueAllLabel = CommandBaseStrings.ContinueAllLabel;
                string continueAllHelpMessage = CommandBaseStrings.ContinueAllHelpMessage;
                choices.Add(new ChoiceDescription(continueAllLabel, continueAllHelpMessage));
                num3 = num++;
            }
            if (replaceNoWithHalt)
            {
                string haltLabel = CommandBaseStrings.HaltLabel;
                string haltHelpMessage = CommandBaseStrings.HaltHelpMessage;
                choices.Add(new ChoiceDescription(haltLabel, haltHelpMessage));
                num4 = num++;
            }
            else
            {
                string skipOneLabel = CommandBaseStrings.SkipOneLabel;
                string skipOneHelpMessage = CommandBaseStrings.SkipOneHelpMessage;
                choices.Add(new ChoiceDescription(skipOneLabel, skipOneHelpMessage));
                num5 = num++;
            }
            if (allowNoToAll)
            {
                string skipAllLabel = CommandBaseStrings.SkipAllLabel;
                string skipAllHelpMessage = CommandBaseStrings.SkipAllHelpMessage;
                choices.Add(new ChoiceDescription(skipAllLabel, skipAllHelpMessage));
                num6 = num++;
            }
            if (this.IsSuspendPromptAllowed())
            {
                string pauseLabel = CommandBaseStrings.PauseLabel;
                string helpMessage = StringUtil.Format(CommandBaseStrings.PauseHelpMessage, "exit");
                choices.Add(new ChoiceDescription(pauseLabel, helpMessage));
                num7 = num++;
            }
            if (string.IsNullOrEmpty(inquireMessage))
            {
                inquireMessage = CommandBaseStrings.ShouldContinuePromptCaption;
            }
            if (string.IsNullOrEmpty(inquireCaption))
            {
                inquireCaption = CommandBaseStrings.InquireCaptionDefault;
            }
            while (true)
            {
                int num8 = this.CBhost.UI.PromptForChoice(inquireCaption, inquireMessage, choices, 0);
                if (num2 == num8)
                {
                    return ContinueStatus.Yes;
                }
                if (num3 == num8)
                {
                    return ContinueStatus.YesToAll;
                }
                if (num4 == num8)
                {
                    ActionPreferenceStopException e = new ActionPreferenceStopException(this.MyInvocation, this.CBResourcesBaseName, "InquireHalt", new object[0]);
                    throw this.ManageException(e);
                }
                if (num5 == num8)
                {
                    return ContinueStatus.No;
                }
                if (num6 == num8)
                {
                    return ContinueStatus.NoToAll;
                }
                if (num7 != num8)
                {
                    if (-1 == num8)
                    {
                        ActionPreferenceStopException exception2 = new ActionPreferenceStopException(this.MyInvocation, this.CBResourcesBaseName, "InquireCtrlC", new object[0]);
                        throw this.ManageException(exception2);
                    }
                    InvalidOperationException exception3 = PSTraceSource.NewInvalidOperationException();
                    throw this.ManageException(exception3);
                }
                this.CBhost.EnterNestedPrompt(this.thisCommand);
            }
        }

        private bool IsSuspendPromptAllowed()
        {
            if (this.CBhost.ExternalHost is ServerRemoteHost)
            {
                return false;
            }
            return true;
        }

        public Exception ManageException(Exception e)
        {
            if (e == null)
            {
                throw PSTraceSource.NewArgumentNullException("e");
            }
            if (this.pipelineProcessor != null)
            {
                this.pipelineProcessor.RecordFailure(e, this.thisCommand);
            }
            if ((!(e is HaltCommandException) && !(e is PipelineStoppedException)) && !(e is ExitNestedPromptException))
            {
                this.AppendErrorToVariables(e);
                MshLog.LogCommandHealthEvent(this.context, e, Severity.Warning);
            }
            return new PipelineStoppedException();
        }

        internal void RemoveVariableListsInPipe()
        {
            if (this.outVarList != null)
            {
                this.OutputPipe.RemoveVariableList(VariableStreamKind.Output, this.outVarList);
            }
            if (this.errorVarList != null)
            {
                this.OutputPipe.RemoveVariableList(VariableStreamKind.Error, this.errorVarList);
            }
            if (this.warningVarList != null)
            {
                this.OutputPipe.RemoveVariableList(VariableStreamKind.Warning, this.warningVarList);
            }
        }

        internal void SetMergeFromRuntime(MshCommandRuntime fromRuntime)
        {
            this.ErrorMergeTo = fromRuntime.ErrorMergeTo;
            if (fromRuntime.WarningOutputPipe != null)
            {
                this.WarningOutputPipe = fromRuntime.WarningOutputPipe;
            }
            if (fromRuntime.VerboseOutputPipe != null)
            {
                this.VerboseOutputPipe = fromRuntime.VerboseOutputPipe;
            }
            if (fromRuntime.DebugOutputPipe != null)
            {
                this.DebugOutputPipe = fromRuntime.DebugOutputPipe;
            }
        }

        internal void SetupErrorVariable()
        {
            if (!string.IsNullOrEmpty(this.ErrorVariable))
            {
                this.EnsureVariableParameterAllowed();
                if (this.state == null)
                {
                    this.state = new SessionState(this.context.EngineSessionState);
                }
                string errorVariable = this.ErrorVariable;
                if (errorVariable.StartsWith("+", StringComparison.Ordinal))
                {
                    errorVariable = errorVariable.Substring(1);
                    object obj2 = PSObject.Base(this.state.PSVariable.GetValue(errorVariable));
                    this.errorVarList = obj2 as ArrayList;
                    if (this.errorVarList == null)
                    {
                        this.errorVarList = new ArrayList();
                        if ((obj2 != null) && (AutomationNull.Value != obj2))
                        {
                            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj2);
                            if (enumerable != null)
                            {
                                foreach (object obj3 in enumerable)
                                {
                                    this.errorVarList.Add(obj3);
                                }
                            }
                            else
                            {
                                this.errorVarList.Add(obj2);
                            }
                        }
                    }
                }
                else
                {
                    this.errorVarList = new ArrayList();
                }
                if (!(this.thisCommand is PSScriptCmdlet))
                {
                    this.OutputPipe.AddVariableList(VariableStreamKind.Error, this.errorVarList);
                }
                this.state.PSVariable.Set(errorVariable, this.errorVarList);
            }
        }

        internal void SetupOutVariable()
        {
            if (!string.IsNullOrEmpty(this.OutVariable))
            {
                this.EnsureVariableParameterAllowed();
                if (this.state == null)
                {
                    this.state = new SessionState(this.context.EngineSessionState);
                }
                string outVariable = this.OutVariable;
                if (outVariable.StartsWith("+", StringComparison.Ordinal))
                {
                    outVariable = outVariable.Substring(1);
                    object obj2 = PSObject.Base(this.state.PSVariable.GetValue(outVariable));
                    this.outVarList = obj2 as IList;
                    if (this.outVarList == null)
                    {
                        this.outVarList = new ArrayList();
                        if (obj2 != null)
                        {
                            this.outVarList.Add(obj2);
                        }
                    }
                    else if (this.outVarList.IsFixedSize)
                    {
                        ArrayList list = new ArrayList();
                        list.AddRange(this.outVarList);
                        this.outVarList = list;
                    }
                }
                else
                {
                    this.outVarList = new ArrayList();
                }
                if (!(this.thisCommand is PSScriptCmdlet))
                {
                    this.OutputPipe.AddVariableList(VariableStreamKind.Output, this.outVarList);
                }
                this.state.PSVariable.Set(outVariable, this.outVarList);
            }
        }

        internal void SetupWarningVariable()
        {
            if (!string.IsNullOrEmpty(this.WarningVariable))
            {
                this.EnsureVariableParameterAllowed();
                if (this.state == null)
                {
                    this.state = new SessionState(this.context.EngineSessionState);
                }
                string warningVariable = this.WarningVariable;
                if (warningVariable.StartsWith("+", StringComparison.Ordinal))
                {
                    warningVariable = warningVariable.Substring(1);
                    object obj2 = PSObject.Base(this.state.PSVariable.GetValue(warningVariable));
                    this.warningVarList = obj2 as ArrayList;
                    if (this.warningVarList == null)
                    {
                        this.warningVarList = new ArrayList();
                        if ((obj2 != null) && (AutomationNull.Value != obj2))
                        {
                            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(obj2);
                            if (enumerable != null)
                            {
                                foreach (object obj3 in enumerable)
                                {
                                    this.warningVarList.Add(obj3);
                                }
                            }
                            else
                            {
                                this.warningVarList.Add(obj2);
                            }
                        }
                    }
                }
                else
                {
                    this.warningVarList = new ArrayList();
                }
                if (!(this.thisCommand is PSScriptCmdlet))
                {
                    this.OutputPipe.AddVariableList(VariableStreamKind.Warning, this.warningVarList);
                }
                this.state.PSVariable.Set(warningVariable, this.warningVarList);
            }
        }

        internal void SetVariableListsInPipe()
        {
            if (this.outVarList != null)
            {
                this.OutputPipe.AddVariableList(VariableStreamKind.Output, this.outVarList);
            }
            if (this.errorVarList != null)
            {
                this.OutputPipe.AddVariableList(VariableStreamKind.Error, this.errorVarList);
            }
            if (this.warningVarList != null)
            {
                this.OutputPipe.AddVariableList(VariableStreamKind.Warning, this.warningVarList);
            }
        }

        public bool ShouldContinue(string query, string caption)
        {
            bool yesToAll = false;
            bool noToAll = false;
            return this.DoShouldContinue(query, caption, false, ref yesToAll, ref noToAll);
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            return this.DoShouldContinue(query, caption, true, ref yesToAll, ref noToAll);
        }

        public bool ShouldProcess(string target)
        {
            ShouldProcessReason reason;
            string verboseDescription = StringUtil.Format(CommandBaseStrings.ShouldProcessMessage, this.MyInvocation.MyCommand.Name, target);
            return this.DoShouldProcess(verboseDescription, null, null, out reason);
        }

        public bool ShouldProcess(string target, string action)
        {
            ShouldProcessReason reason;
            object[] o = new object[3];
            o[0] = action;
            o[1] = target;
            string verboseDescription = StringUtil.Format(CommandBaseStrings.ShouldProcessMessage, o);
            return this.DoShouldProcess(verboseDescription, null, null, out reason);
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            ShouldProcessReason reason;
            return this.DoShouldProcess(verboseDescription, verboseWarning, caption, out reason);
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            return this.DoShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
        }

        internal void ThrowIfStopping()
        {
            if (this.IsStopping)
            {
                throw new PipelineStoppedException();
            }
        }

        internal void ThrowIfWriteNotPermitted(bool needsToWriteToPipeline)
        {
            if ((((this.pipelineProcessor == null) || (this.thisCommand != this.pipelineProcessor._permittedToWrite)) || (needsToWriteToPipeline && !this.pipelineProcessor._permittedToWriteToPipeline)) || (Thread.CurrentThread != this.pipelineProcessor._permittedToWriteThread))
            {
                throw PSTraceSource.NewInvalidOperationException("PipelineStrings", "WriteNotPermitted", new object[0]);
            }
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            this.ThrowIfStopping();
            if (errorRecord == null)
            {
                throw PSTraceSource.NewArgumentNullException("errorRecord");
            }
            errorRecord.SetInvocationInfo(this.MyInvocation);
            if ((errorRecord.ErrorDetails != null) && (errorRecord.ErrorDetails.TextLookupError != null))
            {
                Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
                errorRecord.ErrorDetails.TextLookupError = null;
                MshLog.LogCommandHealthEvent(this.context, textLookupError, Severity.Warning);
            }
            if ((errorRecord.Exception != null) && string.IsNullOrEmpty(errorRecord.Exception.StackTrace))
            {
                try
                {
                    throw errorRecord.Exception;
                }
                catch (Exception)
                {
                }
            }
            CmdletInvocationException e = new CmdletInvocationException(errorRecord);
            throw this.ManageException(e);
        }

        public override string ToString()
        {
            if (this.commandInfo != null)
            {
                return this.commandInfo.ToString();
            }
            return "<NullCommandInfo>";
        }

        public bool TransactionAvailable()
        {
            return (this.UseTransactionFlagSet && this.Context.TransactionManager.HasTransaction);
        }

        public void WriteCommandDetail(string text)
        {
            if (this.LogPipelineExecutionDetail)
            {
                this.pipelineProcessor.LogExecutionInfo(this.thisCommand.MyInvocation, text);
            }
        }

        public void WriteDebug(string text)
        {
            this.WriteDebug(new DebugRecord(text), false);
        }

        internal void WriteDebug(DebugRecord record, bool overrideInquire = false)
        {
            ActionPreference debugPreference = this.DebugPreference;
            if (overrideInquire && (debugPreference == ActionPreference.Inquire))
            {
                debugPreference = ActionPreference.Continue;
            }
            if (this.WriteHelper_ShouldWrite(debugPreference, this.lastDebugContinueStatus))
            {
                if (record.InvocationInfo == null)
                {
                    record.SetInvocationInfo(this.MyInvocation);
                }
                if (this.DebugOutputPipe != null)
                {
                    if (((this.CBhost != null) && (this.CBhost.InternalUI != null)) && this.DebugOutputPipe.NullPipe)
                    {
                        this.CBhost.InternalUI.WriteDebugInfoBuffers(record);
                    }
                    PSObject obj2 = PSObject.AsPSObject(record);
                    if (obj2.Members["WriteDebugStream"] == null)
                    {
                        obj2.Properties.Add(new PSNoteProperty("WriteDebugStream", true));
                    }
                    this.DebugOutputPipe.Add(obj2);
                }
                else
                {
                    if ((this.Host == null) || (this.Host.UI == null))
                    {
                        throw PSTraceSource.NewInvalidOperationException();
                    }
                    this.CBhost.InternalUI.WriteDebugRecord(record);
                }
            }
            this.lastDebugContinueStatus = this.WriteHelper(null, null, debugPreference, this.lastDebugContinueStatus, "DebugPreference", record.Message);
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            this.WriteError(errorRecord, false);
        }

        internal void WriteError(ErrorRecord errorRecord, bool overrideInquire)
        {
            this.ThrowIfStopping();
            ActionPreference errorAction = this.ErrorAction;
            if (overrideInquire && (errorAction == ActionPreference.Inquire))
            {
                errorAction = ActionPreference.Continue;
            }
            if (this.UseSecurityContextRun)
            {
                if ((this.pipelineProcessor == null) || (this.pipelineProcessor.SecurityContext == null))
                {
                    throw PSTraceSource.NewInvalidOperationException("pipeline", "WriteNotPermitted", new object[0]);
                }
                ContextCallback callback = new ContextCallback(this.DoWriteError);
                SecurityContext.Run(this.pipelineProcessor.SecurityContext.CreateCopy(), callback, new KeyValuePair<ErrorRecord, ActionPreference>(errorRecord, errorAction));
            }
            else
            {
                this.DoWriteError(new KeyValuePair<ErrorRecord, ActionPreference>(errorRecord, errorAction));
            }
        }

        internal ContinueStatus WriteHelper(string inquireCaption, string inquireMessage, ActionPreference preference, ContinueStatus lastContinueStatus, string preferenceVariableName, string message)
        {
            switch (lastContinueStatus)
            {
                case ContinueStatus.YesToAll:
                    return ContinueStatus.YesToAll;

                case ContinueStatus.NoToAll:
                    return ContinueStatus.NoToAll;
            }
            switch (preference)
            {
                case ActionPreference.SilentlyContinue:
                case ActionPreference.Continue:
                case ActionPreference.Ignore:
                    return ContinueStatus.Yes;

                case ActionPreference.Stop:
                {
                    ActionPreferenceStopException exception = new ActionPreferenceStopException(this.MyInvocation, this.CBResourcesBaseName, "ErrorPreferenceStop", new object[] { preferenceVariableName, message });
                    throw this.ManageException(exception);
                }
                case ActionPreference.Inquire:
                    return this.InquireHelper(inquireMessage, inquireCaption, true, false, true);
            }
            ActionPreferenceStopException e = new ActionPreferenceStopException(this.MyInvocation, this.CBResourcesBaseName, "PreferenceInvalid", new object[] { preferenceVariableName, preference });
            throw this.ManageException(e);
        }

        internal bool WriteHelper_ShouldWrite(ActionPreference preference, ContinueStatus lastContinueStatus)
        {
            this.ThrowIfStopping();
            this.ThrowIfWriteNotPermitted(false);
            switch (lastContinueStatus)
            {
                case ContinueStatus.YesToAll:
                    return true;

                case ContinueStatus.NoToAll:
                    return false;
            }
            switch (preference)
            {
                case ActionPreference.SilentlyContinue:
                case ActionPreference.Ignore:
                    return false;

                case ActionPreference.Stop:
                case ActionPreference.Continue:
                case ActionPreference.Inquire:
                    return true;
            }
            return true;
        }

        public void WriteObject(object sendToPipeline)
        {
            this.ThrowIfStopping();
            if (this.UseSecurityContextRun)
            {
                if ((this.pipelineProcessor == null) || (this.pipelineProcessor.SecurityContext == null))
                {
                    throw PSTraceSource.NewInvalidOperationException("pipeline", "WriteNotPermitted", new object[0]);
                }
                ContextCallback callback = new ContextCallback(this.DoWriteObject);
                SecurityContext.Run(this.pipelineProcessor.SecurityContext.CreateCopy(), callback, sendToPipeline);
            }
            else
            {
                this.DoWriteObject(sendToPipeline);
            }
        }

        public void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (!enumerateCollection)
            {
                this.WriteObject(sendToPipeline);
            }
            else
            {
                this.ThrowIfStopping();
                if (this.UseSecurityContextRun)
                {
                    if ((this.pipelineProcessor == null) || (this.pipelineProcessor.SecurityContext == null))
                    {
                        throw PSTraceSource.NewInvalidOperationException("pipeline", "WriteNotPermitted", new object[0]);
                    }
                    ContextCallback callback = new ContextCallback(this.DoWriteObjects);
                    SecurityContext.Run(this.pipelineProcessor.SecurityContext.CreateCopy(), callback, sendToPipeline);
                }
                else
                {
                    this.DoWriteObjects(sendToPipeline);
                }
            }
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
            this.WriteProgress(progressRecord, false);
        }

        public void WriteProgress(long sourceId, ProgressRecord progressRecord)
        {
            this.WriteProgress(sourceId, progressRecord, false);
        }

        internal void WriteProgress(ProgressRecord progressRecord, bool overrideInquire)
        {
            this.ThrowIfStopping();
            this.ThrowIfWriteNotPermitted(false);
            if (0L == this._sourceId)
            {
                this._sourceId = Interlocked.Increment(ref _lastUsedSourceId);
            }
            this.WriteProgress(this._sourceId, progressRecord, overrideInquire);
        }

        internal void WriteProgress(long sourceId, ProgressRecord progressRecord, bool overrideInquire)
        {
            if (progressRecord == null)
            {
                throw PSTraceSource.NewArgumentNullException("progressRecord");
            }
            if ((this.Host == null) || (this.Host.UI == null))
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            InternalHostUserInterface uI = this.Host.UI as InternalHostUserInterface;
            ActionPreference progressPreference = this.ProgressPreference;
            if (overrideInquire && (progressPreference == ActionPreference.Inquire))
            {
                progressPreference = ActionPreference.Continue;
            }
            if (this.WriteHelper_ShouldWrite(progressPreference, this.lastProgressContinueStatus))
            {
                uI.WriteProgress(sourceId, progressRecord);
            }
            this.lastProgressContinueStatus = this.WriteHelper(null, null, progressPreference, this.lastProgressContinueStatus, "ProgressPreference", progressRecord.Activity);
        }

        public void WriteVerbose(string text)
        {
            this.WriteVerbose(new VerboseRecord(text), false);
        }

        internal void WriteVerbose(VerboseRecord record, bool overrideInquire = false)
        {
            ActionPreference verbosePreference = this.VerbosePreference;
            if (overrideInquire && (verbosePreference == ActionPreference.Inquire))
            {
                verbosePreference = ActionPreference.Continue;
            }
            if (this.WriteHelper_ShouldWrite(verbosePreference, this.lastVerboseContinueStatus))
            {
                if (record.InvocationInfo == null)
                {
                    record.SetInvocationInfo(this.MyInvocation);
                }
                if (this.VerboseOutputPipe != null)
                {
                    if (((this.CBhost != null) && (this.CBhost.InternalUI != null)) && this.VerboseOutputPipe.NullPipe)
                    {
                        this.CBhost.InternalUI.WriteVerboseInfoBuffers(record);
                    }
                    PSObject obj2 = PSObject.AsPSObject(record);
                    if (obj2.Members["WriteVerboseStream"] == null)
                    {
                        obj2.Properties.Add(new PSNoteProperty("WriteVerboseStream", true));
                    }
                    this.VerboseOutputPipe.Add(obj2);
                }
                else
                {
                    if ((this.Host == null) || (this.Host.UI == null))
                    {
                        throw PSTraceSource.NewInvalidOperationException();
                    }
                    this.CBhost.InternalUI.WriteVerboseRecord(record);
                }
            }
            this.lastVerboseContinueStatus = this.WriteHelper(null, null, verbosePreference, this.lastVerboseContinueStatus, "VerbosePreference", record.Message);
        }

        public void WriteWarning(string text)
        {
            this.WriteWarning(new WarningRecord(text), false);
        }

        internal void WriteWarning(WarningRecord record, bool overrideInquire = false)
        {
            ActionPreference warningPreference = this.WarningPreference;
            if (overrideInquire && (warningPreference == ActionPreference.Inquire))
            {
                warningPreference = ActionPreference.Continue;
            }
            if (this.WriteHelper_ShouldWrite(warningPreference, this.lastWarningContinueStatus))
            {
                if (record.InvocationInfo == null)
                {
                    record.SetInvocationInfo(this.MyInvocation);
                }
                if (this.WarningOutputPipe != null)
                {
                    if (((this.CBhost != null) && (this.CBhost.InternalUI != null)) && this.WarningOutputPipe.NullPipe)
                    {
                        this.CBhost.InternalUI.WriteWarningInfoBuffers(record);
                    }
                    PSObject obj2 = PSObject.AsPSObject(record);
                    if (obj2.Members["WriteWarningStream"] == null)
                    {
                        obj2.Properties.Add(new PSNoteProperty("WriteWarningStream", true));
                    }
                    this.WarningOutputPipe.AddWithoutAppendingOutVarList(obj2);
                }
                else
                {
                    if ((this.Host == null) || (this.Host.UI == null))
                    {
                        throw PSTraceSource.NewInvalidOperationException();
                    }
                    this.CBhost.InternalUI.WriteWarningRecord(record);
                }
            }
            this.AppendWarningVarList(record);
            this.lastWarningContinueStatus = this.WriteHelper(null, null, warningPreference, this.lastWarningContinueStatus, "WarningPreference", record.Message);
        }

        internal SwitchParameter Confirm
        {
            get
            {
                return this.confirmFlag;
            }
            set
            {
                this.confirmFlag = (bool) value;
                this.isConfirmFlagSet = true;
            }
        }

        internal ConfirmImpact ConfirmPreference
        {
            get
            {
                if (this.Confirm != 0)
                {
                    return ConfirmImpact.Low;
                }
                if (this.Debug)
                {
                    if (this.isConfirmFlagSet)
                    {
                        return ConfirmImpact.None;
                    }
                    return ConfirmImpact.Low;
                }
                if (this.isConfirmFlagSet)
                {
                    return ConfirmImpact.None;
                }
                if (!this.isConfirmPreferenceCached)
                {
                    bool defaultUsed = false;
                    this.confirmPreference = this.Context.GetEnumPreference<ConfirmImpact>(SpecialVariables.ConfirmPreferenceVarPath, this.confirmPreference, out defaultUsed);
                    this.isConfirmPreferenceCached = true;
                }
                return this.confirmPreference;
            }
        }

        internal System.Management.Automation.ExecutionContext Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }

        public PSTransactionContext CurrentPSTransaction
        {
            get
            {
                if (this.TransactionAvailable())
                {
                    return new PSTransactionContext(this.Context.TransactionManager);
                }
                string message = null;
                if (!this.UseTransactionFlagSet)
                {
                    message = TransactionStrings.CmdletRequiresUseTx;
                }
                else
                {
                    message = TransactionStrings.NoTransactionAvailable;
                }
                throw new InvalidOperationException(message);
            }
        }

        internal bool Debug
        {
            get
            {
                return this.debugFlag;
            }
            set
            {
                this.debugFlag = value;
                this.isDebugFlagSet = true;
            }
        }

        internal Pipe DebugOutputPipe
        {
            get
            {
                return this.debugOutputPipe;
            }
            set
            {
                this.debugOutputPipe = value;
            }
        }

        internal ActionPreference DebugPreference
        {
            get
            {
                if (!this.isDebugPreferenceSet)
                {
                    if (this.isDebugFlagSet)
                    {
                        if (!this.Debug)
                        {
                            return ActionPreference.SilentlyContinue;
                        }
                        if (this.CBhost.ExternalHost.UI == null)
                        {
                            return ActionPreference.Continue;
                        }
                        return ActionPreference.Inquire;
                    }
                    if (!this.isDebugPreferenceCached)
                    {
                        bool defaultUsed = false;
                        this.debugPreference = this.context.GetEnumPreference<ActionPreference>(SpecialVariables.DebugPreferenceVarPath, this.debugPreference, out defaultUsed);
                        if ((this.CBhost.ExternalHost.UI == null) && (this.debugPreference == ActionPreference.Inquire))
                        {
                            this.debugPreference = ActionPreference.Continue;
                        }
                        this.isDebugPreferenceCached = true;
                    }
                }
                return this.debugPreference;
            }
            set
            {
                this.debugPreference = value;
                this.isDebugPreferenceSet = true;
            }
        }

        internal ActionPreference ErrorAction
        {
            get
            {
                if (!this.isErrorActionSet)
                {
                    if (this.Debug)
                    {
                        return ActionPreference.Inquire;
                    }
                    if (this.Verbose)
                    {
                        return ActionPreference.Continue;
                    }
                    if (!this.isErrorActionPreferenceCached)
                    {
                        bool defaultUsed = false;
                        this.errorAction = this.context.GetEnumPreference<ActionPreference>(SpecialVariables.ErrorActionPreferenceVarPath, this.errorAction, out defaultUsed);
                        this.isErrorActionPreferenceCached = true;
                    }
                }
                return this.errorAction;
            }
            set
            {
                this.errorAction = value;
                this.isErrorActionSet = true;
            }
        }

        internal MergeDataStream ErrorMergeTo
        {
            get
            {
                return this.errorMergeTo;
            }
            set
            {
                this.errorMergeTo = value;
            }
        }

        internal Pipe ErrorOutputPipe
        {
            get
            {
                if (this.errorOutputPipe == null)
                {
                    this.errorOutputPipe = new Pipe();
                }
                return this.errorOutputPipe;
            }
            set
            {
                this.errorOutputPipe = value;
            }
        }

        internal string ErrorVariable
        {
            get
            {
                return this.errorVariable;
            }
            set
            {
                this.errorVariable = value;
            }
        }

        public PSHost Host
        {
            get
            {
                return this.host;
            }
        }

        internal Pipe InputPipe
        {
            get
            {
                if (this.inputPipe == null)
                {
                    this.inputPipe = new Pipe();
                }
                return this.inputPipe;
            }
            set
            {
                this.inputPipe = value;
            }
        }

        internal bool IsClosed
        {
            get
            {
                return this.isClosed;
            }
            set
            {
                this.isClosed = value;
            }
        }

        internal bool IsConfirmFlagSet
        {
            get
            {
                return this.isConfirmFlagSet;
            }
        }

        internal bool IsDebugFlagSet
        {
            get
            {
                return this.isDebugFlagSet;
            }
        }

        internal bool IsErrorActionSet
        {
            get
            {
                return this.isErrorActionSet;
            }
        }

        internal bool IsPipelineInputExpected
        {
            get
            {
                return (!this.isClosed || ((this.inputPipe != null) && !this.inputPipe.Empty));
            }
        }

        internal bool IsStopping
        {
            get
            {
                return ((this.pipelineProcessor != null) && this.pipelineProcessor.Stopping);
            }
        }

        internal bool IsVerboseFlagSet
        {
            get
            {
                return this.isVerboseFlagSet;
            }
        }

        internal bool IsWarningActionSet
        {
            get
            {
                return this.isWarningPreferenceSet;
            }
        }

        internal bool IsWhatIfFlagSet
        {
            get
            {
                return this.isWhatIfFlagSet;
            }
        }

        internal bool LogPipelineExecutionDetail
        {
            get
            {
                return this.shouldLogPipelineExecutionDetail;
            }
        }

        internal bool MergeUnclaimedPreviousErrorResults
        {
            get
            {
                return this.mergeUnclaimedPreviousErrorResults;
            }
            set
            {
                this.mergeUnclaimedPreviousErrorResults = value;
            }
        }

        internal InvocationInfo MyInvocation
        {
            get
            {
                if (this.myInvocation == null)
                {
                    this.myInvocation = this.thisCommand.MyInvocation;
                }
                return this.myInvocation;
            }
        }

        internal int OutBuffer
        {
            get
            {
                return this.OutputPipe.OutBufferCount;
            }
            set
            {
                this.OutputPipe.OutBufferCount = value;
            }
        }

        internal Pipe OutputPipe
        {
            get
            {
                if (this.outputPipe == null)
                {
                    this.outputPipe = new Pipe();
                }
                return this.outputPipe;
            }
            set
            {
                this.outputPipe = value;
            }
        }

        internal string OutVariable
        {
            get
            {
                return this.outVar;
            }
            set
            {
                this.outVar = value;
            }
        }

        internal IList OutVarList
        {
            get
            {
                return this.outVarList;
            }
            set
            {
                this.outVarList = value;
            }
        }

        internal System.Management.Automation.PagingParameters PagingParameters
        {
            get
            {
                return this.pagingParameters;
            }
            set
            {
                this.pagingParameters = value;
            }
        }

        internal System.Management.Automation.Internal.PipelineProcessor PipelineProcessor
        {
            get
            {
                return this.pipelineProcessor;
            }
            set
            {
                this.pipelineProcessor = value;
            }
        }

        internal ActionPreference ProgressPreference
        {
            get
            {
                if (!this.isProgressPreferenceSet && !this.isProgressPreferenceCached)
                {
                    bool defaultUsed = false;
                    this.progressPreference = this.context.GetEnumPreference<ActionPreference>(SpecialVariables.ProgressPreferenceVarPath, this.progressPreference, out defaultUsed);
                    this.isProgressPreferenceCached = true;
                }
                return this.progressPreference;
            }
            set
            {
                this.progressPreference = value;
                this.isProgressPreferenceSet = true;
            }
        }

        internal SwitchParameter UseTransaction
        {
            get
            {
                return this.useTransactionFlag;
            }
            set
            {
                this.useTransactionFlag = (bool) value;
                this.useTransactionFlagSet = true;
            }
        }

        internal bool UseTransactionFlagSet
        {
            get
            {
                return this.useTransactionFlagSet;
            }
        }

        internal bool Verbose
        {
            get
            {
                return this.verboseFlag;
            }
            set
            {
                this.verboseFlag = value;
                this.isVerboseFlagSet = true;
            }
        }

        internal Pipe VerboseOutputPipe
        {
            get
            {
                return this.verboseOutputPipe;
            }
            set
            {
                this.verboseOutputPipe = value;
            }
        }

        internal ActionPreference VerbosePreference
        {
            get
            {
                if (this.isVerboseFlagSet)
                {
                    if (this.Verbose)
                    {
                        return ActionPreference.Continue;
                    }
                    return ActionPreference.SilentlyContinue;
                }
                if (this.Debug)
                {
                    if (this.CBhost.ExternalHost.UI == null)
                    {
                        return ActionPreference.Continue;
                    }
                    return ActionPreference.Inquire;
                }
                if (!this.isVerbosePreferenceCached)
                {
                    bool defaultUsed = false;
                    this.verbosePreference = this.context.GetEnumPreference<ActionPreference>(SpecialVariables.VerbosePreferenceVarPath, this.verbosePreference, out defaultUsed);
                }
                return this.verbosePreference;
            }
        }

        internal Pipe WarningOutputPipe
        {
            get
            {
                return this.warningOutputPipe;
            }
            set
            {
                this.warningOutputPipe = value;
            }
        }

        internal ActionPreference WarningPreference
        {
            get
            {
                if (!this.isWarningPreferenceSet)
                {
                    if (this.Debug)
                    {
                        return ActionPreference.Inquire;
                    }
                    if (this.Verbose)
                    {
                        return ActionPreference.Continue;
                    }
                    if (!this.isWarningPreferenceCached)
                    {
                        bool defaultUsed = false;
                        this.warningPreference = this.context.GetEnumPreference<ActionPreference>(SpecialVariables.WarningPreferenceVarPath, this.warningPreference, out defaultUsed);
                    }
                }
                return this.warningPreference;
            }
            set
            {
                this.warningPreference = value;
                this.isWarningPreferenceSet = true;
            }
        }

        internal string WarningVariable
        {
            get
            {
                return this.warningVariable;
            }
            set
            {
                this.warningVariable = value;
            }
        }

        internal SwitchParameter WhatIf
        {
            get
            {
                if (!this.isWhatIfFlagSet && !this.isWhatIfPreferenceCached)
                {
                    bool defaultUsed = false;
                    this.whatIfFlag = this.context.GetBooleanPreference(SpecialVariables.WhatIfPreferenceVarPath, this.whatIfFlag, out defaultUsed);
                    this.isWhatIfPreferenceCached = true;
                }
                return this.whatIfFlag;
            }
            set
            {
                this.whatIfFlag = (bool) value;
                this.isWhatIfFlagSet = true;
            }
        }

        private class AllowWrite : IDisposable
        {
            private PipelineProcessor _pp;
            private InternalCommand _wasPermittedToWrite;
            private Thread _wasPermittedToWriteThread;
            private bool _wasPermittedToWriteToPipeline;

            internal AllowWrite(InternalCommand permittedToWrite, bool permittedToWriteToPipeline)
            {
                if (permittedToWrite == null)
                {
                    throw PSTraceSource.NewArgumentNullException("permittedToWrite");
                }
                MshCommandRuntime commandRuntime = permittedToWrite.commandRuntime as MshCommandRuntime;
                if (commandRuntime == null)
                {
                    throw PSTraceSource.NewArgumentNullException("permittedToWrite.CommandRuntime");
                }
                this._pp = commandRuntime.PipelineProcessor;
                if (this._pp == null)
                {
                    throw PSTraceSource.NewArgumentNullException("permittedToWrite.CommandRuntime.PipelineProcessor");
                }
                this._wasPermittedToWrite = this._pp._permittedToWrite;
                this._wasPermittedToWriteToPipeline = this._pp._permittedToWriteToPipeline;
                this._wasPermittedToWriteThread = this._pp._permittedToWriteThread;
                this._pp._permittedToWrite = permittedToWrite;
                this._pp._permittedToWriteToPipeline = permittedToWriteToPipeline;
                this._pp._permittedToWriteThread = Thread.CurrentThread;
            }

            public void Dispose()
            {
                this._pp._permittedToWrite = this._wasPermittedToWrite;
                this._pp._permittedToWriteToPipeline = this._wasPermittedToWriteToPipeline;
                this._pp._permittedToWriteThread = this._wasPermittedToWriteThread;
                GC.SuppressFinalize(this);
            }
        }

        internal enum ContinueStatus
        {
            Yes,
            No,
            YesToAll,
            NoToAll
        }

        internal enum MergeDataStream
        {
            None,
            All,
            Output,
            Error,
            Warning,
            Verbose,
            Debug,
            Host
        }

        internal enum ShouldProcessPossibleOptimization
        {
            AutoYes_CanSkipShouldProcessCall,
            AutoYes_CanCallShouldProcessAsynchronously,
            AutoNo_CanCallShouldProcessAsynchronously,
            NoOptimizationPossible
        }
    }
}

