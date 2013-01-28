namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Management.Automation;

    internal class PSSetDynamicMemberBinder : DynamicMetaObjectBinder
    {
        private static readonly PSSetDynamicMemberBinder _instanceBinder = new PSSetDynamicMemberBinder(false);
        private readonly bool _static;
        private static readonly PSSetDynamicMemberBinder _staticBinder = new PSSetDynamicMemberBinder(true);

        private PSSetDynamicMemberBinder(bool @static)
        {
            this._static = @static;
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            string str;
            Expression expression;
            Expression expression2;
            if (!target.HasValue || !args[0].HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[] { args[0] }).WriteToDebugLog(this);
            }
            DynamicMetaObject obj2 = args[0];
            object obj3 = PSObject.Base(obj2.Value);
            if (obj3 is string)
            {
                str = (string) obj3;
                if (obj2.Value is PSObject)
                {
                    expression = Expression.Call(CachedReflectionInfo.PSObject_Base, obj2.Expression).Cast(typeof(string));
                }
                else
                {
                    expression = obj2.Expression.Cast(typeof(string));
                }
            }
            else
            {
                str = PSObject.ToStringParser(null, obj2.Value);
                expression = PSToStringBinder.InvokeToString(ExpressionCache.NullConstant, obj2.Expression);
            }
            DynamicMetaObject obj4 = PSSetMemberBinder.Get(str, this._static).FallbackSetMember(target, args[1]);
            BindingRestrictions restrictions = obj4.Restrictions.Merge(args[0].PSGetTypeRestriction()).Merge(BindingRestrictions.GetExpressionRestriction(Expression.Call(CachedReflectionInfo.String_Equals, Expression.Constant(str), expression, ExpressionCache.Ordinal)));
            if (target.Value is IDictionary)
            {
                ParameterExpression variable = Expression.Variable(typeof(Exception));
                expression2 = Expression.TryCatch(PSSetIndexBinder.Get(1, null).FallbackSetIndex(target, new DynamicMetaObject[] { args[0] }, args[1]).Expression, new CatchBlock[] { Expression.Catch(variable, Expression.Block(Expression.Call(CachedReflectionInfo.CommandProcessorBase_CheckForSevereException, variable), obj4.Expression)) });
            }
            else
            {
                expression2 = obj4.Expression;
            }
            return new DynamicMetaObject(expression2, restrictions).WriteToDebugLog(this);
        }

        internal static PSSetDynamicMemberBinder Get(bool @static)
        {
            if (!@static)
            {
                return _instanceBinder;
            }
            return _staticBinder;
        }
    }
}

