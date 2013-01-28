namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Management.Automation;

    internal class PSToObjectArrayBinder : DynamicMetaObjectBinder
    {
        private static readonly PSToObjectArrayBinder _binder = new PSToObjectArrayBinder();

        private PSToObjectArrayBinder()
        {
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, args);
            }
            if ((target.Value is PSObject) && (PSObject.Base(target.Value) != target.Value))
            {
                return this.DeferForPSObject(new DynamicMetaObject[] { target }).WriteToDebugLog(this);
            }
            DynamicMetaObject obj2 = PSEnumerableBinder.IsEnumerable(target);
            if (obj2 == null)
            {
                return new DynamicMetaObject(Expression.NewArrayInit(typeof(object), new Expression[] { target.Expression.Cast(typeof(object)) }), target.PSGetTypeRestriction()).WriteToDebugLog(this);
            }
            if (PSObject.Base(target.Value) is ArrayList)
            {
                return new DynamicMetaObject(Expression.Call(PSEnumerableBinder.MaybeDebase(this, e => e.Cast(typeof(ArrayList)), target), CachedReflectionInfo.ArrayList_ToArray), PSEnumerableBinder.GetRestrictions(target)).WriteToDebugLog(this);
            }
            return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.EnumerableOps_ToArray, obj2.Expression), target.PSGetTypeRestriction()).WriteToDebugLog(this);
        }

        internal static PSToObjectArrayBinder Get()
        {
            return _binder;
        }

        public override string ToString()
        {
            return "ToObjectArray";
        }
    }
}

