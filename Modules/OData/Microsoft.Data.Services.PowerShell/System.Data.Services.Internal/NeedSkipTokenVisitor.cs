namespace System.Data.Services.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal sealed class NeedSkipTokenVisitor : ALinqExpressionVisitor
    {
        private ResourceProperty property;
        private readonly ResourceType rt;

        private NeedSkipTokenVisitor() : this(null)
        {
        }

        private NeedSkipTokenVisitor(ResourceType rt)
        {
            this.rt = rt;
        }

        internal static ICollection<ResourceProperty> CollectSkipTokenProperties(OrderingInfo orderingInfo, ResourceType rt)
        {
            List<ResourceProperty> list = new List<ResourceProperty>();
            foreach (OrderingExpression expression in orderingInfo.OrderingExpressions)
            {
                LambdaExpression expression2 = (LambdaExpression) expression.Expression;
                NeedSkipTokenVisitor visitor = new NeedSkipTokenVisitor(rt);
                visitor.Visit(expression2.Body);
                if (visitor.NeedSkipToken)
                {
                    return null;
                }
                list.Add(visitor.Property);
            }
            return list;
        }

        internal static bool IsSkipTokenRequired(OrderingInfo orderingInfo)
        {
            if ((orderingInfo != null) && orderingInfo.IsPaged)
            {
                foreach (OrderingExpression expression in orderingInfo.OrderingExpressions)
                {
                    LambdaExpression expression2 = (LambdaExpression) expression.Expression;
                    NeedSkipTokenVisitor visitor = new NeedSkipTokenVisitor();
                    visitor.Visit(expression2.Body);
                    if (visitor.NeedSkipToken)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal override Expression Visit(Expression exp)
        {
            if (exp != null)
            {
                switch (exp.NodeType)
                {
                    case ExpressionType.MemberAccess:
                    case ExpressionType.Parameter:
                        return base.Visit(exp);
                }
                this.NeedSkipToken = true;
            }
            return exp;
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if ((m.Member.MemberType != MemberTypes.Property) || (m.Expression.NodeType != ExpressionType.Parameter))
            {
                this.NeedSkipToken = true;
                return m;
            }
            if (this.rt != null)
            {
                ResourcePropertyKind stream = ResourcePropertyKind.Stream;
                this.property = this.rt.TryResolvePropertyName(m.Member.Name, stream);
            }
            return base.VisitMemberAccess(m);
        }

        internal override Expression VisitParameter(ParameterExpression p)
        {
            if ((this.rt != null) && (p.Type != this.rt.InstanceType))
            {
                this.NeedSkipToken = true;
                return p;
            }
            return base.VisitParameter(p);
        }

        private bool NeedSkipToken { get; set; }

        private ResourceProperty Property
        {
            get
            {
                if (this.rt == null)
                {
                    return null;
                }
                return this.property;
            }
        }
    }
}

