namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    internal class PSGetDynamicMemberBinder : DynamicMetaObjectBinder
    {
        private static readonly PSGetDynamicMemberBinder _instanceBinder = new PSGetDynamicMemberBinder(false);
        private readonly bool _static;
        private static readonly PSGetDynamicMemberBinder _staticBinder = new PSGetDynamicMemberBinder(true);

        private PSGetDynamicMemberBinder(bool @static)
        {
            this._static = @static;
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            string str;
            Expression expression;
            BindingRestrictions restrictions;
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
                if (target.Value is IDictionary)
                {
                    restrictions = target.PSGetTypeRestriction().Merge(BindingRestrictions.GetExpressionRestriction(Expression.Not(Expression.TypeIs(args[0].Expression, typeof(string)))));
                    return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.PSGetDynamicMemberBinder_GetIDictionaryMember, target.Expression.Cast(typeof(IDictionary)), args[0].Expression.Cast(typeof(object))), restrictions).WriteToDebugLog(this);
                }
                str = PSObject.ToStringParser(null, obj3);
                expression = PSToStringBinder.InvokeToString(ExpressionCache.NullConstant, obj2.Expression);
            }
            DynamicMetaObject obj4 = PSGetMemberBinder.Get(str, this._static).FallbackGetMember(target);
            restrictions = obj4.Restrictions.Merge(args[0].PSGetTypeRestriction()).Merge(BindingRestrictions.GetExpressionRestriction(Expression.Call(CachedReflectionInfo.String_Equals, Expression.Constant(str), expression, ExpressionCache.Ordinal)));
            return new DynamicMetaObject(obj4.Expression, restrictions).WriteToDebugLog(this);
        }

        internal static PSGetDynamicMemberBinder Get(bool @static)
        {
            if (!@static)
            {
                return _instanceBinder;
            }
            return _staticBinder;
        }

        internal static object GetIDictionaryMember(IDictionary hash, object key)
        {
            try
            {
                key = PSObject.Base(key);
                if (hash.Contains(key))
                {
                    return hash[key];
                }
            }
            catch (InvalidOperationException)
            {
            }
            if (LocalPipeline.GetExecutionContextFromTLS().IsStrictVersion(2))
            {
                throw new PropertyNotFoundException("PropertyNotFoundStrict", null, ParserStrings.PropertyNotFoundStrict, new object[] { LanguagePrimitives.ConvertTo<string>(key) });
            }
            return null;
        }
    }
}

