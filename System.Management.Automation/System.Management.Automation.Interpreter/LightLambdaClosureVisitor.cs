namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    internal sealed class LightLambdaClosureVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _closureArray;
        private readonly Dictionary<ParameterExpression, LocalVariable> _closureVars;
        private readonly Stack<HashSet<ParameterExpression>> _shadowedVars = new Stack<HashSet<ParameterExpression>>();

        private LightLambdaClosureVisitor(Dictionary<ParameterExpression, LocalVariable> closureVariables, ParameterExpression closureArray)
        {
            this._closureArray = closureArray;
            this._closureVars = closureVariables;
        }

        internal static Func<StrongBox<object>[], Delegate> BindLambda(LambdaExpression lambda, Dictionary<ParameterExpression, LocalVariable> closureVariables)
        {
            ParameterExpression expression;
            LightLambdaClosureVisitor visitor = new LightLambdaClosureVisitor(closureVariables, expression = Expression.Parameter(typeof(StrongBox<object>[]), "closure"));
            lambda = (LambdaExpression) visitor.Visit(lambda);
            return Expression.Lambda<Func<StrongBox<object>[], Delegate>>(lambda, new ParameterExpression[] { expression }).Compile();
        }

        private Expression GetClosureItem(ParameterExpression variable, bool unbox)
        {
            LocalVariable variable2;
            foreach (HashSet<ParameterExpression> set in this._shadowedVars)
            {
                if (set.Contains(variable))
                {
                    return null;
                }
            }
            if (!this._closureVars.TryGetValue(variable, out variable2))
            {
                throw new InvalidOperationException("unbound variable: " + variable.Name);
            }
            Expression strongBoxExpression = variable2.LoadFromArray(null, this._closureArray);
            if (!unbox)
            {
                return strongBoxExpression;
            }
            return LightCompiler.Unbox(strongBoxExpression);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if ((node.NodeType == ExpressionType.Assign) && (node.Left.NodeType == ExpressionType.Parameter))
            {
                ParameterExpression left = (ParameterExpression) node.Left;
                Expression closureItem = this.GetClosureItem(left, true);
                if (closureItem != null)
                {
                    return Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Assign(left, this.Visit(node.Right)), Expression.Assign(closureItem, Utils.Convert(left, typeof(object))), left });
                }
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            if (node.Variables.Count > 0)
            {
                this._shadowedVars.Push(new HashSet<ParameterExpression>(node.Variables));
            }
            ReadOnlyCollection<Expression> onlys = base.Visit(node.Expressions);
            if (node.Variables.Count > 0)
            {
                this._shadowedVars.Pop();
            }
            if (onlys == node.Expressions)
            {
                return node;
            }
            return Expression.Block((IEnumerable<ParameterExpression>) node.Variables, (IEnumerable<Expression>) onlys);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            if (node.Variable != null)
            {
                this._shadowedVars.Push(new HashSet<ParameterExpression>(new ParameterExpression[] { node.Variable }));
            }
            Expression body = this.Visit(node.Body);
            Expression filter = this.Visit(node.Filter);
            if (node.Variable != null)
            {
                this._shadowedVars.Pop();
            }
            if ((body == node.Body) && (filter == node.Filter))
            {
                return node;
            }
            return Expression.MakeCatchBlock(node.Test, node.Variable, body, filter);
        }

        protected override Expression VisitExtension(Expression node)
        {
            return this.Visit(node.ReduceExtensions());
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            this._shadowedVars.Push(new HashSet<ParameterExpression>(node.Parameters));
            Expression body = this.Visit(node.Body);
            this._shadowedVars.Pop();
            if (body == node.Body)
            {
                return node;
            }
            return Expression.Lambda<T>(body, node.Name, node.TailCall, node.Parameters);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression closureItem = this.GetClosureItem(node, true);
            if (closureItem == null)
            {
                return node;
            }
            return Utils.Convert(closureItem, node.Type);
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            int count = node.Variables.Count;
            List<Expression> initializers = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();
            int[] numArray = new int[count];
            for (int i = 0; i < count; i++)
            {
                Expression closureItem = this.GetClosureItem(node.Variables[i], false);
                if (closureItem == null)
                {
                    numArray[i] = variables.Count;
                    variables.Add(node.Variables[i]);
                }
                else
                {
                    numArray[i] = -1 - initializers.Count;
                    initializers.Add(closureItem);
                }
            }
            if (initializers.Count == 0)
            {
                return node;
            }
            NewArrayExpression expression2 = Expression.NewArrayInit(typeof(IStrongBox), initializers);
            if (variables.Count == 0)
            {
                return Expression.Invoke(Expression.Constant(new Func<IStrongBox[], IRuntimeVariables>(System.Management.Automation.Interpreter.RuntimeVariables.Create)), new Expression[] { expression2 });
            }
            Func<IRuntimeVariables, IRuntimeVariables, int[], IRuntimeVariables> func = new Func<IRuntimeVariables, IRuntimeVariables, int[], IRuntimeVariables>(MergedRuntimeVariables.Create);
            return Expression.Invoke(Utils.Constant(func), new Expression[] { Expression.RuntimeVariables(variables), expression2, Utils.Constant(numArray) });
        }

        private sealed class MergedRuntimeVariables : IRuntimeVariables
        {
            private readonly IRuntimeVariables _first;
            private readonly int[] _indexes;
            private readonly IRuntimeVariables _second;

            private MergedRuntimeVariables(IRuntimeVariables first, IRuntimeVariables second, int[] indexes)
            {
                this._first = first;
                this._second = second;
                this._indexes = indexes;
            }

            internal static IRuntimeVariables Create(IRuntimeVariables first, IRuntimeVariables second, int[] indexes)
            {
                return new LightLambdaClosureVisitor.MergedRuntimeVariables(first, second, indexes);
            }

            int IRuntimeVariables.Count
            {
                get
                {
                    return this._indexes.Length;
                }
            }

            object IRuntimeVariables.this[int index]
            {
                get
                {
                    index = this._indexes[index];
                    if (index < 0)
                    {
                        return this._second[-1 - index];
                    }
                    return this._first[index];
                }
                set
                {
                    index = this._indexes[index];
                    if (index >= 0)
                    {
                        this._first[index] = value;
                    }
                    else
                    {
                        this._second[-1 - index] = value;
                    }
                }
            }
        }
    }
}

