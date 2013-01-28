namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class VariableExpressionAst : ExpressionAst, ISupportsAssignment, IAssignableValue
    {
        private int _tupleIndex;

        internal VariableExpressionAst(VariableToken token) : this(token.Extent, token.VariablePath, token.Kind == TokenKind.SplattedVariable)
        {
        }

        public VariableExpressionAst(IScriptExtent extent, System.Management.Automation.VariablePath variablePath, bool splatted) : base(extent)
        {
            this._tupleIndex = -1;
            if (variablePath == null)
            {
                throw PSTraceSource.NewArgumentNullException("variablePath");
            }
            this.VariablePath = variablePath;
            this.Splatted = splatted;
        }

        public VariableExpressionAst(IScriptExtent extent, string variableName, bool splatted) : base(extent)
        {
            this._tupleIndex = -1;
            if (string.IsNullOrEmpty(variableName))
            {
                throw PSTraceSource.NewArgumentNullException("variableName");
            }
            this.VariablePath = new System.Management.Automation.VariablePath(variableName);
            this.Splatted = splatted;
        }

        internal override object Accept(ICustomAstVisitor visitor)
        {
            return visitor.VisitVariableExpression(this);
        }

        private bool AstAssignsToSameVariable(Ast ast)
        {
            ParameterAst ast2 = ast as ParameterAst;
            if (ast2 != null)
            {
                return (this.VariablePath.IsUnscopedVariable && ast2.Name.VariablePath.UnqualifiedPath.Equals(this.VariablePath.UnqualifiedPath, StringComparison.OrdinalIgnoreCase));
            }
            ForEachStatementAst ast3 = ast as ForEachStatementAst;
            if (ast3 != null)
            {
                return (this.VariablePath.IsUnscopedVariable && ast3.Variable.VariablePath.UnqualifiedPath.Equals(this.VariablePath.UnqualifiedPath, StringComparison.OrdinalIgnoreCase));
            }
            AssignmentStatementAst ast4 = (AssignmentStatementAst) ast;
            ExpressionAst left = ast4.Left;
            ConvertExpressionAst ast6 = left as ConvertExpressionAst;
            if (ast6 != null)
            {
                left = ast6.Child;
            }
            VariableExpressionAst ast7 = left as VariableExpressionAst;
            if (ast7 == null)
            {
                return false;
            }
            System.Management.Automation.VariablePath variablePath = ast7.VariablePath;
            return (variablePath.UserPath.Equals(this.VariablePath.UserPath, StringComparison.OrdinalIgnoreCase) || (this.VariablePath.IsScript && this.VariablePath.UnqualifiedPath.Equals(variablePath.UnqualifiedPath, StringComparison.OrdinalIgnoreCase)));
        }

        internal override IEnumerable<PSTypeName> GetInferredType(CompletionContext context)
        {
            if (this.VariablePath.IsVariable)
            {
                Ast parent = this.Parent;
                if (this.VariablePath.IsUnqualified && (this.VariablePath.UserPath.Equals("_", StringComparison.Ordinal) || this.VariablePath.UserPath.Equals("PSItem", StringComparison.OrdinalIgnoreCase)))
                {
                    while (parent != null)
                    {
                        if (parent is ScriptBlockExpressionAst)
                        {
                            break;
                        }
                        parent = parent.Parent;
                    }
                    if (parent != null)
                    {
                        if ((parent.Parent is CommandExpressionAst) && (parent.Parent.Parent is PipelineAst))
                        {
                            if (parent.Parent.Parent.Parent is HashtableAst)
                            {
                                parent = parent.Parent.Parent.Parent;
                            }
                            else if ((parent.Parent.Parent.Parent is ArrayLiteralAst) && (parent.Parent.Parent.Parent.Parent is HashtableAst))
                            {
                                parent = parent.Parent.Parent.Parent.Parent;
                            }
                        }
                        if (parent.Parent is CommandParameterAst)
                        {
                            parent = parent.Parent;
                        }
                        CommandAst iteratorVariable1 = parent.Parent as CommandAst;
                        if (iteratorVariable1 != null)
                        {
                            PipelineAst iteratorVariable2 = (PipelineAst) iteratorVariable1.Parent;
                            int iteratorVariable3 = iteratorVariable2.PipelineElements.IndexOf(iteratorVariable1) - 1;
                            if (iteratorVariable3 >= 0)
                            {
                                foreach (PSTypeName iteratorVariable4 in iteratorVariable2.PipelineElements[0].GetInferredType(context))
                                {
                                    if (iteratorVariable4.Type != null)
                                    {
                                        if (iteratorVariable4.Type.IsArray)
                                        {
                                            yield return new PSTypeName(iteratorVariable4.Type.GetElementType());
                                            continue;
                                        }
                                        if (typeof(IEnumerable).IsAssignableFrom(iteratorVariable4.Type))
                                        {
                                            IEnumerable<Type> iteratorVariable5 = from t in iteratorVariable4.Type.GetInterfaces()
                                                where t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>))
                                                select t;
                                            foreach (Type iteratorVariable6 in iteratorVariable5)
                                            {
                                                yield return new PSTypeName(iteratorVariable6.GetGenericArguments()[0]);
                                            }
                                            continue;
                                        }
                                    }
                                    yield return iteratorVariable4;
                                }
                            }
                            goto Label_0833;
                        }
                    }
                }
                if (this.VariablePath.IsUnqualified)
                {
                    for (int i = 0; i < SpecialVariables.AutomaticVariables.Length; i++)
                    {
                        if (this.VariablePath.UserPath.Equals(SpecialVariables.AutomaticVariables[i], StringComparison.OrdinalIgnoreCase))
                        {
                            Type type = SpecialVariables.AutomaticVariableTypes[i];
                            if (!type.Equals(typeof(object)))
                            {
                                yield return new PSTypeName(type);
                                break;
                            }
                            break;
                        }
                    }
                }
                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }
                if (parent.Parent is FunctionDefinitionAst)
                {
                    parent = parent.Parent;
                }
                IEnumerable<Ast> source = AstSearcher.FindAll(parent, ast => (((ast is ParameterAst) || (ast is AssignmentStatementAst)) || (ast is ForEachStatementAst)) && this.AstAssignsToSameVariable(ast), true);
                ParameterAst iteratorVariable10 = source.OfType<ParameterAst>().FirstOrDefault<ParameterAst>();
                if (iteratorVariable10 != null)
                {
                    PSTypeName[] iteratorVariable11 = iteratorVariable10.GetInferredType(context).ToArray<PSTypeName>();
                    if (iteratorVariable11.Length > 0)
                    {
                        foreach (PSTypeName iteratorVariable12 in iteratorVariable11)
                        {
                            yield return iteratorVariable12;
                        }
                        goto Label_0833;
                    }
                }
                AssignmentStatementAst[] iteratorVariable13 = source.OfType<AssignmentStatementAst>().ToArray<AssignmentStatementAst>();
                foreach (AssignmentStatementAst iteratorVariable14 in iteratorVariable13)
                {
                    ConvertExpressionAst left = iteratorVariable14.Left as ConvertExpressionAst;
                    if ((left != null) && (left.StaticType != null))
                    {
                        yield return new PSTypeName(left.StaticType);
                        goto Label_0833;
                    }
                }
                ForEachStatementAst iteratorVariable16 = source.OfType<ForEachStatementAst>().FirstOrDefault<ForEachStatementAst>();
                if (iteratorVariable16 != null)
                {
                    foreach (PSTypeName iteratorVariable17 in iteratorVariable16.Condition.GetInferredType(context))
                    {
                        yield return iteratorVariable17;
                    }
                }
                else
                {
                    int startOffset = this.Extent.StartOffset;
                    int iteratorVariable19 = 0x7fffffff;
                    AssignmentStatementAst iteratorVariable20 = null;
                    foreach (AssignmentStatementAst ast in iteratorVariable13)
                    {
                        int endOffset = ast.Extent.EndOffset;
                        if ((endOffset < startOffset) && ((startOffset - endOffset) < iteratorVariable19))
                        {
                            iteratorVariable19 = startOffset - endOffset;
                            iteratorVariable20 = ast;
                        }
                    }
                    if (iteratorVariable20 != null)
                    {
                        foreach (PSTypeName iteratorVariable21 in iteratorVariable20.Right.GetInferredType(context))
                        {
                            yield return iteratorVariable21;
                        }
                    }
                }
            }
        Label_0833:
            yield break;
        }

        internal Type GetVariableType(Compiler compiler, out IEnumerable<PropertyInfo> tupleAccessPath, out bool localInTuple)
        {
            localInTuple = (this._tupleIndex >= 0) && (compiler.Optimize || (this._tupleIndex < 9));
            tupleAccessPath = null;
            if (localInTuple)
            {
                tupleAccessPath = MutableTuple.GetAccessPath(compiler.LocalVariablesTupleType, this._tupleIndex);
                return tupleAccessPath.Last<PropertyInfo>().PropertyType;
            }
            return typeof(object);
        }

        internal override AstVisitAction InternalVisit(AstVisitor visitor)
        {
            AstVisitAction action = visitor.VisitVariableExpression(this);
            if (action != AstVisitAction.SkipChildren)
            {
                return action;
            }
            return AstVisitAction.Continue;
        }

        public bool IsConstantVariable()
        {
            if (this.VariablePath.IsVariable)
            {
                string unqualifiedPath = this.VariablePath.UnqualifiedPath;
                if ((unqualifiedPath.Equals("true", StringComparison.OrdinalIgnoreCase) || unqualifiedPath.Equals("false", StringComparison.OrdinalIgnoreCase)) || unqualifiedPath.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        Expression IAssignableValue.GetValue(Compiler compiler, List<Expression> exprs, List<ParameterExpression> temps)
        {
            return (Expression) compiler.VisitVariableExpression(this);
        }

        Expression IAssignableValue.SetValue(Compiler compiler, Expression rhs)
        {
            IEnumerable<PropertyInfo> enumerable;
            bool flag;
            if (this.VariablePath.IsVariable && this.VariablePath.UnqualifiedPath.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return rhs;
            }
            Type type = this.GetVariableType(compiler, out enumerable, out flag);
            Type type2 = rhs.Type;
            if ((flag && (type.Equals(typeof(object)) || type.Equals(typeof(PSObject)))) && (type2.Equals(typeof(object)) || type2.Equals(typeof(PSObject))))
            {
                rhs = Expression.Dynamic(PSVariableAssignmentBinder.Get(), typeof(object), rhs);
            }
            rhs = rhs.Convert(type);
            if (!flag)
            {
                return Compiler.CallSetVariable(Expression.Constant(this.VariablePath), rhs, null);
            }
            Expression localVariablesParameter = compiler.LocalVariablesParameter;
            foreach (PropertyInfo info in enumerable)
            {
                localVariablesParameter = Expression.Property(localVariablesParameter, info);
            }
            return Expression.Assign(localVariablesParameter, rhs);
        }

        IAssignableValue ISupportsAssignment.GetAssignableValue()
        {
            return this;
        }

        internal bool Automatic { get; set; }

        public bool Splatted { get; private set; }

        internal int TupleIndex
        {
            get
            {
                return this._tupleIndex;
            }
            set
            {
                this._tupleIndex = value;
            }
        }

        public System.Management.Automation.VariablePath VariablePath { get; private set; }

        
    }
}

