namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;

    public class ScriptInfo : CommandInfo, IScriptCommandInfo
    {
        private System.Management.Automation.CommandMetadata _commandMetadata;

        internal ScriptInfo(ScriptInfo other) : base(other)
        {
            this.ScriptBlock = other.ScriptBlock;
        }

        internal ScriptInfo(string name, System.Management.Automation.ScriptBlock script, ExecutionContext context) : base(name, CommandTypes.Script, context)
        {
            if (script == null)
            {
                throw PSTraceSource.NewArgumentException("script");
            }
            this.ScriptBlock = script;
        }

        internal override CommandInfo CreateGetCommandCopy(object[] argumentList)
        {
            return new ScriptInfo(this) { IsGetCommandCopy = true, Arguments = argumentList };
        }

        public override string ToString()
        {
            return this.ScriptBlock.ToString();
        }

        internal override System.Management.Automation.CommandMetadata CommandMetadata
        {
            get
            {
                return (this._commandMetadata ?? (this._commandMetadata = new System.Management.Automation.CommandMetadata(this.ScriptBlock, base.Name, LocalPipeline.GetExecutionContextFromTLS())));
            }
        }

        public override string Definition
        {
            get
            {
                return this.ScriptBlock.ToString();
            }
        }

        internal override System.Management.Automation.HelpCategory HelpCategory
        {
            get
            {
                return System.Management.Automation.HelpCategory.ScriptCommand;
            }
        }

        internal override bool ImplementsDynamicParameters
        {
            get
            {
                return this.ScriptBlock.HasDynamicParameters;
            }
        }

        public override ReadOnlyCollection<PSTypeName> OutputType
        {
            get
            {
                return this.ScriptBlock.OutputType;
            }
        }

        public System.Management.Automation.ScriptBlock ScriptBlock { get; private set; }
    }
}

