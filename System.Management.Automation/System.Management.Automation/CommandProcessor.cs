namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Sqm;
    using System.Runtime.InteropServices;

    internal class CommandProcessor : CommandProcessorBase
    {
        private bool _bailInNextCall;
        private System.Management.Automation.CmdletParameterBinderController _cmdletParameterBinderController;
        private bool _firstCallToRead;
        private static readonly ConcurrentDictionary<Type, Func<Cmdlet>> ConstructInstanceCache = new ConcurrentDictionary<Type, Func<Cmdlet>>();

        internal CommandProcessor(CmdletInfo cmdletInfo, ExecutionContext context) : base(cmdletInfo)
        {
            this._firstCallToRead = true;
            base._context = context;
            this.Init(cmdletInfo);
        }

        internal CommandProcessor(IScriptCommandInfo scriptCommandInfo, ExecutionContext context, bool useLocalScope, bool fromScriptFile, SessionStateInternal sessionState) : base(scriptCommandInfo as CommandInfo)
        {
            this._firstCallToRead = true;
            base._context = context;
            base._useLocalScope = useLocalScope;
            base._fromScriptFile = fromScriptFile;
            base.CommandSessionState = sessionState;
            this.Init(scriptCommandInfo);
        }

        internal void BindCommandLineParameters()
        {
            using (base.commandRuntime.AllowThisCommandToWrite(false))
            {
                this.CmdletParameterBinderController.CommandLineParameters.UpdateInvocationInfo(base.Command.MyInvocation);
                base.Command.MyInvocation.UnboundArguments = new List<object>();
                this.CmdletParameterBinderController.BindCommandLineParameters(base.arguments);
            }
        }

        private static Cmdlet ConstructInstance(Type type)
        {
            return ConstructInstanceCache.GetOrAdd(type, t => Expression.Lambda<Func<Cmdlet>>(typeof(Cmdlet).IsAssignableFrom(t) ? ((Expression) Expression.New(t)) : ((Expression) Expression.Constant(null, typeof(Cmdlet))), new ParameterExpression[0]).Compile())();
        }

        private void Init(CmdletInfo cmdletInformation)
        {
            Cmdlet cmdlet = null;
            Exception exception = null;
            string str = null;
            string cmdletDoesNotDeriveFromCmdletType = null;
            try
            {
                cmdlet = ConstructInstance(cmdletInformation.ImplementingType);
                if (cmdlet == null)
                {
                    exception = new InvalidCastException();
                    str = "CmdletDoesNotDeriveFromCmdletType";
                    cmdletDoesNotDeriveFromCmdletType = DiscoveryExceptions.CmdletDoesNotDeriveFromCmdletType;
                }
            }
            catch (MemberAccessException exception2)
            {
                exception = exception2;
            }
            catch (TypeLoadException exception3)
            {
                exception = exception3;
            }
            catch (Exception exception4)
            {
                CommandProcessorBase.CheckForSevereException(exception4);
                CmdletInvocationException exception5 = new CmdletInvocationException(exception4, null);
                MshLog.LogCommandHealthEvent(base._context, exception5, Severity.Warning);
                throw exception5;
            }
            if (exception != null)
            {
                MshLog.LogCommandHealthEvent(base._context, exception, Severity.Warning);
                CommandNotFoundException exception6 = new CommandNotFoundException(cmdletInformation.Name, exception, str ?? "CmdletNotFoundException", cmdletDoesNotDeriveFromCmdletType ?? DiscoveryExceptions.CmdletNotFoundException, new object[] { exception.Message });
                throw exception6;
            }
            base.Command = cmdlet;
            base.CommandScope = base.Context.EngineSessionState.CurrentScope;
            this.InitCommon();
        }

        private void Init(IScriptCommandInfo scriptCommandInfo)
        {
            InternalCommand command = new PSScriptCmdlet(scriptCommandInfo.ScriptBlock, base._useLocalScope, base.FromScriptFile, base._context);
            base.Command = command;
            base.CommandScope = base._useLocalScope ? base.CommandSessionState.NewScope(base._fromScriptFile) : base.CommandSessionState.CurrentScope;
            this.InitCommon();
            if (!base.UseLocalScope)
            {
                CommandProcessorBase.ValidateCompatibleLanguageMode(scriptCommandInfo.ScriptBlock, base._context.LanguageMode, base.Command.MyInvocation);
            }
        }

        private void InitCommon()
        {
            base.Command.CommandInfo = base.CommandInfo;
            base.Command.Context = base._context;
            try
            {
                base.commandRuntime = new MshCommandRuntime(base._context, base.CommandInfo, base.Command);
                base.Command.commandRuntime = base.commandRuntime;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                MshLog.LogCommandHealthEvent(base._context, exception, Severity.Warning);
                throw;
            }
        }

        internal override bool IsHelpRequested(out string helpTarget, out HelpCategory helpCategory)
        {
            if (base.arguments != null)
            {
                foreach (CommandParameterInternal internal2 in base.arguments)
                {
                    if (internal2.IsDashQuestion())
                    {
                        helpCategory = HelpCategory.All;
                        if (((base.Command != null) && (base.Command.MyInvocation != null)) && !string.IsNullOrEmpty(base.Command.MyInvocation.InvocationName))
                        {
                            helpTarget = base.Command.MyInvocation.InvocationName;
                            if (string.Equals(base.Command.MyInvocation.InvocationName, base.CommandInfo.Name, StringComparison.OrdinalIgnoreCase))
                            {
                                helpCategory = base.CommandInfo.HelpCategory;
                            }
                        }
                        else
                        {
                            helpTarget = base.CommandInfo.Name;
                            helpCategory = base.CommandInfo.HelpCategory;
                        }
                        return true;
                    }
                }
            }
            return base.IsHelpRequested(out helpTarget, out helpCategory);
        }

        internal ParameterBinderController NewParameterBinderController(InternalCommand command)
        {
            ParameterBinderBase base2;
            Cmdlet cmdlet = command as Cmdlet;
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentException("command");
            }
            IScriptCommandInfo commandInfo = base.CommandInfo as IScriptCommandInfo;
            if (commandInfo != null)
            {
                base2 = new ScriptParameterBinder(commandInfo.ScriptBlock, cmdlet.MyInvocation, base._context, cmdlet, base.CommandScope);
            }
            else
            {
                base2 = new ReflectionParameterBinder(cmdlet, cmdlet);
            }
            this._cmdletParameterBinderController = new System.Management.Automation.CmdletParameterBinderController(cmdlet, base.CommandInfo.CommandMetadata, base2);
            return this._cmdletParameterBinderController;
        }

        internal override void Prepare(IDictionary psDefaultParameterValues)
        {
            this.CmdletParameterBinderController.DefaultParameterValues = psDefaultParameterValues;
            CmdletInfo commandInfo = base.CommandInfo as CmdletInfo;
            if (commandInfo != null)
            {
                PSSQMAPI.IncrementData(commandInfo);
            }
            this.BindCommandLineParameters();
        }

        private bool ProcessInputPipelineObject(object inputObject)
        {
            PSObject inputToOperateOn = null;
            if (inputObject != null)
            {
                inputToOperateOn = PSObject.AsPSObject(inputObject);
            }
            base.Command.CurrentPipelineObject = inputToOperateOn;
            return this.CmdletParameterBinderController.BindPipelineParameters(inputToOperateOn);
        }

        internal override void ProcessRecord()
        {
            Pipe pipe;
            if (base.RanBeginAlready)
            {
                goto Label_0151;
            }
            base.RanBeginAlready = true;
            try
            {
                using (base.commandRuntime.AllowThisCommandToWrite(true))
                {
                    if ((base.Context._debuggingMode > 0) && !(base.Command is PSScriptCmdlet))
                    {
                        base.Context.Debugger.CheckCommand(base.Command.MyInvocation);
                    }
                    base.Command.DoBeginProcessing();
                }
                goto Label_0151;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw base.ManageInvocationException(exception);
            }
        Label_0084:
            pipe = base._context.ShellFunctionErrorOutputPipe;
            Exception e = null;
            try
            {
                if (base.RedirectShellErrorOutputPipe || (base._context.ShellFunctionErrorOutputPipe != null))
                {
                    base._context.ShellFunctionErrorOutputPipe = base.commandRuntime.ErrorOutputPipe;
                }
                using (base.commandRuntime.AllowThisCommandToWrite(true))
                {
                    base.Command.MyInvocation.PipelineIterationInfo[base.Command.MyInvocation.PipelinePosition]++;
                    base.Command.DoProcessRecord();
                }
            }
            catch (RuntimeException exception3)
            {
                if (exception3.WasThrownFromThrowStatement)
                {
                    throw;
                }
                e = exception3;
            }
            catch (LoopFlowException)
            {
                throw;
            }
            catch (Exception exception4)
            {
                e = exception4;
            }
            finally
            {
                base._context.ShellFunctionErrorOutputPipe = pipe;
            }
            if (e != null)
            {
                CommandProcessorBase.CheckForSevereException(e);
                throw base.ManageInvocationException(e);
            }
        Label_0151:
            if (this.Read())
            {
                goto Label_0084;
            }
        }

        internal sealed override bool Read()
        {
            if (this._bailInNextCall)
            {
                return false;
            }
            base.Command.ThrowIfStopping();
            if (this._firstCallToRead)
            {
                this._firstCallToRead = false;
                if (!base.IsPipelineInputExpected())
                {
                    this._bailInNextCall = true;
                    return true;
                }
            }
            bool flag = false;
            while (!flag)
            {
                Collection<MergedCompiledCommandParameter> collection;
                object inputObject = base.commandRuntime.InputPipe.Retrieve();
                if (inputObject == AutomationNull.Value)
                {
                    base.Command.CurrentPipelineObject = null;
                    return false;
                }
                if (base.Command.MyInvocation.PipelinePosition == 1)
                {
                    base.Command.MyInvocation.PipelineIterationInfo[0]++;
                }
                try
                {
                    if (!this.ProcessInputPipelineObject(inputObject))
                    {
                        this.WriteInputObjectError(inputObject, "InputObjectNotBound", new object[0]);
                        continue;
                    }
                }
                catch (ParameterBindingException exception)
                {
                    exception.ErrorRecord.SetTargetObject(inputObject);
                    ErrorRecord errorRecord = new ErrorRecord(exception.ErrorRecord, exception);
                    ActionPreference? actionPreference = null;
                    base.commandRuntime._WriteErrorSkipAllowCheck(errorRecord, actionPreference);
                    continue;
                }
                using (ParameterBinderBase.bindingTracer.TraceScope("MANDATORY PARAMETER CHECK on cmdlet [{0}]", new object[] { base.CommandInfo.Name }))
                {
                    flag = this.CmdletParameterBinderController.HandleUnboundMandatoryParameters(out collection);
                }
                if (!flag)
                {
                    string str = System.Management.Automation.CmdletParameterBinderController.BuildMissingParamsString(collection);
                    this.WriteInputObjectError(inputObject, "InputObjectMissingMandatory", new object[] { str });
                }
            }
            return true;
        }

        private void WriteInputObjectError(object inputObject, string resourceAndErrorId, params object[] args)
        {
            Type typeSpecified = (inputObject == null) ? null : inputObject.GetType();
            ParameterBindingException exception = new ParameterBindingException(ErrorCategory.InvalidArgument, base.Command.MyInvocation, null, null, null, typeSpecified, "ParameterBinderStrings", resourceAndErrorId, args);
            ErrorRecord errorRecord = new ErrorRecord(exception, resourceAndErrorId, ErrorCategory.InvalidArgument, inputObject);
            errorRecord.SetInvocationInfo(base.Command.MyInvocation);
            base.commandRuntime._WriteErrorSkipAllowCheck(errorRecord, null);
        }

        internal System.Management.Automation.CmdletParameterBinderController CmdletParameterBinderController
        {
            get
            {
                if (this._cmdletParameterBinderController == null)
                {
                    this.NewParameterBinderController(base.Command);
                }
                return this._cmdletParameterBinderController;
            }
        }
    }
}

