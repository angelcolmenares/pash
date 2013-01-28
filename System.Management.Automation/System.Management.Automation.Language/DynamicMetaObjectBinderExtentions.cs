namespace System.Management.Automation.Language
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal static class DynamicMetaObjectBinderExtentions
    {
        internal static DynamicMetaObject DeferForPSObject(this DynamicMetaObjectBinder binder, params DynamicMetaObject[] args)
        {
            Expression[] arguments = new Expression[args.Length];
            BindingRestrictions empty = BindingRestrictions.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                if (PSObject.Base(args[i].Value) != args[i].Value)
                {
                    arguments[i] = Expression.Call(CachedReflectionInfo.PSObject_Base, args[i].Expression.Cast(typeof(object)));
                    empty = empty.Merge(args[i].GetSimpleTypeRestriction()).Merge(BindingRestrictions.GetExpressionRestriction(Expression.NotEqual(arguments[i], args[i].Expression)));
                }
                else
                {
                    arguments[i] = args[i].Expression;
                    empty = empty.Merge(args[i].PSGetTypeRestriction());
                }
            }
            return new DynamicMetaObject(Expression.Dynamic(binder, binder.ReturnType, arguments), empty);
        }
    }
}

