namespace System.Management.Automation.Language
{
    using System;

    public abstract class PipelineBaseAst : StatementAst
    {
        protected PipelineBaseAst(IScriptExtent extent) : base(extent)
        {
        }

        public virtual ExpressionAst GetPureExpression()
        {
            return null;
        }
    }
}

