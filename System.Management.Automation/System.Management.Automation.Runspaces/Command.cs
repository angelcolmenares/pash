namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    public sealed class Command
    {
        private readonly string _command;
        private readonly System.Management.Automation.CommandInfo _commandInfo;
        private readonly bool _isScript;
        private PipelineResultTypes[] _mergeInstructions;
        private PipelineResultTypes _mergeMyResult;
        private PipelineResultTypes _mergeToResult;
        private PipelineResultTypes _mergeUnclaimedPreviousCommandResults;
        private readonly CommandParameterCollection _parameters;
        private bool? _useLocalScope;
        private bool isEndOfStatement;
        internal const int MaxMergeType = 4;

        internal Command(System.Management.Automation.CommandInfo commandInfo) : this(commandInfo, false)
        {
        }

        internal Command(Command command)
        {
            this._mergeInstructions = new PipelineResultTypes[4];
            this._parameters = new CommandParameterCollection();
            this._command = string.Empty;
            this._isScript = command._isScript;
            this._useLocalScope = command._useLocalScope;
            this._command = command._command;
            this._mergeInstructions = command._mergeInstructions;
            this._mergeMyResult = command._mergeMyResult;
            this._mergeToResult = command._mergeToResult;
            this._mergeUnclaimedPreviousCommandResults = command._mergeUnclaimedPreviousCommandResults;
            this.isEndOfStatement = command.isEndOfStatement;
            foreach (CommandParameter parameter in command.Parameters)
            {
                this.Parameters.Add(new CommandParameter(parameter.Name, parameter.Value));
            }
        }

        public Command(string command) : this(command, false, (bool?) null)
        {
        }

        internal Command(System.Management.Automation.CommandInfo commandInfo, bool isScript)
        {
            this._mergeInstructions = new PipelineResultTypes[4];
            this._parameters = new CommandParameterCollection();
            this._command = string.Empty;
            this._commandInfo = commandInfo;
            this._command = this._commandInfo.Name;
            this._isScript = isScript;
        }

        public Command(string command, bool isScript) : this(command, isScript, (bool?) null)
        {
        }

        internal Command(string command, bool isScript, bool? useLocalScope)
        {
            this._mergeInstructions = new PipelineResultTypes[4];
            this._parameters = new CommandParameterCollection();
            this._command = string.Empty;
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            this._command = command;
            this._isScript = isScript;
            this._useLocalScope = useLocalScope;
        }

        public Command(string command, bool isScript, bool useLocalScope)
        {
            this._mergeInstructions = new PipelineResultTypes[4];
            this._parameters = new CommandParameterCollection();
            this._command = string.Empty;
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            this._command = command;
            this._isScript = isScript;
            this._useLocalScope = new bool?(useLocalScope);
        }

        internal Command(string command, bool isScript, bool? useLocalScope, bool mergeUnclaimedPreviousErrorResults) : this(command, isScript, useLocalScope)
        {
            if (mergeUnclaimedPreviousErrorResults)
            {
                this._mergeUnclaimedPreviousCommandResults = PipelineResultTypes.Warning;
            }
        }

        internal Command Clone()
        {
            return new Command(this);
        }

        internal CommandProcessorBase CreateCommandProcessor(ExecutionContext executionContext, CommandFactory commandFactory, bool addToHistory, CommandOrigin origin)
        {
            CommandProcessorBase base2;
            string str2;
            HelpCategory category;
            if (!this.IsScript)
            {
                if (this._useLocalScope.HasValue && !this._useLocalScope.Value)
                {
                    switch (executionContext.LanguageMode)
                    {
                        case PSLanguageMode.RestrictedLanguage:
                        case PSLanguageMode.NoLanguage:
                            throw new RuntimeException(StringUtil.Format(RunspaceStrings.UseLocalScopeNotAllowed, new object[] { "UseLocalScope", PSLanguageMode.RestrictedLanguage.ToString(), PSLanguageMode.NoLanguage.ToString() }));
                    }
                }
                base2 = commandFactory.CreateCommand(this.CommandText, origin, this._useLocalScope);
            }
            else
            {
                if (executionContext.LanguageMode == PSLanguageMode.NoLanguage)
                {
                    throw InterpreterError.NewInterpreterException(this.CommandText, typeof(ParseException), null, "ScriptsNotAllowed", ParserStrings.ScriptsNotAllowed, new object[0]);
                }
                ScriptBlock function = executionContext.Engine.ParseScriptBlock(this.CommandText, addToHistory);
                switch (executionContext.LanguageMode)
                {
                    case PSLanguageMode.FullLanguage:
                    case PSLanguageMode.ConstrainedLanguage:
                        break;

                    case PSLanguageMode.RestrictedLanguage:
                        function.CheckRestrictedLanguage(null, null, false);
                        break;

                    default:
                        throw new InvalidOperationException("Invalid langage mode was set when building a ScriptCommandProcessor");
                }
                if (function.UsesCmdletBinding)
                {
                    FunctionInfo scriptCommandInfo = new FunctionInfo("", function, executionContext);
                    bool? nullable = this._useLocalScope;
                    base2 = new CommandProcessor(scriptCommandInfo, executionContext, nullable.HasValue ? nullable.GetValueOrDefault() : false, false, executionContext.EngineSessionState);
                }
                else
                {
                    bool? nullable2 = this._useLocalScope;
                    base2 = new DlrScriptCommandProcessor(function, executionContext, nullable2.HasValue ? nullable2.GetValueOrDefault() : false, CommandOrigin.Runspace, executionContext.EngineSessionState);
                }
            }
            CommandParameterCollection parameters = this.Parameters;
            if (parameters != null)
            {
                bool forNativeCommand = base2 is NativeCommandProcessor;
                foreach (CommandParameter parameter in parameters)
                {
                    CommandParameterInternal internal2 = CommandParameter.ToCommandParameterInternal(parameter, forNativeCommand);
                    base2.AddParameter(internal2);
                }
            }
            if (base2.IsHelpRequested(out str2, out category))
            {
                base2 = CommandProcessorBase.CreateGetHelpCommandProcessor(executionContext, str2, category);
            }
            this.SetMergeSettingsOnCommandProcessor(base2);
            return base2;
        }

        internal static Command FromPSObjectForRemoting(PSObject commandAsPSObject)
        {
            if (commandAsPSObject == null)
            {
                throw PSTraceSource.NewArgumentNullException("commandAsPSObject");
            }
            string propertyValue = RemotingDecoder.GetPropertyValue<string>(commandAsPSObject, "Cmd");
            bool isScript = RemotingDecoder.GetPropertyValue<bool>(commandAsPSObject, "IsScript");
            bool? useLocalScope = RemotingDecoder.GetPropertyValue<bool?>(commandAsPSObject, "UseLocalScope");
            Command command = new Command(propertyValue, isScript, useLocalScope);
            PipelineResultTypes myResult = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergeMyResult");
            PipelineResultTypes toResult = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergeToResult");
            command.MergeMyResults(myResult, toResult);
            command.MergeUnclaimedPreviousCommandResults = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergePreviousResults");
            if (commandAsPSObject.Properties["MergeError"] != null)
            {
                command.MergeInstructions[0] = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergeError");
            }
            if (commandAsPSObject.Properties["MergeWarning"] != null)
            {
                command.MergeInstructions[1] = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergeWarning");
            }
            if (commandAsPSObject.Properties["MergeVerbose"] != null)
            {
                command.MergeInstructions[2] = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergeVerbose");
            }
            if (commandAsPSObject.Properties["MergeDebug"] != null)
            {
                command.MergeInstructions[3] = RemotingDecoder.GetPropertyValue<PipelineResultTypes>(commandAsPSObject, "MergeDebug");
            }
            foreach (PSObject obj2 in RemotingDecoder.EnumerateListProperty<PSObject>(commandAsPSObject, "Args"))
            {
                command.Parameters.Add(CommandParameter.FromPSObjectForRemoting(obj2));
            }
            return command;
        }

        private Pipe GetRedirectionPipe(PipelineResultTypes toType, MshCommandRuntime mcr)
        {
            if (toType == PipelineResultTypes.Output)
            {
                return mcr.OutputPipe;
            }
            return new Pipe { NullPipe = true };
        }

        public void MergeMyResults(PipelineResultTypes myResult, PipelineResultTypes toResult)
        {
            if ((myResult == PipelineResultTypes.None) && (toResult == PipelineResultTypes.None))
            {
                this._mergeMyResult = myResult;
                this._mergeToResult = toResult;
                for (int i = 0; i < 4; i++)
                {
                    this._mergeInstructions[i] = PipelineResultTypes.None;
                }
            }
            else
            {
                if ((myResult == PipelineResultTypes.None) || (myResult == PipelineResultTypes.Output))
                {
                    throw PSTraceSource.NewArgumentException("myResult", "RunspaceStrings", "InvalidMyResultError", new object[0]);
                }
                if ((myResult == PipelineResultTypes.Error) && (toResult != PipelineResultTypes.Output))
                {
                    throw PSTraceSource.NewArgumentException("toResult", "RunspaceStrings", "InvalidValueToResultError", new object[0]);
                }
                if ((toResult != PipelineResultTypes.Output) && (toResult != PipelineResultTypes.Null))
                {
                    throw PSTraceSource.NewArgumentException("toResult", "RunspaceStrings", "InvalidValueToResult", new object[0]);
                }
                if (myResult == PipelineResultTypes.Error)
                {
                    this._mergeMyResult = myResult;
                    this._mergeToResult = toResult;
                }
                if ((myResult == PipelineResultTypes.Error) || (myResult == PipelineResultTypes.All))
                {
                    this._mergeInstructions[0] = toResult;
                }
                if ((myResult == PipelineResultTypes.Warning) || (myResult == PipelineResultTypes.All))
                {
                    this._mergeInstructions[1] = toResult;
                }
                if ((myResult == PipelineResultTypes.Verbose) || (myResult == PipelineResultTypes.All))
                {
                    this._mergeInstructions[2] = toResult;
                }
                if ((myResult == PipelineResultTypes.Debug) || (myResult == PipelineResultTypes.All))
                {
                    this._mergeInstructions[3] = toResult;
                }
            }
        }

        private void SetMergeSettingsOnCommandProcessor(CommandProcessorBase commandProcessor)
        {
            MshCommandRuntime commandRuntime = commandProcessor.Command.commandRuntime as MshCommandRuntime;
            if ((this._mergeUnclaimedPreviousCommandResults != PipelineResultTypes.None) && (commandRuntime != null))
            {
                commandRuntime.MergeUnclaimedPreviousErrorResults = true;
            }
            if (this._mergeInstructions[0] == PipelineResultTypes.Output)
            {
                commandRuntime.ErrorMergeTo = MshCommandRuntime.MergeDataStream.Output;
            }
            PipelineResultTypes toType = this._mergeInstructions[1];
            if (toType != PipelineResultTypes.None)
            {
                commandRuntime.WarningOutputPipe = this.GetRedirectionPipe(toType, commandRuntime);
            }
            toType = this._mergeInstructions[2];
            if (toType != PipelineResultTypes.None)
            {
                commandRuntime.VerboseOutputPipe = this.GetRedirectionPipe(toType, commandRuntime);
            }
            toType = this._mergeInstructions[3];
            if (toType != PipelineResultTypes.None)
            {
                commandRuntime.DebugOutputPipe = this.GetRedirectionPipe(toType, commandRuntime);
            }
        }

        internal PSObject ToPSObjectForRemoting(Version psRPVersion)
        {
            PSObject obj2 = RemotingEncoder.CreateEmptyPSObject();
            obj2.Properties.Add(new PSNoteProperty("Cmd", this.CommandText));
            obj2.Properties.Add(new PSNoteProperty("IsScript", this.IsScript));
            obj2.Properties.Add(new PSNoteProperty("UseLocalScope", this.UseLocalScopeNullable));
            obj2.Properties.Add(new PSNoteProperty("MergeMyResult", this.MergeMyResult));
            obj2.Properties.Add(new PSNoteProperty("MergeToResult", this.MergeToResult));
            obj2.Properties.Add(new PSNoteProperty("MergePreviousResults", this.MergeUnclaimedPreviousCommandResults));
            if ((psRPVersion != null) && (psRPVersion >= RemotingConstants.ProtocolVersionWin8RTM))
            {
                obj2.Properties.Add(new PSNoteProperty("MergeError", this._mergeInstructions[0]));
                obj2.Properties.Add(new PSNoteProperty("MergeWarning", this._mergeInstructions[1]));
                obj2.Properties.Add(new PSNoteProperty("MergeVerbose", this._mergeInstructions[2]));
                obj2.Properties.Add(new PSNoteProperty("MergeDebug", this._mergeInstructions[3]));
            }
            else
            {
                if (this._mergeInstructions[1] == PipelineResultTypes.Output)
                {
                    throw new RuntimeException(StringUtil.Format(RunspaceStrings.WarningRedirectionNotSupported, new object[0]));
                }
                if (this._mergeInstructions[2] == PipelineResultTypes.Output)
                {
                    throw new RuntimeException(StringUtil.Format(RunspaceStrings.VerboseRedirectionNotSupported, new object[0]));
                }
                if (this._mergeInstructions[3] == PipelineResultTypes.Output)
                {
                    throw new RuntimeException(StringUtil.Format(RunspaceStrings.DebugRedirectionNotSupported, new object[0]));
                }
            }
            List<PSObject> list = new List<PSObject>(this.Parameters.Count);
            foreach (CommandParameter parameter in this.Parameters)
            {
                list.Add(parameter.ToPSObjectForRemoting());
            }
            obj2.Properties.Add(new PSNoteProperty("Args", list));
            return obj2;
        }

        public override string ToString()
        {
            return this._command;
        }

        internal System.Management.Automation.CommandInfo CommandInfo
        {
            get
            {
                return this._commandInfo;
            }
        }

        public string CommandText
        {
            get
            {
                return this._command;
            }
        }

        internal bool IsEndOfStatement
        {
            get
            {
                return this.isEndOfStatement;
            }
            set
            {
                this.isEndOfStatement = value;
            }
        }

        public bool IsScript
        {
            get
            {
                return this._isScript;
            }
        }

        internal PipelineResultTypes[] MergeInstructions
        {
            get
            {
                return this._mergeInstructions;
            }
            set
            {
                this._mergeInstructions = value;
            }
        }

        internal PipelineResultTypes MergeMyResult
        {
            get
            {
                return this._mergeMyResult;
            }
        }

        internal PipelineResultTypes MergeToResult
        {
            get
            {
                return this._mergeToResult;
            }
        }

        public PipelineResultTypes MergeUnclaimedPreviousCommandResults
        {
            get
            {
                return this._mergeUnclaimedPreviousCommandResults;
            }
            set
            {
                if (value == PipelineResultTypes.None)
                {
                    this._mergeUnclaimedPreviousCommandResults = value;
                }
                else
                {
                    if (value != PipelineResultTypes.Warning)
                    {
                        throw PSTraceSource.NewNotSupportedException();
                    }
                    this._mergeUnclaimedPreviousCommandResults = value;
                }
            }
        }

        public CommandParameterCollection Parameters
        {
            get
            {
                return this._parameters;
            }
        }

        public bool UseLocalScope
        {
            get
            {
                bool? nullable = this._useLocalScope;
                if (!nullable.HasValue)
                {
                    return false;
                }
                return nullable.GetValueOrDefault();
            }
        }

        internal bool? UseLocalScopeNullable
        {
            get
            {
                return this._useLocalScope;
            }
        }

        internal enum MergeType
        {
            Error,
            Warning,
            Verbose,
            Debug
        }
    }
}

