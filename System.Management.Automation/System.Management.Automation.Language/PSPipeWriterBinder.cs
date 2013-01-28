namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class PSPipeWriterBinder : DynamicMetaObjectBinder
    {
        private static readonly PSPipeWriterBinder _binder = new PSPipeWriterBinder();

        private PSPipeWriterBinder()
        {
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, args);
            }
            if (target.Value == AutomationNull.Value)
            {
                return new DynamicMetaObject(Expression.Block(typeof(void), new Expression[] { Expression.Call(CachedReflectionInfo.PipelineOps_Nop, new Expression[0]) }), BindingRestrictions.GetInstanceRestriction(target.Expression, AutomationNull.Value)).WriteToDebugLog(this);
            }
            DynamicMetaObject obj2 = PSEnumerableBinder.IsEnumerable(target);
            if (obj2 == null)
            {
                DynamicMetaObject obj3 = PSVariableAssignmentBinder.Get().Bind(target, new DynamicMetaObject[0]);
                BindingRestrictions restrictions = target.LimitType.IsValueType ? obj3.Restrictions : target.PSGetTypeRestriction();
                return new DynamicMetaObject(Expression.Call(args[0].Expression, CachedReflectionInfo.Pipe_Add, new Expression[] { obj3.Expression.Cast(typeof(object)) }), restrictions).WriteToDebugLog(this);
            }
            bool b = !(PSObject.Base(target.Value) is IEnumerator);
            return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.EnumerableOps_WriteEnumerableToPipe, obj2.Expression, args[0].Expression, args[1].Expression, ExpressionCache.Constant(b)), obj2.Restrictions).WriteToDebugLog(this);
        }

        internal static PSPipeWriterBinder Get()
        {
            return _binder;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "PipelineWriter", new object[0]);
        }
    }
}

