namespace System.Data.Services.Client
{
    using System;
    using System.Data.Services.Client.Metadata;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class ProjectionRewriter : ALinqExpressionVisitor
    {
        private readonly ParameterExpression newLambdaParameter;
        private ParameterExpression oldLambdaParameter;
        private ResourceExpression projectionSource;
        private bool successfulRebind;

        private ProjectionRewriter(Type proposedParameterType)
        {
            this.newLambdaParameter = Expression.Parameter(proposedParameterType, "it");
        }

        internal LambdaExpression Rebind(LambdaExpression lambda, ResourceExpression source)
        {
            this.successfulRebind = true;
            this.oldLambdaParameter = lambda.Parameters[0];
            this.projectionSource = source;
            Expression body = this.Visit(lambda.Body);
            if (!this.successfulRebind)
            {
                throw new NotSupportedException(System.Data.Services.Client.Strings.ALinq_CanOnlyProjectTheLeaf);
            }
            return Expression.Lambda(typeof(Func<,>).MakeGenericType(new Type[] { this.newLambdaParameter.Type, lambda.Body.Type }), body, new ParameterExpression[] { this.newLambdaParameter });
        }

        internal static LambdaExpression TryToRewrite(LambdaExpression le, ResourceExpression source)
        {
            Type proposedParameterType = source.ResourceType;
            if ((!ResourceBinder.PatternRules.MatchSingleArgumentLambda(le, out le) || ClientTypeUtil.TypeOrElementTypeIsEntity(le.Parameters[0].Type)) || !le.Parameters[0].Type.GetProperties().Any<PropertyInfo>(p => (p.PropertyType == proposedParameterType)))
            {
                return le;
            }
            ProjectionRewriter rewriter = new ProjectionRewriter(proposedParameterType);
            return rewriter.Rebind(le, source);
        }

        internal override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression == this.oldLambdaParameter)
            {
                ResourceSetExpression projectionSource = this.projectionSource as ResourceSetExpression;
                if (((projectionSource != null) && projectionSource.HasTransparentScope) && (projectionSource.TransparentScope.Accessor == m.Member.Name))
                {
                    return this.newLambdaParameter;
                }
                this.successfulRebind = false;
            }
            return base.VisitMemberAccess(m);
        }
    }
}

