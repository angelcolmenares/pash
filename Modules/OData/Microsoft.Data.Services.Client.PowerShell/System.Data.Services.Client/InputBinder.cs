namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;

    internal sealed class InputBinder : DataServiceALinqExpressionVisitor
    {
        private readonly ResourceExpression input;
        private readonly ParameterExpression inputParameter;
        private readonly ResourceSetExpression inputSet;
        private readonly HashSet<ResourceExpression> referencedInputs = new HashSet<ResourceExpression>(EqualityComparer<ResourceExpression>.Default);

        private InputBinder(ResourceExpression resource, ParameterExpression setReferenceParam)
        {
            this.input = resource;
            this.inputSet = resource as ResourceSetExpression;
            this.inputParameter = setReferenceParam;
        }

        internal static Expression Bind(Expression e, ResourceExpression currentInput, ParameterExpression inputParameter, List<ResourceExpression> referencedInputs)
        {
            InputBinder binder = new InputBinder(currentInput, inputParameter);
            Expression expression = binder.Visit(e);
            referencedInputs.AddRange(binder.referencedInputs);
            return expression;
        }

        private Expression CreateReference(ResourceExpression resource)
        {
            this.referencedInputs.Add(resource);
            return resource.CreateReference();
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if ((this.inputSet == null) || !this.inputSet.HasTransparentScope)
            {
                return base.VisitMemberAccess(m);
            }
            ParameterExpression expression = null;
            Stack<PropertyInfo> stack = new Stack<PropertyInfo>();
            for (MemberExpression expression2 = m; ((expression2 != null) && PlatformHelper.IsProperty(expression2.Member)) && (expression2.Expression != null); expression2 = expression2.Expression as MemberExpression)
            {
                stack.Push((PropertyInfo) expression2.Member);
                if (expression2.Expression.NodeType == ExpressionType.Parameter)
                {
                    expression = (ParameterExpression) expression2.Expression;
                }
            }
            if ((expression != this.inputParameter) || (stack.Count == 0))
            {
                return m;
            }
            ResourceExpression input = this.input;
            ResourceSetExpression inputSet = this.inputSet;
            bool flag = false;
            while (stack.Count > 0)
            {
                if ((inputSet == null) || !inputSet.HasTransparentScope)
                {
                    break;
                }
                PropertyInfo info = stack.Peek();
                if (info.Name.Equals(inputSet.TransparentScope.Accessor, StringComparison.Ordinal))
                {
                    input = inputSet;
                    stack.Pop();
                    flag = true;
                }
                else
                {
                    Expression expression5;
                    if (!inputSet.TransparentScope.SourceAccessors.TryGetValue(info.Name, out expression5))
                    {
                        break;
                    }
                    flag = true;
                    stack.Pop();
                    InputReferenceExpression expression6 = expression5 as InputReferenceExpression;
                    if (expression6 == null)
                    {
                        inputSet = expression5 as ResourceSetExpression;
                        if ((inputSet == null) || !inputSet.HasTransparentScope)
                        {
                            input = (ResourceExpression) expression5;
                        }
                        continue;
                    }
                    inputSet = expression6.Target as ResourceSetExpression;
                    input = inputSet;
                }
            }
            if (!flag)
            {
                return m;
            }
            Expression expression7 = this.CreateReference(input);
            while (stack.Count > 0)
            {
                expression7 = Expression.Property(expression7, stack.Pop());
            }
            return expression7;
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            if (((this.inputSet == null) || !this.inputSet.HasTransparentScope) && (p == this.inputParameter))
            {
                return this.CreateReference(this.input);
            }
            return base.VisitParameter(p);
        }
    }
}

