namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    public abstract class CommandInfo : IHasSessionStateEntryVisibility
    {
        private object[] _arguments;
        private ExecutionContext _context;
        private CommandInfo _copiedCommand;
        internal System.Management.Automation.CommandMetadata _externalCommandMetadata;
        private bool _isImported;
        private PSModuleInfo _module;
        private string _name;
        internal ReadOnlyCollection<CommandParameterSetInfo> _parameterSets;
        private string _prefix;
        private CommandTypes _type;
        private SessionStateEntryVisibility _visibility;
        internal const int HasWorkflowKeyWord = 8;
        internal const int IsCimCommand = 0x10;
        internal const int IsFile = 0x20;

        internal CommandInfo(CommandInfo other)
        {
            this._name = string.Empty;
            this._type = CommandTypes.Application;
            this._prefix = "";
            this._module = other._module;
            this._visibility = other._visibility;
            this._arguments = other._arguments;
            this.Context = other.Context;
            this._name = other._name;
            this._type = other._type;
            this._copiedCommand = other;
            this.DefiningLanguageMode = other.DefiningLanguageMode;
        }

        internal CommandInfo(string name, CommandInfo other) : this(other)
        {
            this._name = name;
        }

        internal CommandInfo(string name, CommandTypes type)
        {
            this._name = string.Empty;
            this._type = CommandTypes.Application;
            this._prefix = "";
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this._name = name;
            this._type = type;
        }

        internal CommandInfo(string name, CommandTypes type, ExecutionContext context) : this(name, type)
        {
            this.Context = context;
        }

        internal virtual CommandInfo CreateGetCommandCopy(object[] argumentList)
        {
            throw new InvalidOperationException();
        }

        internal Collection<CommandParameterSetInfo> GenerateCommandParameterSetInfo()
        {
            if (this.IsGetCommandCopy && this.ImplementsDynamicParameters)
            {
                return GetParameterMetadata(this.CommandMetadata, this.GetMergedCommandParameterMetdata());
            }
            return GetCacheableMetadata(this.CommandMetadata);
        }

        internal static Collection<CommandParameterSetInfo> GetCacheableMetadata(System.Management.Automation.CommandMetadata metadata)
        {
            return GetParameterMetadata(metadata, metadata.StaticCommandParameterMetadata);
        }

        private MergedCommandParameterMetadata GetMergedCommandParameterMetdata()
        {
            if (this._context == null)
            {
                return null;
            }
            IScriptCommandInfo scriptCommandInfo = this as IScriptCommandInfo;
            CommandProcessor commandProcessor = (scriptCommandInfo != null) ? new CommandProcessor(scriptCommandInfo, this._context, true, false, scriptCommandInfo.ScriptBlock.SessionStateInternal ?? this.Context.EngineSessionState) : new CommandProcessor((CmdletInfo) this, this._context);
            ParameterBinderController.AddArgumentsToCommandProcessor(commandProcessor, this.Arguments);
            CommandProcessorBase currentCommandProcessor = this.Context.CurrentCommandProcessor;
            try
            {
                this.Context.CurrentCommandProcessor = commandProcessor;
                commandProcessor.SetCurrentScopeToExecutionScope();
                commandProcessor.CmdletParameterBinderController.BindCommandLineParametersNoValidation(commandProcessor.arguments);
            }
            catch (ParameterBindingException)
            {
                if (commandProcessor.arguments.Count > 0)
                {
                    throw;
                }
            }
            finally
            {
                this.Context.CurrentCommandProcessor = currentCommandProcessor;
                commandProcessor.RestorePreviousScope();
            }
            return commandProcessor.CmdletParameterBinderController.BindableParameters;
        }

        internal static Collection<CommandParameterSetInfo> GetParameterMetadata(System.Management.Automation.CommandMetadata metadata, MergedCommandParameterMetadata parameterMetadata)
        {
            Collection<CommandParameterSetInfo> collection = new Collection<CommandParameterSetInfo>();
            if (parameterMetadata != null)
            {
                if (parameterMetadata.ParameterSetCount == 0)
                {
                    collection.Add(new CommandParameterSetInfo("__AllParameterSets", false, int.MaxValue, parameterMetadata));
                    return collection;
                }
                int parameterSetCount = parameterMetadata.ParameterSetCount;
                for (int i = 0; i < parameterSetCount; i++)
                {
                    int parameterSet = ((int) 1) << i;
                    string parameterSetName = parameterMetadata.GetParameterSetName(parameterSet);
                    bool isDefaultParameterSet = (parameterSet & metadata.DefaultParameterSetFlag) != 0;
                    collection.Add(new CommandParameterSetInfo(parameterSetName, isDefaultParameterSet, parameterSet, parameterMetadata));
                }
            }
            return collection;
        }

        internal void Rename(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                throw new ArgumentNullException("newName");
            }
            this._name = newName;
        }

        public ParameterMetadata ResolveParameter(string name)
        {
            MergedCompiledCommandParameter parameter = this.GetMergedCommandParameterMetdata().GetMatchingParameter(name, true, true, null);
            return this.Parameters[parameter.Parameter.Name];
        }

        internal void SetCommandType(CommandTypes newType)
        {
            this._type = newType;
        }

        internal void SetModule(PSModuleInfo module)
        {
            this._module = module;
        }

        public override string ToString()
        {
            return this._name;
        }

        internal object[] Arguments
        {
            get
            {
                return this._arguments;
            }
            set
            {
                this._arguments = value;
            }
        }

        internal virtual System.Management.Automation.CommandMetadata CommandMetadata
        {
            get
            {
                throw new InvalidOperationException();
            }
        }

        public CommandTypes CommandType
        {
            get
            {
                return this._type;
            }
        }

        internal ExecutionContext Context
        {
            get
            {
                return this._context;
            }
            set
            {
                this._context = value;
                if ((value != null) && !this.DefiningLanguageMode.HasValue)
                {
                    this.DefiningLanguageMode = new PSLanguageMode?(value.LanguageMode);
                }
            }
        }

        internal CommandInfo CopiedCommand
        {
            get
            {
                return this._copiedCommand;
            }
            set
            {
                this._copiedCommand = value;
            }
        }

        internal PSLanguageMode? DefiningLanguageMode { get; set; }

        public abstract string Definition { get; }

        private System.Management.Automation.CommandMetadata ExternalCommandMetadata
        {
            get
            {
                if (this._externalCommandMetadata == null)
                {
                    this._externalCommandMetadata = new System.Management.Automation.CommandMetadata(this, true);
                }
                return this._externalCommandMetadata;
            }
        }

        internal virtual System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.None;
            }
        }

        internal virtual bool ImplementsDynamicParameters
        {
            get
            {
                return false;
            }
        }

        internal bool IsGetCommandCopy { get; set; }

        internal bool IsImported
        {
            get
            {
                return this._isImported;
            }
            set
            {
                this._isImported = value;
            }
        }

        public PSModuleInfo Module
        {
            get
            {
                return this._module;
            }
        }

        public string ModuleName
        {
            get
            {
                string name = null;
                if ((this._module != null) && !string.IsNullOrEmpty(this._module.Name))
                {
                    name = this._module.Name;
                }
                else
                {
                    CmdletInfo info = this as CmdletInfo;
                    if ((info != null) && (info.PSSnapIn != null))
                    {
                        name = info.PSSnapInName;
                    }
                }
                if (name == null)
                {
                    return string.Empty;
                }
                return name;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public abstract ReadOnlyCollection<PSTypeName> OutputType { get; }

        public virtual Dictionary<string, ParameterMetadata> Parameters
        {
            get
            {
                Dictionary<string, ParameterMetadata> dictionary = new Dictionary<string, ParameterMetadata>(StringComparer.OrdinalIgnoreCase);
                if (!this.ImplementsDynamicParameters || (this.Context == null))
                {
                    return this.ExternalCommandMetadata.Parameters;
                }
                foreach (KeyValuePair<string, MergedCompiledCommandParameter> pair in this.GetMergedCommandParameterMetdata().BindableParameters)
                {
                    dictionary.Add(pair.Key, new ParameterMetadata(pair.Value.Parameter));
                }
                return dictionary;
            }
        }

        public ReadOnlyCollection<CommandParameterSetInfo> ParameterSets
        {
            get
            {
                if (this._parameterSets == null)
                {
                    Collection<CommandParameterSetInfo> list = this.GenerateCommandParameterSetInfo();
                    this._parameterSets = new ReadOnlyCollection<CommandParameterSetInfo>(list);
                }
                return this._parameterSets;
            }
        }

        internal string Prefix
        {
            get
            {
                return this._prefix;
            }
            set
            {
                this._prefix = value;
            }
        }

        public System.Management.Automation.RemotingCapability RemotingCapability
        {
            get
            {
                try
                {
                    return this.ExternalCommandMetadata.RemotingCapability;
                }
                catch (PSNotSupportedException)
                {
                    return System.Management.Automation.RemotingCapability.PowerShell;
                }
            }
        }

        internal virtual string Syntax
        {
            get
            {
                return this.Definition;
            }
        }

        public virtual SessionStateEntryVisibility Visibility
        {
            get
            {
                if (this._copiedCommand != null)
                {
                    return this._copiedCommand.Visibility;
                }
                return this._visibility;
            }
            set
            {
                if (this._copiedCommand == null)
                {
                    this._visibility = value;
                }
                else
                {
                    this._copiedCommand.Visibility = value;
                }
                if ((value == SessionStateEntryVisibility.Private) && (this._module != null))
                {
                    this._module.ModuleHasPrivateMembers = true;
                }
            }
        }
    }
}

