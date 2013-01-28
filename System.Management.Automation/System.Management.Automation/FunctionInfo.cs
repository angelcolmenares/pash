namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Runspaces;
    using System.Text;
    using System.Threading;

    public class FunctionInfo : CommandInfo, IScriptCommandInfo
    {
        private System.Management.Automation.CommandMetadata _commandMetadata;
        private string _description;
        private string _helpFile;
        private ScopedItemOptions _options;
        private System.Management.Automation.ScriptBlock _scriptBlock;
        private string noun;
        private string verb;

        internal FunctionInfo(FunctionInfo other) : base(other)
        {
            this.verb = string.Empty;
            this.noun = string.Empty;
            this._helpFile = string.Empty;
            this.CopyFieldsFromOther(other);
        }

        internal FunctionInfo(string name, FunctionInfo other) : base(name, other)
        {
            this.verb = string.Empty;
            this.noun = string.Empty;
            this._helpFile = string.Empty;
            this.CopyFieldsFromOther(other);
            CmdletInfo.SplitCmdletName(name, out this.verb, out this.noun);
        }

        internal FunctionInfo(string name, System.Management.Automation.ScriptBlock function, System.Management.Automation.ExecutionContext context) : this(name, function, context, null)
        {
        }

        internal FunctionInfo(string name, System.Management.Automation.ScriptBlock function, System.Management.Automation.ExecutionContext context, string helpFile) : base(name, CommandTypes.Function, context)
        {
            this.verb = string.Empty;
            this.noun = string.Empty;
            this._helpFile = string.Empty;
            if (function == null)
            {
                throw PSTraceSource.NewArgumentNullException("function");
            }
            this._scriptBlock = function;
            CmdletInfo.SplitCmdletName(name, out this.verb, out this.noun);
            base.SetModule(function.Module);
            this._helpFile = helpFile;
        }

        internal FunctionInfo(string name, System.Management.Automation.ScriptBlock function, ScopedItemOptions options, System.Management.Automation.ExecutionContext context) : this(name, function, options, context, null)
        {
        }

        internal FunctionInfo(string name, System.Management.Automation.ScriptBlock function, ScopedItemOptions options, System.Management.Automation.ExecutionContext context, string helpFile) : this(name, function, context, helpFile)
        {
            this._options = options;
        }

        private void CopyFieldsFromOther(FunctionInfo other)
        {
            this._scriptBlock = other._scriptBlock;
            this._description = other._description;
            this._options = other._options;
            this._helpFile = other._helpFile;
        }

        internal override CommandInfo CreateGetCommandCopy(object[] arguments)
        {
            return new FunctionInfo(this) { IsGetCommandCopy = true, Arguments = arguments };
        }

        internal void Update(System.Management.Automation.ScriptBlock newFunction, bool force, ScopedItemOptions options)
        {
            this.Update(newFunction, force, options, null);
        }

        protected internal virtual void Update(FunctionInfo newFunction, bool force, ScopedItemOptions options, string helpFile)
        {
            this.Update(newFunction.ScriptBlock, force, options, helpFile);
        }

        internal void Update(System.Management.Automation.ScriptBlock newFunction, bool force, ScopedItemOptions options, string helpFile)
        {
            if (newFunction == null)
            {
                throw PSTraceSource.NewArgumentNullException("function");
            }
            if ((this._options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
            {
                SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Function, "FunctionIsConstant", SessionStateStrings.FunctionIsConstant);
                throw exception;
            }
            if (!force && ((this._options & ScopedItemOptions.ReadOnly) != ScopedItemOptions.None))
            {
                SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Function, "FunctionIsReadOnly", SessionStateStrings.FunctionIsReadOnly);
                throw exception2;
            }
            this._scriptBlock = newFunction;
            base.SetModule(newFunction.Module);
            this._commandMetadata = null;
            base._parameterSets = null;
            base._externalCommandMetadata = null;
            if (options != ScopedItemOptions.Unspecified)
            {
                this.Options = options;
            }
            this._helpFile = helpFile;
        }

        public bool CmdletBinding
        {
            get
            {
                return this.ScriptBlock.UsesCmdletBinding;
            }
        }

        internal override System.Management.Automation.CommandMetadata CommandMetadata
        {
            get
            {
                return (this._commandMetadata ?? (this._commandMetadata = new System.Management.Automation.CommandMetadata(this.ScriptBlock, base.Name, LocalPipeline.GetExecutionContextFromTLS())));
            }
        }

        public string DefaultParameterSet
        {
            get
            {
                if (!this.CmdletBinding)
                {
                    return null;
                }
                return this.CommandMetadata.DefaultParameterSetName;
            }
        }

        public override string Definition
        {
            get
            {
                return this._scriptBlock.ToString();
            }
        }

        public string Description
        {
            get
            {
                if (base.CopiedCommand != null)
                {
                    return ((FunctionInfo) base.CopiedCommand).Description;
                }
                return this._description;
            }
            set
            {
                if (base.CopiedCommand == null)
                {
                    this._description = value;
                }
                else
                {
                    ((FunctionInfo) base.CopiedCommand).Description = value;
                }
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.Function;
            }
        }

        public string HelpFile
        {
            get
            {
                return this._helpFile;
            }
        }

        internal override bool ImplementsDynamicParameters
        {
            get
            {
                return this.ScriptBlock.HasDynamicParameters;
            }
        }

        public string Noun
        {
            get
            {
                return this.noun;
            }
        }

        public ScopedItemOptions Options
        {
            get
            {
                if (base.CopiedCommand != null)
                {
                    return ((FunctionInfo) base.CopiedCommand).Options;
                }
                return this._options;
            }
            set
            {
                if (base.CopiedCommand == null)
                {
                    if ((this._options & ScopedItemOptions.Constant) != ScopedItemOptions.None)
                    {
                        SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Function, "FunctionIsConstant", SessionStateStrings.FunctionIsConstant);
                        throw exception;
                    }
                    if ((value & ScopedItemOptions.Constant) != ScopedItemOptions.None)
                    {
                        SessionStateUnauthorizedAccessException exception2 = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Function, "FunctionCannotBeMadeConstant", SessionStateStrings.FunctionCannotBeMadeConstant);
                        throw exception2;
                    }
                    if (((value & ScopedItemOptions.AllScope) == ScopedItemOptions.None) && ((this._options & ScopedItemOptions.AllScope) != ScopedItemOptions.None))
                    {
                        SessionStateUnauthorizedAccessException exception3 = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Function, "FunctionAllScopeOptionCannotBeRemoved", SessionStateStrings.FunctionAllScopeOptionCannotBeRemoved);
                        throw exception3;
                    }
                    this._options = value;
                }
                else
                {
                    ((FunctionInfo) base.CopiedCommand).Options = value;
                }
            }
        }

        public override ReadOnlyCollection<PSTypeName> OutputType
        {
            get
            {
                return this.ScriptBlock.OutputType;
            }
        }

        public System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                return this._scriptBlock;
            }
        }

        internal override string Syntax
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (CommandParameterSetInfo info in base.ParameterSets)
                {
                    builder.AppendLine();
                    builder.AppendLine(string.Format(Thread.CurrentThread.CurrentCulture, "{0} {1}", new object[] { base.Name, info.ToString((base.CommandType & CommandTypes.Workflow) == CommandTypes.Workflow) }));
                }
                return builder.ToString();
            }
        }

        public string Verb
        {
            get
            {
                return this.verb;
            }
        }
    }
}

