namespace System.Management.Automation.Language
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Management.Automation;

    internal class PSToStringBinder : DynamicMetaObjectBinder
    {
        private static readonly PSToStringBinder _binder = new PSToStringBinder();

        private PSToStringBinder()
        {
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (!target.HasValue || !args[0].HasValue)
            {
                return base.Defer(target, args).WriteToDebugLog(this);
            }
            if ((target.Value is PSObject) && (PSObject.Base(target.Value) != target.Value))
            {
                return this.DeferForPSObject(new DynamicMetaObject[] { target, args[0] }).WriteToDebugLog(this);
            }
            BindingRestrictions restrictions = target.PSGetTypeRestriction();
            if (target.LimitType.Equals(typeof(string)))
            {
                return new DynamicMetaObject(target.Expression.Cast(typeof(string)), restrictions).WriteToDebugLog(this);
            }
            return new DynamicMetaObject(InvokeToString(args[0].Expression, target.Expression), restrictions).WriteToDebugLog(this);
        }

        internal static PSToStringBinder Get()
        {
            return _binder;
        }

        internal static Expression InvokeToString(Expression context, Expression target)
        {
            if (target.Type.Equals(typeof(string)))
            {
                return target;
            }
            return Expression.Call(CachedReflectionInfo.PSObject_ToStringParser, context.Cast(typeof(ExecutionContext)), target.Cast(typeof(object)));
        }

        public override Type ReturnType
        {
            get
            {
                return typeof(string);
            }
        }
    }
}

