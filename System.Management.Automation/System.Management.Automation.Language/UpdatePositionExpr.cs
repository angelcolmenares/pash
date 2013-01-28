namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Management.Automation.Interpreter;

    internal class UpdatePositionExpr : Expression, IInstructionProvider
    {
        private readonly bool _checkBreakpoints;
        private readonly SymbolDocumentInfo _debugSymbolDocument;
        private readonly IScriptExtent _extent;
        private readonly int _sequencePoint;

        public UpdatePositionExpr(IScriptExtent extent, int sequencePoint, SymbolDocumentInfo debugSymbolDocument, bool checkBreakpoints)
        {
            this._extent = extent;
            this._checkBreakpoints = checkBreakpoints;
            this._debugSymbolDocument = debugSymbolDocument;
            this._sequencePoint = sequencePoint;
        }

        public void AddInstructions(LightCompiler compiler)
        {
            compiler.Instructions.Emit(UpdatePositionInstruction.Create(this._sequencePoint, this._checkBreakpoints));
        }

        public override Expression Reduce()
        {
            List<Expression> expressions = new List<Expression>();
            if (this._debugSymbolDocument != null)
            {
                expressions.Add(Expression.DebugInfo(this._debugSymbolDocument, this._extent.StartLineNumber, this._extent.StartColumnNumber, this._extent.EndLineNumber, this._extent.EndColumnNumber));
            }
            expressions.Add(Expression.Assign(Expression.Field(Compiler._functionContext, CachedReflectionInfo.FunctionContext__currentSequencePointIndex), ExpressionCache.Constant(this._sequencePoint)));
            if (this._checkBreakpoints)
            {
                expressions.Add(Expression.IfThen(Expression.GreaterThan(Expression.Field(Compiler._executionContextParameter, CachedReflectionInfo.ExecutionContext_DebuggingMode), ExpressionCache.Constant(0)), Expression.Call(Expression.Field(Compiler._executionContextParameter, CachedReflectionInfo.ExecutionContext_Debugger), CachedReflectionInfo.Debugger_OnSequencePointHit, new Expression[] { Compiler._functionContext })));
            }
            expressions.Add(ExpressionCache.Empty);
            return Expression.Block(expressions);
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

