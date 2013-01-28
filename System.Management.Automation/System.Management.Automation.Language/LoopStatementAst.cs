namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    public abstract class LoopStatementAst : LabeledStatementAst
    {
        protected LoopStatementAst(IScriptExtent extent, string label, PipelineBaseAst condition, StatementBlockAst body) : base(extent, label, condition)
        {
            if (body == null)
            {
                throw PSTraceSource.NewArgumentNullException("body");
            }
            this.Body = body;
            base.SetParent(body);
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            return this.Body.GetInferredType(context);
        }

        public StatementBlockAst Body { get; private set; }
    }
}

