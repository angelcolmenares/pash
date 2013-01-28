namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Internal.Host;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Security;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public sealed class Debugger
    {
        private readonly Dictionary<string, Tuple<WeakReference, List<LineBreakpoint>>> _boundBreakpoints;
        private readonly List<CallStackInfo> _callStack;
        private readonly List<CommandBreakpoint> _commandBreakpoints;
        private readonly System.Management.Automation.ExecutionContext _context;
        private static readonly List<Breakpoint> _emptyBreakpointList = new List<Breakpoint>();
        private readonly Dictionary<int, Breakpoint> _idToBreakpoint;
        private readonly ConditionalWeakTable<CompiledScriptBlockData, Tuple<List<LineBreakpoint>, BitArray>> _mapScriptToBreakpoints = new ConditionalWeakTable<CompiledScriptBlockData, Tuple<List<LineBreakpoint>, BitArray>>();
        private CallStackInfo _overOrOutFrame;
        private List<LineBreakpoint> _pendingBreakpoints;
        private SteppingMode _steppingMode;
        private readonly Dictionary<string, List<VariableBreakpoint>> _variableBreakpoints;
        private bool savedIgnoreScriptDebug;

        public event EventHandler<BreakpointUpdatedEventArgs> BreakpointUpdated;

        public event EventHandler<DebuggerStopEventArgs> DebuggerStop;

        internal Debugger(System.Management.Automation.ExecutionContext context)
        {
            this._context = context;
            this.InBreakpoint = false;
            this._idToBreakpoint = new Dictionary<int, Breakpoint>();
            this._pendingBreakpoints = new List<LineBreakpoint>();
            this._boundBreakpoints = new Dictionary<string, Tuple<WeakReference, List<LineBreakpoint>>>(StringComparer.OrdinalIgnoreCase);
            this._commandBreakpoints = new List<CommandBreakpoint>();
            this._variableBreakpoints = new Dictionary<string, List<VariableBreakpoint>>(StringComparer.OrdinalIgnoreCase);
            this._steppingMode = SteppingMode.None;
            this._callStack = new List<CallStackInfo>();
        }

        internal void AddBreakpointCommon(Breakpoint breakpoint)
        {
            if (this._context._debuggingMode == 0)
            {
                this._context._debuggingMode = 1;
            }
            this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Set));
            this._idToBreakpoint[breakpoint.Id] = breakpoint;
        }

        private Breakpoint AddCommandBreakpoint(CommandBreakpoint breakpoint)
        {
            this.AddBreakpointCommon(breakpoint);
            this._commandBreakpoints.Add(breakpoint);
            return breakpoint;
        }

        private Breakpoint AddLineBreakpoint(LineBreakpoint breakpoint)
        {
            this.AddBreakpointCommon(breakpoint);
            this._pendingBreakpoints.Add(breakpoint);
            return breakpoint;
        }

        internal VariableBreakpoint AddVariableBreakpoint(VariableBreakpoint breakpoint)
        {
            List<VariableBreakpoint> list;
            this.AddBreakpointCommon(breakpoint);
            if (!this._variableBreakpoints.TryGetValue(breakpoint.Variable, out list))
            {
                list = new List<VariableBreakpoint>();
                this._variableBreakpoints.Add(breakpoint.Variable, list);
            }
            list.Add(breakpoint);
            return breakpoint;
        }

        internal bool CheckCommand(InvocationInfo invocationInfo)
        {
            FunctionContext context = (this._callStack.Count > 0) ? this._callStack.Last<CallStackInfo>().FunctionContext : null;
            if ((context != null) && context._scriptBlock.DebuggerHidden)
            {
                return false;
            }
            List<Breakpoint> source = (from bp in this._commandBreakpoints
                where bp.Enabled && bp.Trigger(invocationInfo)
                select bp).ToList<Breakpoint>();
            bool flag = true;
            if (source.Any<Breakpoint>())
            {
                source = this.TriggerBreakpoints(source);
                if (source.Any<Breakpoint>())
                {
                    InvocationInfo info = (context != null) ? new InvocationInfo(invocationInfo.MyCommand, context.CurrentPosition) : null;
                    this.OnDebuggerStop(info, source);
                    flag = false;
                }
            }
            return flag;
        }

        internal void CheckVariableRead(string variableName)
        {
            List<VariableBreakpoint> variableBreakpointsToTrigger = this.GetVariableBreakpointsToTrigger(variableName, true);
            if ((variableBreakpointsToTrigger != null) && variableBreakpointsToTrigger.Any<VariableBreakpoint>())
            {
                this.TriggerVariableBreakpoints(variableBreakpointsToTrigger);
            }
        }

        internal void CheckVariableWrite(string variableName)
        {
            List<VariableBreakpoint> variableBreakpointsToTrigger = this.GetVariableBreakpointsToTrigger(variableName, false);
            if ((variableBreakpointsToTrigger != null) && variableBreakpointsToTrigger.Any<VariableBreakpoint>())
            {
                this.TriggerVariableBreakpoints(variableBreakpointsToTrigger);
            }
        }

        internal void DisableBreakpoint(Breakpoint bp)
        {
            bp.SetEnabled(false);
            this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(bp, BreakpointUpdateType.Disabled));
        }

        internal void DisableTracing()
        {
            this._context.IgnoreScriptDebug = this.savedIgnoreScriptDebug;
            this._context.PSDebugTraceLevel = 0;
            this._context.PSDebugTraceStep = false;
            if (!this._idToBreakpoint.Any<KeyValuePair<int, Breakpoint>>())
            {
                this._context._debuggingMode = 0;
            }
        }

        internal void EnableBreakpoint(Breakpoint bp)
        {
            bp.SetEnabled(true);
            this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(bp, BreakpointUpdateType.Enabled));
        }

        internal void EnableTracing(int traceLevel, bool? step)
        {
            if ((traceLevel < 1) && (!step.HasValue || !step.Value))
            {
                this.DisableTracing();
            }
            else
            {
                this.savedIgnoreScriptDebug = this._context.IgnoreScriptDebug;
                this._context.IgnoreScriptDebug = false;
                this._context.PSDebugTraceLevel = traceLevel;
                if (step.HasValue)
                {
                    this._context.PSDebugTraceStep = step.Value;
                }
                this._context._debuggingMode = 1;
            }
        }

        private void EnterScriptFunction(FunctionContext functionContext)
        {
            InvocationInfo automaticVariable = (InvocationInfo) functionContext._localsTuple.GetAutomaticVariable(AutomaticVariable.MyInvocation);
            ScriptBlock block = functionContext._scriptBlock;
            CallStackInfo item = new CallStackInfo {
                InvocationInfo = automaticVariable,
                ScriptBlock = block,
                FunctionContext = functionContext,
                IsFrameHidden = block.DebuggerHidden
            };
            this._callStack.Add(item);
            if (this._context._debuggingMode > 0) //TODO: REVIEW: Why was 0??
            {
                ExternalScriptInfo myCommand = automaticVariable.MyCommand as ExternalScriptInfo;
                if (myCommand != null)
                {
                    this.RegisterScriptFile(myCommand);
                }
                bool flag = this.CheckCommand(automaticVariable);
                this.SetupBreakpoints(functionContext);
                if ((block.DebuggerStepThrough && (this._overOrOutFrame == null)) && (this._steppingMode == SteppingMode.StepIn))
                {
                    this.ResumeExecution(DebuggerResumeAction.StepOut);
                }
                if (flag)
                {
                    this.OnSequencePointHit(functionContext);
                }
                if (((this._context.PSDebugTraceLevel > 1) && !block.DebuggerStepThrough) && !block.DebuggerHidden)
                {
                    this.TraceScriptFunctionEntry(functionContext);
                }
            }
        }

        internal void ExitScriptFunction()
        {
            if (this._callStack.Last<CallStackInfo>() == this._overOrOutFrame)
            {
                this._overOrOutFrame = null;
            }
            this._callStack.RemoveAt(this._callStack.Count - 1);
            if (this._callStack.Count == 0)
            {
                this._steppingMode = SteppingMode.None;
            }
        }

        internal List<LineBreakpoint> GetBoundBreakpoints(ScriptBlock scriptBlock)
        {
            Tuple<List<LineBreakpoint>, BitArray> tuple;
            if (this._mapScriptToBreakpoints.TryGetValue(scriptBlock.ScriptBlockData, out tuple))
            {
                return tuple.Item1;
            }
            return null;
        }

        internal Breakpoint GetBreakpoint(int id)
        {
            Breakpoint breakpoint;
            this._idToBreakpoint.TryGetValue(id, out breakpoint);
            return breakpoint;
        }

        internal List<Breakpoint> GetBreakpoints()
        {
            return (from bp in this._idToBreakpoint.Values
                orderby bp.Id
                select bp).ToList<Breakpoint>();
        }

        internal IEnumerable<CallStackFrame> GetCallStack()
        {
            if (this._callStack.Count > 0)
            {
                int iteratorVariable0 = this._callStack.Count - 1;
                for (int j = iteratorVariable0; j >= 0; j--)
                {
                    if (this._callStack[j].TopFrameAtBreakpoint)
                    {
                        iteratorVariable0 = j;
                        break;
                    }
                }
                for (int i = iteratorVariable0; i >= 0; i--)
                {
                    FunctionContext functionContext = this._callStack[i].FunctionContext;
                    yield return new CallStackFrame(functionContext, this._callStack[i].InvocationInfo);
                }
            }
        }

        private List<VariableBreakpoint> GetVariableBreakpointsToTrigger(string variableName, bool read)
        {
            List<VariableBreakpoint> list2;
            FunctionContext context = (this._callStack.Count > 0) ? this._callStack.Last<CallStackInfo>().FunctionContext : null;
            if ((context != null) && context._scriptBlock.DebuggerHidden)
            {
                return null;
            }
            try
            {
                List<VariableBreakpoint> list;
                this._context._debuggingMode = 0;
                if (!this._variableBreakpoints.TryGetValue(variableName, out list) && variableName.Equals("_", StringComparison.OrdinalIgnoreCase))
                {
                    this._variableBreakpoints.TryGetValue("PSItem", out list);
                }
                if (list == null)
                {
                    return null;
                }
                string currentScriptFile = (this._callStack.Count > 0) ? this._callStack.Last<CallStackInfo>().ScriptBlock.File : null;
                list2 = (from bp in list
                    where bp.Trigger(currentScriptFile, read)
                    select bp).ToList<VariableBreakpoint>();
            }
            finally
            {
                this._context._debuggingMode = 1;
            }
            return list2;
        }

        internal Breakpoint NewCommandBreakpoint(string command, ScriptBlock action)
        {
            WildcardPattern pattern = new WildcardPattern(command, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            return this.AddCommandBreakpoint(new CommandBreakpoint(null, pattern, command, action));
        }

        internal Breakpoint NewCommandBreakpoint(string path, string command, ScriptBlock action)
        {
            WildcardPattern pattern = new WildcardPattern(command, WildcardOptions.IgnoreCase | WildcardOptions.Compiled);
            return this.AddCommandBreakpoint(new CommandBreakpoint(path, pattern, command, action));
        }

        internal Breakpoint NewLineBreakpoint(string path, int line, ScriptBlock action)
        {
            return this.AddLineBreakpoint(new LineBreakpoint(path, line, action));
        }

        internal Breakpoint NewStatementBreakpoint(string path, int line, int column, ScriptBlock action)
        {
            return this.AddLineBreakpoint(new LineBreakpoint(path, line, column, action));
        }

        internal Breakpoint NewVariableBreakpoint(string variableName, VariableAccessMode accessMode, ScriptBlock action)
        {
            return this.AddVariableBreakpoint(new VariableBreakpoint(null, variableName, accessMode, action));
        }

        internal Breakpoint NewVariableBreakpoint(string path, string variableName, VariableAccessMode accessMode, ScriptBlock action)
        {
            return this.AddVariableBreakpoint(new VariableBreakpoint(path, variableName, accessMode, action));
        }

        private void OnBreakpointUpdated(BreakpointUpdatedEventArgs e)
        {
            EventHandler<BreakpointUpdatedEventArgs> breakpointUpdated = this.BreakpointUpdated;
            if (breakpointUpdated != null)
            {
                breakpointUpdated(this, e);
            }
        }

        private void OnDebuggerStop(InvocationInfo invocationInfo, List<Breakpoint> breakpoints)
        {
            LocalRunspace currentRunspace = this._context.CurrentRunspace as LocalRunspace;
            if ((currentRunspace.PulsePipeline != null) && (currentRunspace.PulsePipeline == currentRunspace.GetCurrentlyRunningPipeline()))
            {
                this._context.EngineHostInterface.UI.WriteWarningLine((breakpoints.Count > 0) ? string.Format(CultureInfo.CurrentCulture, DebuggerStrings.WarningBreakpointWillNotBeHit, new object[] { breakpoints[0] }) : new InvalidOperationException().Message);
            }
            else
            {
                this._steppingMode = SteppingMode.None;
                EventHandler<DebuggerStopEventArgs> debuggerStop = this.DebuggerStop;
                if (debuggerStop != null)
                {
                    this.InBreakpoint = true;
                    this._context.SetVariable(SpecialVariables.PSDebugContextVarPath, new PSDebugContext(invocationInfo, breakpoints));
                    FunctionInfo baseObject = null;
                    bool flag = false;
                    try
                    {
                        Collection<PSObject> collection = this._context.SessionState.InvokeProvider.Item.Get(@"function:\prompt");
                        if ((collection != null) && (collection.Count > 0))
                        {
                            baseObject = collection[0].BaseObject as FunctionInfo;
                            if (string.Equals(baseObject.Definition, InitialSessionState.DefaultPromptString, StringComparison.OrdinalIgnoreCase))
                            {
                                flag = true;
                            }
                        }
                    }
                    catch (ItemNotFoundException)
                    {
                    }
                    if (flag)
                    {
                        string script = "\"[DBG]: PS $($executionContext.SessionState.Path.CurrentLocation)$('>' * ($nestedPromptLevel + 1)) \"";
                        baseObject.Update(ScriptBlock.Create(script), true, ScopedItemOptions.Unspecified);
                    }
                    PSLanguageMode languageMode = this._context.LanguageMode;
                    PSLanguageMode? nullable = null;
                    if (this._context.UseFullLanguageModeInDebugger && (this._context.LanguageMode != PSLanguageMode.FullLanguage))
                    {
                        nullable = new PSLanguageMode?(this._context.LanguageMode);
                        this._context.LanguageMode = PSLanguageMode.FullLanguage;
                    }
                    else if (SystemPolicy.GetSystemLockdownPolicy() == SystemEnforcementMode.Enforce)
                    {
                        nullable = new PSLanguageMode?(this._context.LanguageMode);
                        this._context.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                    }
                    RunspaceAvailability runspaceAvailability = this._context.CurrentRunspace.RunspaceAvailability;
                    this._context.CurrentRunspace.UpdateRunspaceAvailability((this._context.CurrentRunspace.GetCurrentlyRunningPipeline() != null) ? RunspaceAvailability.AvailableForNestedCommand : RunspaceAvailability.Available, true);
                    try
                    {
                        this._context._debuggingMode = -1;
                        if (this._callStack.Any<CallStackInfo>())
                        {
                            this._callStack.Last<CallStackInfo>().TopFrameAtBreakpoint = true;
                        }
                        DebuggerStopEventArgs e = new DebuggerStopEventArgs(invocationInfo, breakpoints);
                        debuggerStop(this, e);
                        this.ResumeExecution(e.ResumeAction);
                    }
                    finally
                    {
                        this._context._debuggingMode = 1;
                        if (this._callStack.Any<CallStackInfo>())
                        {
                            this._callStack.Last<CallStackInfo>().TopFrameAtBreakpoint = false;
                        }
                        this._context.CurrentRunspace.UpdateRunspaceAvailability(runspaceAvailability, true);
                        if (nullable.HasValue)
                        {
                            this._context.LanguageMode = nullable.Value;
                        }
                        this._context.EngineSessionState.RemoveVariable("PSDebugContext");
                        if (flag)
                        {
                            baseObject.Update(ScriptBlock.Create(InitialSessionState.DefaultPromptString), true, ScopedItemOptions.Unspecified);
                        }
                        this.InBreakpoint = false;
                    }
                }
            }
        }

        internal void OnSequencePointHit(FunctionContext functionContext)
        {
            Func<LineBreakpoint, bool> predicate = null;
            if ((this._context.ShouldTraceStatement && !this._callStack.Last<CallStackInfo>().IsFrameHidden) && !functionContext._scriptBlock.DebuggerStepThrough)
            {
                this.TraceLine(functionContext.CurrentPosition);
            }
            if ((this._steppingMode == SteppingMode.StepIn) && ((this._overOrOutFrame == null) || (this._callStack.Last<CallStackInfo>() == this._overOrOutFrame)))
            {
                if (!this._callStack.Last<CallStackInfo>().IsFrameHidden)
                {
                    this._overOrOutFrame = null;
                    this.StopOnSequencePoint(functionContext, _emptyBreakpointList);
                }
                else if (this._overOrOutFrame == null)
                {
                    this.ResumeExecution(DebuggerResumeAction.StepOut);
                }
            }
            else
            {
                if (functionContext._breakPoints == null)
                {
                    this.SetupBreakpoints(functionContext);
                }
                if (functionContext._breakPoints[functionContext._currentSequencePointIndex])
                {
                    if (predicate == null)
                    {
                        predicate = breakpoint => (breakpoint.SequencePointIndex == functionContext._currentSequencePointIndex) && breakpoint.Enabled;
                    }
                    List<Breakpoint> breakpoints = functionContext._boundBreakpoints.Where<LineBreakpoint>(predicate).ToList<Breakpoint>();
                    breakpoints = this.TriggerBreakpoints(breakpoints);
                    if (breakpoints.Any<Breakpoint>())
                    {
                        this.StopOnSequencePoint(functionContext, breakpoints);
                    }
                }
            }
        }

        internal void RegisterScriptFile(ExternalScriptInfo scriptCommandInfo)
        {
            this.RegisterScriptFile(scriptCommandInfo.Path, scriptCommandInfo.ScriptContents);
        }

        internal void RegisterScriptFile(string path, string scriptContents)
        {
            Tuple<WeakReference, List<LineBreakpoint>> tuple;
            if (!this._boundBreakpoints.TryGetValue(path, out tuple))
            {
                this._boundBreakpoints.Add(path, Tuple.Create<WeakReference, List<LineBreakpoint>>(new WeakReference(scriptContents), new List<LineBreakpoint>()));
            }
            else
            {
                string str;
                tuple.Item1.TryGetTarget<string>(out str);
                if ((str == null) || !str.Equals(scriptContents, StringComparison.Ordinal))
                {
                    this.UnbindBoundBreakpoints(tuple.Item2);
                    this._boundBreakpoints[path] = Tuple.Create<WeakReference, List<LineBreakpoint>>(new WeakReference(scriptContents), new List<LineBreakpoint>());
                }
            }
        }

        internal void RemoveBreakpoint(Breakpoint breakpoint)
        {
            if (this._idToBreakpoint.ContainsKey(breakpoint.Id))
            {
                this._idToBreakpoint.Remove(breakpoint.Id);
            }
            breakpoint.RemoveSelf(this);
            if (this._idToBreakpoint.Count == 0)
            {
                this._context._debuggingMode = 0;
            }
            this.OnBreakpointUpdated(new BreakpointUpdatedEventArgs(breakpoint, BreakpointUpdateType.Removed));
        }

        internal void RemoveCommandBreakpoint(CommandBreakpoint breakpoint)
        {
            this._commandBreakpoints.Remove(breakpoint);
        }

        internal void RemoveLineBreakpoint(LineBreakpoint breakpoint)
        {
            Tuple<WeakReference, List<LineBreakpoint>> tuple;
            this._pendingBreakpoints.Remove(breakpoint);
            if (this._boundBreakpoints.TryGetValue(breakpoint.Script, out tuple))
            {
                tuple.Item2.Remove(breakpoint);
            }
        }

        internal void RemoveVariableBreakpoint(VariableBreakpoint breakpoint)
        {
            this._variableBreakpoints[breakpoint.Variable].Remove(breakpoint);
        }

        private void ResumeExecution(DebuggerResumeAction action)
        {
            switch (action)
            {
                case DebuggerResumeAction.Continue:
                    break;

                case DebuggerResumeAction.StepInto:
                    this._steppingMode = SteppingMode.StepIn;
                    this._overOrOutFrame = null;
                    return;

                case DebuggerResumeAction.StepOut:
                    if (this._callStack.Count <= 1)
                    {
                        break;
                    }
                    this._steppingMode = SteppingMode.StepIn;
                    this._overOrOutFrame = this._callStack[this._callStack.Count - 2];
                    return;

                case DebuggerResumeAction.StepOver:
                    this._steppingMode = SteppingMode.StepIn;
                    this._overOrOutFrame = this._callStack.Last<CallStackInfo>();
                    return;

                case DebuggerResumeAction.Stop:
                    this._steppingMode = SteppingMode.None;
                    this._overOrOutFrame = null;
                    throw new TerminateException();

                default:
                    return;
            }
            this._steppingMode = SteppingMode.None;
            this._overOrOutFrame = null;
        }

        private void SetPendingBreakpoints(FunctionContext functionContext)
        {
            if (this._pendingBreakpoints.Any<LineBreakpoint>())
            {
                List<LineBreakpoint> list = new List<LineBreakpoint>();
                string file = functionContext._scriptBlock.File;
                if (file != null)
                {
                    Tuple<List<LineBreakpoint>, BitArray> tuple;
                    this.RegisterScriptFile(file, functionContext.CurrentPosition.StartScriptPosition.GetFullScript());
                    this._mapScriptToBreakpoints.TryGetValue(functionContext._scriptBlock.ScriptBlockData, out tuple);
                    foreach (LineBreakpoint breakpoint in this._pendingBreakpoints)
                    {
                        bool flag = false;
                        if (breakpoint.TrySetBreakpoint(file, functionContext))
                        {
                            if (this._context._debuggingMode == 0)
                            {
                                this._context._debuggingMode = 1;
                            }
                            flag = true;
                            tuple.Item1.Add(breakpoint);
                            this._boundBreakpoints[file].Item2.Add(breakpoint);
                        }
                        if (!flag)
                        {
                            list.Add(breakpoint);
                        }
                    }
                    this._pendingBreakpoints = list;
                }
            }
        }

        private void SetupBreakpoints(FunctionContext functionContext)
        {
            ScriptBlock scriptBlock = functionContext._scriptBlock;
            Tuple<List<LineBreakpoint>, BitArray> tuple = this._mapScriptToBreakpoints.GetValue(scriptBlock.ScriptBlockData, _ => Tuple.Create<List<LineBreakpoint>, BitArray>(new List<LineBreakpoint>(), new BitArray(scriptBlock.SequencePoints.Length)));
            functionContext._boundBreakpoints = tuple.Item1;
            functionContext._breakPoints = tuple.Item2;
            this.SetPendingBreakpoints(functionContext);
        }

        private void StopOnSequencePoint(FunctionContext functionContext, List<Breakpoint> breakpoints)
        {
            if (!functionContext._scriptBlock.DebuggerHidden && ((functionContext._sequencePoints.Length != 1) || !object.ReferenceEquals(functionContext._sequencePoints[0], functionContext._scriptBlock.Ast.Extent)))
            {
                InvocationInfo invocationInfo = new InvocationInfo(null, functionContext.CurrentPosition, this._context);
                this.OnDebuggerStop(invocationInfo, breakpoints);
            }
        }

        internal void Trace(string messageId, string resourceString, params object[] args)
        {
            string str;
            ActionPreference preference = ActionPreference.Continue;
            if ((args == null) || (args.Length == 0))
            {
                str = resourceString;
            }
            else
            {
                str = StringUtil.Format(resourceString, args);
            }
            if (string.IsNullOrEmpty(str))
            {
                str = "Could not load text for msh script tracing message id '" + messageId + "'";
            }
            ((InternalHostUserInterface) this._context.EngineHostInterface.UI).WriteDebugLine(str, ref preference);
        }

        internal void TraceLine(IScriptExtent extent)
        {
            string message = PositionUtilities.BriefMessage(extent.StartScriptPosition);
            InternalHostUserInterface uI = (InternalHostUserInterface) this._context.EngineHostInterface.UI;
            ActionPreference preference = this._context.PSDebugTraceStep ? ActionPreference.Inquire : ActionPreference.Continue;
            uI.WriteDebugLine(message, ref preference);
            if (preference == ActionPreference.Continue)
            {
                this._context.PSDebugTraceStep = false;
            }
        }

        internal void TraceScriptFunctionEntry(FunctionContext functionContext)
        {
            ScriptBlock block = functionContext._scriptBlock;
            string str = functionContext._functionName;
            if (string.IsNullOrEmpty(block.File))
            {
                this.Trace("TraceEnteringFunction", ParserStrings.TraceEnteringFunction, new object[] { str });
            }
            else
            {
                this.Trace("TraceEnteringFunctionDefinedInFile", ParserStrings.TraceEnteringFunctionDefinedInFile, new object[] { str, block.File });
            }
        }

        internal void TraceVariableSet(string varName, object value)
        {
            if (this._callStack.Any<CallStackInfo>() && (this._context.PSDebugTraceLevel <= 2))
            {
                ScriptBlock scriptBlock = this._callStack.Last<CallStackInfo>().ScriptBlock;
                if (scriptBlock.DebuggerHidden || scriptBlock.DebuggerStepThrough)
                {
                    return;
                }
            }
            string str = PSObject.ToStringParser(this._context, value);
            int length = 60 - varName.Length;
            if (str.Length > length)
            {
                str = str.Substring(0, length) + "...";
            }
            this.Trace("TraceVariableAssignment", ParserStrings.TraceVariableAssignment, new object[] { varName, str });
        }

        private List<Breakpoint> TriggerBreakpoints(List<Breakpoint> breakpoints)
        {
            List<Breakpoint> list = new List<Breakpoint>();
            try
            {
                this._context._debuggingMode = -1;
                foreach (Breakpoint breakpoint in breakpoints)
                {
                    if (breakpoint.Enabled && (breakpoint.Trigger() == Breakpoint.BreakpointAction.Break))
                    {
                        list.Add(breakpoint);
                    }
                }
            }
            finally
            {
                this._context._debuggingMode = 1;
            }
            return list;
        }

        internal void TriggerVariableBreakpoints(List<VariableBreakpoint> breakpoints)
        {
            FunctionContext context = (this._callStack.Count > 0) ? this._callStack.Last<CallStackInfo>().FunctionContext : null;
            InvocationInfo invocationInfo = (context != null) ? new InvocationInfo(null, context.CurrentPosition, this._context) : null;
            this.OnDebuggerStop(invocationInfo, breakpoints.ToList<Breakpoint>());
        }

        private void UnbindBoundBreakpoints(List<LineBreakpoint> boundBreakpoints)
        {
            foreach (LineBreakpoint breakpoint in boundBreakpoints)
            {
                breakpoint.ScriptBlock = null;
                breakpoint.SequencePointIndex = -1;
                breakpoint.BreakpointBitArray = null;
                this._pendingBreakpoints.Add(breakpoint);
            }
            boundBreakpoints.Clear();
        }

        internal bool InBreakpoint { get; private set; }

        

        [DebuggerDisplay("{FunctionContext.CurrentPosition}")]
        private class CallStackInfo
        {
            internal System.Management.Automation.Language.FunctionContext FunctionContext { get; set; }

            internal System.Management.Automation.InvocationInfo InvocationInfo { get; set; }

            internal bool IsFrameHidden { get; set; }

            internal System.Management.Automation.ScriptBlock ScriptBlock { get; set; }

            internal bool TopFrameAtBreakpoint { get; set; }
        }

        private enum SteppingMode
        {
            StepIn,
            None
        }
    }
}

