namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Management.Automation.Interpreter;

    internal class PowerShellLoopExpression : Expression, IInstructionProvider
    {
        private readonly IEnumerable<Expression> _exprs;

        internal PowerShellLoopExpression(IEnumerable<Expression> exprs)
        {
            this._exprs = exprs;
        }

        public void AddInstructions(LightCompiler compiler)
        {
            EnterLoopInstruction enterLoopInstruction = null;
            compiler.PushLabelBlock(LabelScopeKind.Statement);
            foreach (Expression expression in this._exprs)
            {
                compiler.CompileAsVoid(expression);
                EnterLoopExpression expression2 = expression as EnterLoopExpression;
                if (expression2 != null)
                {
                    enterLoopInstruction = expression2.EnterLoopInstruction;
                }
            }
            compiler.PopLabelBlock(LabelScopeKind.Statement);
            if (enterLoopInstruction != null)
            {
                enterLoopInstruction.FinishLoop(compiler.Instructions.Count);
            }
        }

        public override Expression Reduce()
        {
            return Expression.Block(this._exprs);
        }

        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        public override System.Type Type
        {
            get
            {
                return typeof(void);
            }
        }
    }
}

