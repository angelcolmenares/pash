namespace System.Data.Services.Client
{
    using System;
    using System.Linq.Expressions;

    internal abstract class DataServiceALinqExpressionVisitor : ALinqExpressionVisitor
    {
        protected DataServiceALinqExpressionVisitor()
        {
        }

        internal override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            switch (((ResourceExpressionType) exp.NodeType))
            {
                case ResourceExpressionType.RootResourceSet:
                case ResourceExpressionType.ResourceNavigationProperty:
                    return this.VisitResourceSetExpression((ResourceSetExpression) exp);

                case ResourceExpressionType.ResourceNavigationPropertySingleton:
                    return this.VisitNavigationPropertySingletonExpression((NavigationPropertySingletonExpression) exp);

                case ResourceExpressionType.InputReference:
                    return this.VisitInputReferenceExpression((InputReferenceExpression) exp);
            }
            return base.Visit(exp);
        }

        internal virtual Expression VisitInputReferenceExpression(InputReferenceExpression ire)
        {
            ResourceExpression expression = (ResourceExpression) this.Visit(ire.Target);
            return expression.CreateReference();
        }

        internal virtual Expression VisitNavigationPropertySingletonExpression(NavigationPropertySingletonExpression npse)
        {
            Expression source = this.Visit(npse.Source);
            if (source != npse.Source)
            {
                npse = new NavigationPropertySingletonExpression(npse.Type, source, npse.MemberExpression, npse.MemberExpression.Type, npse.ExpandPaths, npse.CountOption, npse.CustomQueryOptions, npse.Projection, npse.ResourceTypeAs, npse.UriVersion);
            }
            return npse;
        }

        internal virtual Expression VisitResourceSetExpression(ResourceSetExpression rse)
        {
            Expression source = this.Visit(rse.Source);
            if (source != rse.Source)
            {
                rse = new ResourceSetExpression(rse.Type, source, rse.MemberExpression, rse.ResourceType, rse.ExpandPaths, rse.CountOption, rse.CustomQueryOptions, rse.Projection, rse.ResourceTypeAs, rse.UriVersion);
            }
            return rse;
        }
    }
}

