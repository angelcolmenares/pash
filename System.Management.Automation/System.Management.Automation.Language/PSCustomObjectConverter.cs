namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Dynamic;
    using System.Management.Automation;

    internal class PSCustomObjectConverter : DynamicMetaObjectBinder
    {
        private static readonly PSCustomObjectConverter _binder = new PSCustomObjectConverter();

        private PSCustomObjectConverter()
        {
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            bool flag;
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]).WriteToDebugLog(this);
            }
            Type resultType = ((target.Value is OrderedDictionary) || (target.Value is Hashtable)) ? typeof(LanguagePrimitives.InternalPSCustomObject) : typeof(PSObject);
            return new DynamicMetaObject(PSConvertBinder.InvokeConverter(LanguagePrimitives.FigureConversion(target.Value, resultType, out flag), target.Expression, resultType, flag, ExpressionCache.InvariantCulture), target.PSGetTypeRestriction()).WriteToDebugLog(this);
        }

        internal static PSCustomObjectConverter Get()
        {
            return _binder;
        }
    }
}

