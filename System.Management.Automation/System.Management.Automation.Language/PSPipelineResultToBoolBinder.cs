namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class PSPipelineResultToBoolBinder : DynamicMetaObjectBinder
    {
        private static readonly PSPipelineResultToBoolBinder _binder = new PSPipelineResultToBoolBinder();

        private PSPipelineResultToBoolBinder()
        {
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            Expression expression3;
            BindingRestrictions expressionRestriction;
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]).WriteToDebugLog(this);
            }
            IList list = target.Value as IList;
            Expression expression = target.Expression;
            if (!typeof(IList).Equals(expression.Type))
            {
                expression = Expression.Convert(expression, typeof(IList));
            }
            MemberExpression left = Expression.Property(Expression.Convert(expression, typeof(ICollection)), CachedReflectionInfo.ICollection_Count);
            switch (list.Count)
            {
                case 0:
                    expression3 = ExpressionCache.Constant(false);
                    expressionRestriction = BindingRestrictions.GetExpressionRestriction(Expression.Equal(left, ExpressionCache.Constant(0)));
                    break;

                case 1:
                    expression3 = Expression.Call(expression, CachedReflectionInfo.IList_get_Item, new Expression[] { ExpressionCache.Constant(0) }).Convert(typeof(bool));
                    expressionRestriction = BindingRestrictions.GetExpressionRestriction(Expression.Equal(left, ExpressionCache.Constant(1)));
                    break;

                default:
                    expression3 = ExpressionCache.Constant(true);
                    expressionRestriction = BindingRestrictions.GetExpressionRestriction(Expression.GreaterThan(left, ExpressionCache.Constant(1)));
                    break;
            }
            return new DynamicMetaObject(expression3, expressionRestriction).WriteToDebugLog(this);
        }

        internal static PSPipelineResultToBoolBinder Get()
        {
            return _binder;
        }

        public override Type ReturnType
        {
            get
            {
                return typeof(bool);
            }
        }
    }
}

