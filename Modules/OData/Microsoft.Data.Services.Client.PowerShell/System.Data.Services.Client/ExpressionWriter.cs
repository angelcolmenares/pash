namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    internal class ExpressionWriter : DataServiceALinqExpressionVisitor
    {
        private readonly StringBuilder builder;
        private bool cantTranslateExpression;
        private readonly DataServiceContext context;
        private readonly Stack<Expression> expressionStack;
        private Expression parent;
        private int scopeCount;
        private Version uriVersion;

        private ExpressionWriter(DataServiceContext context)
        {
            this.context = context;
            this.builder = new StringBuilder();
            this.expressionStack = new Stack<Expression>();
            this.expressionStack.Push(null);
            this.uriVersion = Util.DataServiceVersion1;
            this.scopeCount = 0;
        }

        private static bool AreExpressionTypesCollapsible(ExpressionType type, ExpressionType parentType, ChildDirection childDirection)
        {
            int num = BinaryPrecedence(type);
            int num2 = BinaryPrecedence(parentType);
            if ((num >= 0) && (num2 >= 0))
            {
                if (childDirection == ChildDirection.Left)
                {
                    if (num <= num2)
                    {
                        return true;
                    }
                }
                else if (num < num2)
                {
                    return true;
                }
            }
            return false;
        }

        private static int BinaryPrecedence(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return 1;

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return 3;

                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return 0;

                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    return 2;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return 4;
            }
            return -1;
        }

        internal static string ExpressionToString(DataServiceContext context, Expression e, ref Version uriVersion)
        {
            ExpressionWriter writer = new ExpressionWriter(context);
            string str = writer.Translate(e);
            WebUtil.RaiseVersion(ref uriVersion, writer.uriVersion);
            if (writer.cantTranslateExpression)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CantTranslateExpression(e.ToString()));
            }
            return str;
        }

        private bool IsImplicitInputReference(Expression exp)
        {
            if (this.InSubScope)
            {
                return false;
            }
            return ((exp is InputReferenceExpression) || (exp is ParameterExpression));
        }

        private string Translate(Expression e)
        {
            this.Visit(e);
            return this.builder.ToString();
        }

        internal override Expression Visit(Expression exp)
        {
            this.parent = this.expressionStack.Peek();
            this.expressionStack.Push(exp);
            Expression expression = base.Visit(exp);
            this.expressionStack.Pop();
            return expression;
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            this.VisitOperand(b.Left, new ExpressionType?(b.NodeType), 0);
            this.builder.Append(' ');
            switch (b.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    this.builder.Append("add");
                    break;

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    this.builder.Append("and");
                    break;

                case ExpressionType.Divide:
                    this.builder.Append("div");
                    break;

                case ExpressionType.Equal:
                    this.builder.Append("eq");
                    break;

                case ExpressionType.GreaterThan:
                    this.builder.Append("gt");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    this.builder.Append("ge");
                    break;

                case ExpressionType.LessThan:
                    this.builder.Append("lt");
                    break;

                case ExpressionType.LessThanOrEqual:
                    this.builder.Append("le");
                    break;

                case ExpressionType.Modulo:
                    this.builder.Append("mod");
                    break;

                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    this.builder.Append("mul");
                    break;

                case ExpressionType.NotEqual:
                    this.builder.Append("ne");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    this.builder.Append("or");
                    break;

                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    this.builder.Append("sub");
                    break;

                default:
                    this.cantTranslateExpression = true;
                    break;
            }
            this.builder.Append(' ');
            this.VisitOperand(b.Right, new ExpressionType?(b.NodeType), (ChildDirection)1);
            return b;
        }

        internal override Expression VisitConditional(ConditionalExpression c)
        {
            this.cantTranslateExpression = true;
            return c;
        }

        internal override Expression VisitConstant(ConstantExpression c)
        {
            string result = null;
            if (c.Value == null)
            {
                this.builder.Append("null");
                return c;
            }
            if (!ClientConvert.TryKeyPrimitiveToString(c.Value, out result))
            {
                if (!this.cantTranslateExpression)
                {
                    throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CouldNotConvert(c.Value));
                }
                return c;
            }
            this.builder.Append(result);
            return c;
        }

        internal override Expression VisitInputReferenceExpression(InputReferenceExpression ire)
        {
            if ((this.parent == null) || ((!this.InSubScope && (this.parent.NodeType != ExpressionType.MemberAccess)) && (this.parent.NodeType != ExpressionType.TypeAs)))
            {
                string str = (this.parent != null) ? this.parent.ToString() : ire.ToString();
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CantTranslateExpression(str));
            }
            if (this.InSubScope)
            {
                this.builder.Append("$it");
            }
            return ire;
        }

        internal override Expression VisitInvocation(InvocationExpression iv)
        {
            this.cantTranslateExpression = true;
            return iv;
        }

        internal override Expression VisitLambda(LambdaExpression lambda)
        {
            this.cantTranslateExpression = true;
            return lambda;
        }

        internal override Expression VisitListInit(ListInitExpression init)
        {
            this.cantTranslateExpression = true;
            return init;
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Member is FieldInfo)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CantReferToPublicField(m.Member.Name));
            }
            Expression exp = this.Visit(m.Expression);
            if (((m.Member.Name != "Value") || !m.Member.DeclaringType.IsGenericType()) || (m.Member.DeclaringType.GetGenericTypeDefinition() != typeof(Nullable<>)))
            {
                if (!this.IsImplicitInputReference(exp))
                {
                    this.builder.Append('/');
                }
                this.builder.Append(m.Member.Name);
            }
            return m;
        }

        internal override Expression VisitMemberInit(MemberInitExpression init)
        {
            this.cantTranslateExpression = true;
            return init;
        }

        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            string str;
            SequenceMethod method;
            if (TypeSystem.TryGetQueryOptionMethod(m.Method, out str))
            {
                this.builder.Append(str);
                this.builder.Append('(');
                if (str == "substringof")
                {
                    this.Visit(m.Arguments[0]);
                    this.builder.Append(',');
                    this.Visit(m.Object);
                }
                else
                {
                    if (m.Object != null)
                    {
                        this.Visit(m.Object);
                    }
                    if (m.Arguments.Count > 0)
                    {
                        if (m.Object != null)
                        {
                            this.builder.Append(',');
                        }
                        for (int i = 0; i < m.Arguments.Count; i++)
                        {
                            this.Visit(m.Arguments[i]);
                            if (i < (m.Arguments.Count - 1))
                            {
                                this.builder.Append(',');
                            }
                        }
                    }
                }
                this.builder.Append(')');
                return m;
            }
            if (ReflectionUtil.TryIdentifySequenceMethod(m.Method, out method))
            {
                if (ReflectionUtil.IsAnyAllMethod(method))
                {
                    WebUtil.RaiseVersion(ref this.uriVersion, Util.DataServiceVersion3);
                    this.Visit(m.Arguments[0]);
                    this.builder.Append('/');
                    if (method == SequenceMethod.All)
                    {
                        this.builder.Append("all");
                    }
                    else
                    {
                        this.builder.Append("any");
                    }
                    this.builder.Append('(');
                    if (method != SequenceMethod.Any)
                    {
                        LambdaExpression expression = (LambdaExpression) m.Arguments[1];
                        string name = expression.Parameters[0].Name;
                        this.builder.Append(name);
                        this.builder.Append(':');
                        this.scopeCount++;
                        this.Visit(expression.Body);
                        this.scopeCount--;
                    }
                    this.builder.Append(')');
                    return m;
                }
                if ((method == SequenceMethod.OfType) && (this.parent != null))
                {
                    MethodCallExpression parent = this.parent as MethodCallExpression;
                    if (((parent != null) && ReflectionUtil.TryIdentifySequenceMethod(parent.Method, out method)) && ReflectionUtil.IsAnyAllMethod(method))
                    {
                        Type type = parent.Method.GetGenericArguments().SingleOrDefault<Type>();
                        if (ClientTypeUtil.TypeOrElementTypeIsEntity(type))
                        {
                            this.Visit(m.Arguments[0]);
                            this.builder.Append('/');
                            this.builder.Append(System.Data.Services.Client.UriHelper.GetEntityTypeNameForUriAndValidateMaxProtocolVersion(type, this.context, ref this.uriVersion));
                            return m;
                        }
                    }
                }
            }
            this.cantTranslateExpression = true;
            return m;
        }

        internal override NewExpression VisitNew(NewExpression nex)
        {
            this.cantTranslateExpression = true;
            return nex;
        }

        internal override Expression VisitNewArray(NewArrayExpression na)
        {
            this.cantTranslateExpression = true;
            return na;
        }

        private void VisitOperand(Expression e)
        {
            this.VisitOperand(e, null, null);
        }

        private void VisitOperand(Expression e, ExpressionType? parentType, ChildDirection? childDirection)
        {
            if (e is BinaryExpression)
            {
                bool flag = !parentType.HasValue || !AreExpressionTypesCollapsible(e.NodeType, parentType.Value, childDirection.Value);
                if (flag)
                {
                    this.builder.Append('(');
                }
                this.Visit(e);
                if (flag)
                {
                    this.builder.Append(')');
                }
            }
            else
            {
                this.Visit(e);
            }
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            if (this.InSubScope)
            {
                this.builder.Append(p.Name);
            }
            return p;
        }

        internal override Expression VisitTypeIs(TypeBinaryExpression b)
        {
            this.builder.Append("isof");
            this.builder.Append('(');
            if (!this.IsImplicitInputReference(b.Expression))
            {
                this.Visit(b.Expression);
                this.builder.Append(',');
                this.builder.Append(' ');
            }
            this.builder.Append('\'');
            this.builder.Append(System.Data.Services.Client.UriHelper.GetTypeNameForUri(b.TypeOperand, this.context));
            this.builder.Append('\'');
            this.builder.Append(')');
            return b;
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    if (!(u.Type != typeof(object)))
                    {
                        if (!this.IsImplicitInputReference(u.Operand))
                        {
                            this.Visit(u.Operand);
                        }
                        return u;
                    }
                    this.builder.Append("cast");
                    this.builder.Append('(');
                    if (!this.IsImplicitInputReference(u.Operand))
                    {
                        this.Visit(u.Operand);
                        this.builder.Append(',');
                    }
                    this.builder.Append('\'');
                    this.builder.Append(System.Data.Services.Client.UriHelper.GetTypeNameForUri(u.Type, this.context));
                    this.builder.Append('\'');
                    this.builder.Append(')');
                    return u;

                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    this.builder.Append(' ');
                    this.builder.Append("-");
                    this.VisitOperand(u.Operand);
                    return u;

                case ExpressionType.UnaryPlus:
                    return u;

                case ExpressionType.Not:
                    this.builder.Append("not");
                    this.builder.Append(' ');
                    this.VisitOperand(u.Operand);
                    return u;

                case ExpressionType.TypeAs:
                    if (u.Operand.NodeType == ExpressionType.TypeAs)
                    {
                        throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CannotUseTypeFiltersMultipleTimes);
                    }
                    this.Visit(u.Operand);
                    if (!this.IsImplicitInputReference(u.Operand))
                    {
                        this.builder.Append('/');
                    }
                    this.builder.Append(System.Data.Services.Client.UriHelper.GetEntityTypeNameForUriAndValidateMaxProtocolVersion(u.Type, this.context, ref this.uriVersion));
                    return u;
            }
            this.cantTranslateExpression = true;
            return u;
        }

        private bool InSubScope
        {
            get
            {
                return (this.scopeCount > 0);
            }
        }

        private enum ChildDirection
        {
            Left,
            Right
        }
    }
}

