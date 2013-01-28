namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class LoopCompiler : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, LocalVariable> _closureVariables;
        private readonly ParameterExpression _frameClosureVar;
        private readonly ParameterExpression _frameDataVar;
        private readonly ParameterExpression _frameVar;
        private readonly HybridReferenceDictionary<LabelTarget, BranchLabel> _labelMapping;
        private readonly PowerShellLoopExpression _loop;
        private readonly int _loopEndInstructionIndex;
        private HashSet<ParameterExpression> _loopLocals;
        private readonly int _loopStartInstructionIndex;
        private readonly Dictionary<ParameterExpression, LoopVariable> _loopVariables;
        private readonly Dictionary<ParameterExpression, LocalVariable> _outerVariables;
        private readonly LabelTarget _returnLabel;
        private ReadOnlyCollectionBuilder<ParameterExpression> _temps;

        internal LoopCompiler(PowerShellLoopExpression loop, HybridReferenceDictionary<LabelTarget, BranchLabel> labelMapping, Dictionary<ParameterExpression, LocalVariable> locals, Dictionary<ParameterExpression, LocalVariable> closureVariables, int loopStartInstructionIndex, int loopEndInstructionIndex)
        {
            this._loop = loop;
            this._outerVariables = locals;
            this._closureVariables = closureVariables;
            this._frameDataVar = Expression.Parameter(typeof(object[]));
            this._frameClosureVar = Expression.Parameter(typeof(StrongBox<object>[]));
            this._frameVar = Expression.Parameter(typeof(InterpretedFrame));
            this._loopVariables = new Dictionary<ParameterExpression, LoopVariable>();
            this._returnLabel = Expression.Label(typeof(int));
            this._labelMapping = labelMapping;
            this._loopStartInstructionIndex = loopStartInstructionIndex;
            this._loopEndInstructionIndex = loopEndInstructionIndex;
        }

        private ParameterExpression AddTemp(ParameterExpression variable)
        {
            if (this._temps == null)
            {
                this._temps = new ReadOnlyCollectionBuilder<ParameterExpression>();
            }
            this._temps.Add(variable);
            return variable;
        }

        internal Func<object[], StrongBox<object>[], InterpretedFrame, int> CreateDelegate()
        {
            Expression body = this.Visit(this._loop);
            ReadOnlyCollectionBuilder<Expression> expressions = new ReadOnlyCollectionBuilder<Expression>();
            ReadOnlyCollectionBuilder<Expression> builder2 = new ReadOnlyCollectionBuilder<Expression>();
            foreach (KeyValuePair<ParameterExpression, LoopVariable> pair in this._loopVariables)
            {
                LocalVariable variable;
                if (!this._outerVariables.TryGetValue(pair.Key, out variable))
                {
                    variable = this._closureVariables[pair.Key];
                }
                Expression right = variable.LoadFromArray(this._frameDataVar, this._frameClosureVar);
                if (variable.InClosureOrBoxed)
                {
                    ParameterExpression boxStorage = pair.Value.BoxStorage;
                    expressions.Add(Expression.Assign(boxStorage, right));
                    this.AddTemp(boxStorage);
                }
                else
                {
                    expressions.Add(Expression.Assign(pair.Key, Utils.Convert(right, pair.Key.Type)));
                    if ((pair.Value.Access & ExpressionAccess.Write) != ExpressionAccess.None)
                    {
                        builder2.Add(Expression.Assign(right, Utils.Box(pair.Key)));
                    }
                    this.AddTemp(pair.Key);
                }
            }
            if (builder2.Count > 0)
            {
                expressions.Add(Expression.TryFinally(body, Expression.Block(builder2)));
            }
            else
            {
                expressions.Add(body);
            }
            expressions.Add(Expression.Label(this._returnLabel, Expression.Constant(this._loopEndInstructionIndex - this._loopStartInstructionIndex)));
            return Expression.Lambda<Func<object[], StrongBox<object>[], InterpretedFrame, int>>((this._temps != null) ? Expression.Block((IEnumerable<ParameterExpression>) this._temps.ToReadOnlyCollection(), (IEnumerable<Expression>) expressions) : Expression.Block(expressions), new ParameterExpression[] { this._frameDataVar, this._frameClosureVar, this._frameVar }).Compile();
        }

        private HashSet<ParameterExpression> EnterVariableScope(ICollection<ParameterExpression> variables)
        {
            if (this._loopLocals == null)
            {
                this._loopLocals = new HashSet<ParameterExpression>(variables);
                return null;
            }
            HashSet<ParameterExpression> set = new HashSet<ParameterExpression>(this._loopLocals);
            this._loopLocals.UnionWith(variables);
            return set;
        }

        private void ExitVariableScope(HashSet<ParameterExpression> prevLocals)
        {
            this._loopLocals = prevLocals;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression expression4;
            if (node.CanReduce)
            {
                return this.Visit(node.Reduce());
            }
            ParameterExpression left = node.Left as ParameterExpression;
            if ((left == null) || (node.NodeType != ExpressionType.Assign))
            {
                return base.VisitBinary(node);
            }
            Expression expression2 = this.VisitVariable(left, ExpressionAccess.Write);
            Expression right = this.Visit(node.Right);
            if (!(expression2.Type != left.Type))
            {
                return node.Update(expression2, null, right);
            }
            if (right.NodeType != ExpressionType.Parameter)
            {
                expression4 = this.AddTemp(Expression.Parameter(right.Type));
                right = Expression.Assign(expression4, right);
            }
            else
            {
                expression4 = right;
            }
            return Expression.Block(node.Update(expression2, null, Expression.Convert(right, expression2.Type)), expression4);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            ReadOnlyCollection<ParameterExpression> variables = node.Variables;
            HashSet<ParameterExpression> prevLocals = this.EnterVariableScope(variables);
            Expression expression = base.VisitBlock(node);
            this.ExitVariableScope(prevLocals);
            return expression;
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            if (node.Variable != null)
            {
                HashSet<ParameterExpression> prevLocals = this.EnterVariableScope(new ParameterExpression[] { node.Variable });
                CatchBlock block = base.VisitCatchBlock(node);
                this.ExitVariableScope(prevLocals);
                return block;
            }
            return base.VisitCatchBlock(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node.CanReduce)
            {
                return this.Visit(node.Reduce());
            }
            return base.VisitExtension(node);
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            BranchLabel label;
            LabelTarget key = node.Target;
            Expression expression = this.Visit(node.Value);
            if (!this._labelMapping.TryGetValue(key, out label))
            {
                return node.Update(key, expression);
            }
            if ((label.TargetIndex >= this._loopStartInstructionIndex) && (label.TargetIndex < this._loopEndInstructionIndex))
            {
                return node.Update(key, expression);
            }
            return Expression.Return(this._returnLabel, ((expression != null) && (expression.Type != typeof(void))) ? Expression.Call(this._frameVar, InterpretedFrame.GotoMethod, Expression.Constant(label.LabelIndex), Utils.Box(expression)) : Expression.Call(this._frameVar, InterpretedFrame.VoidGotoMethod, new Expression[] { Expression.Constant(label.LabelIndex) }), node.Type);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Expression expression;
            HashSet<ParameterExpression> prevLocals = this.EnterVariableScope(node.Parameters);
            try
            {
                expression = base.VisitLambda<T>(node);
            }
            finally
            {
                this.ExitVariableScope(prevLocals);
            }
            return expression;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return this.VisitVariable(node, ExpressionAccess.Read);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.CanReduce)
            {
                return this.Visit(node.Reduce());
            }
            return base.VisitUnary(node);
        }

        private Expression VisitVariable(ParameterExpression node, ExpressionAccess access)
        {
            ParameterExpression boxStorage;
            if (!this._loopLocals.Contains(node))
            {
                LoopVariable variable;
                LocalVariable variable2;
                if (this._loopVariables.TryGetValue(node, out variable))
                {
                    boxStorage = variable.BoxStorage;
                    this._loopVariables[node] = new LoopVariable(variable.Access | access, boxStorage);
                    goto Label_00A5;
                }
                if (this._outerVariables.TryGetValue(node, out variable2) || ((this._closureVariables != null) && this._closureVariables.TryGetValue(node, out variable2)))
                {
                    boxStorage = variable2.InClosureOrBoxed ? Expression.Parameter(typeof(StrongBox<object>), node.Name) : null;
                    this._loopVariables[node] = new LoopVariable(access, boxStorage);
                    goto Label_00A5;
                }
            }
            return node;
        Label_00A5:
            if (boxStorage == null)
            {
                return node;
            }
            if ((access & ExpressionAccess.Write) != ExpressionAccess.None)
            {
                return LightCompiler.Unbox(boxStorage);
            }
            return Expression.Convert(LightCompiler.Unbox(boxStorage), node.Type);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LoopVariable
        {
            public ExpressionAccess Access;
            public ParameterExpression BoxStorage;
            public LoopVariable(ExpressionAccess access, ParameterExpression box)
            {
                this.Access = access;
                this.BoxStorage = box;
            }

            public override string ToString()
            {
                return (this.Access.ToString() + " " + this.BoxStorage);
            }
        }
    }
}

