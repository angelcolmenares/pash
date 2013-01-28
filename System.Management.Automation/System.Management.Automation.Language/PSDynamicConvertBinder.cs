namespace System.Management.Automation.Language
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;

    internal class PSDynamicConvertBinder : DynamicMetaObjectBinder
    {
        private static readonly PSDynamicConvertBinder _binder = new PSDynamicConvertBinder();

        private PSDynamicConvertBinder()
        {
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            DynamicMetaObject obj2 = args[0];
            if (!target.HasValue || !obj2.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[] { obj2 }).WriteToDebugLog(this);
            }
            Type instance = target.Value as Type;
            BindingRestrictions restrictions = BindingRestrictions.GetInstanceRestriction(target.Expression, instance).Merge(obj2.PSGetTypeRestriction());
            return new DynamicMetaObject(Expression.Dynamic(PSConvertBinder.Get(instance), instance, obj2.Expression).Cast(typeof(object)), restrictions).WriteToDebugLog(this);
        }

        internal static PSDynamicConvertBinder Get()
        {
            return _binder;
        }
    }
}

