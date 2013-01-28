namespace System.Management.Automation.Language
{
    using System;
    using System.Linq.Expressions;
    using System.Management.Automation.Interpreter;
    using System.Runtime.CompilerServices;

    internal class EnterLoopExpression : Expression, IInstructionProvider
    {
        public void AddInstructions(LightCompiler compiler)
        {
            if (this.LoopStatementCount < 300)
            {
                this.EnterLoopInstruction = new System.Management.Automation.Interpreter.EnterLoopInstruction(this.Loop, compiler.Locals, 0x10, compiler.Instructions.Count);
                compiler.Instructions.Emit(this.EnterLoopInstruction);
            }
        }

        public override Expression Reduce()
        {
            return ExpressionCache.Empty;
        }

        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

        internal System.Management.Automation.Interpreter.EnterLoopInstruction EnterLoopInstruction { get; private set; }

        internal PowerShellLoopExpression Loop { get; set; }

        internal int LoopStatementCount { get; set; }

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

