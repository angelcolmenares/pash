namespace System.Management.Automation.Language
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class LabeledStatementAst : StatementAst
    {
        protected LabeledStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition) : base(extent)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                label = null;
            }
            this.Label = label;
            if (condition != null)
            {
                this.Condition = condition;
                base.SetParent(condition);
            }
        }

        public PipelineBaseAst Condition { get; private set; }

        public string Label { get; private set; }
    }
}

