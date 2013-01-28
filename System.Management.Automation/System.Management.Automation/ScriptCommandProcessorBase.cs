namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation.Language;
    using System.Runtime.InteropServices;

    internal abstract class ScriptCommandProcessorBase : CommandProcessorBase
    {
        protected bool _dontUseScopeCommandOrigin;
        protected bool _exitWasCalled;
        protected bool _rethrowExitException;
        protected ScriptBlock _scriptBlock;
        private System.Management.Automation.ScriptParameterBinderController _scriptParameterBinderController;

        protected ScriptCommandProcessorBase(IScriptCommandInfo commandInfo, ExecutionContext context, bool useLocalScope, SessionStateInternal sessionState) : base((CommandInfo) commandInfo)
        {
            base._fromScriptFile = (base.CommandInfo is ExternalScriptInfo) || (base.CommandInfo is ScriptInfo);
            this._dontUseScopeCommandOrigin = true;
            this.CommonInitialization(commandInfo.ScriptBlock, context, useLocalScope, sessionState);
        }

        protected ScriptCommandProcessorBase(ScriptBlock scriptBlock, ExecutionContext context, bool useLocalScope, CommandOrigin origin, SessionStateInternal sessionState)
        {
            this._dontUseScopeCommandOrigin = false;
            base.CommandInfo = new ScriptInfo(string.Empty, scriptBlock, context);
            base._fromScriptFile = false;
            this.CommonInitialization(scriptBlock, context, useLocalScope, sessionState);
            base.Command.CommandOriginInternal = origin;
        }

        protected void CommonInitialization(ScriptBlock scriptBlock, ExecutionContext context, bool useLocalScope, SessionStateInternal sessionState)
        {
            base.CommandSessionState = sessionState;
            base._context = context;
            this._rethrowExitException = base.Context.ScriptCommandProcessorShouldRethrowExit;
            base._context.ScriptCommandProcessorShouldRethrowExit = false;
            ScriptCommand thisCommand = new ScriptCommand {
                CommandInfo = base.CommandInfo
            };
            base.Command = thisCommand;
            base.Command.commandRuntime = base.commandRuntime = new MshCommandRuntime(base.Context, base.CommandInfo, thisCommand);
            base.CommandScope = useLocalScope ? base.CommandSessionState.NewScope(base.FromScriptFile) : base.CommandSessionState.CurrentScope;
            base.UseLocalScope = useLocalScope;
            this._scriptBlock = scriptBlock;
            if (!base.UseLocalScope && !this._rethrowExitException)
            {
                CommandProcessorBase.ValidateCompatibleLanguageMode(this._scriptBlock, context.LanguageMode, base.Command.MyInvocation);
            }
        }

        internal override bool IsHelpRequested(out string helpTarget, out HelpCategory helpCategory)
        {
            if (((base.arguments != null) && (base.CommandInfo != null)) && (!string.IsNullOrEmpty(base.CommandInfo.Name) && (this._scriptBlock != null)))
            {
                foreach (CommandParameterInternal internal2 in base.arguments)
                {
                    if (internal2.IsDashQuestion())
                    {
                        string str;
                        Dictionary<Ast, Token[]> scriptBlockTokenCache = new Dictionary<Ast, Token[]>();
                        HelpInfo info = this._scriptBlock.GetHelpInfo(base.Context, base.CommandInfo, false, scriptBlockTokenCache, out str, out str);
                        if (info == null)
                        {
                            break;
                        }
                        helpTarget = info.Name;
                        helpCategory = info.HelpCategory;
                        return true;
                    }
                }
            }
            return base.IsHelpRequested(out helpTarget, out helpCategory);
        }

        internal System.Management.Automation.ScriptParameterBinderController ScriptParameterBinderController
        {
            get
            {
                if (this._scriptParameterBinderController == null)
                {
                    this._scriptParameterBinderController = new System.Management.Automation.ScriptParameterBinderController(((IScriptCommandInfo) base.CommandInfo).ScriptBlock, base.Command.MyInvocation, base.Context, base.Command, base.CommandScope);
                    this._scriptParameterBinderController.CommandLineParameters.UpdateInvocationInfo(base.Command.MyInvocation);
                    base.Command.MyInvocation.UnboundArguments = this._scriptParameterBinderController.DollarArgs;
                }
                return this._scriptParameterBinderController;
            }
        }
    }
}

