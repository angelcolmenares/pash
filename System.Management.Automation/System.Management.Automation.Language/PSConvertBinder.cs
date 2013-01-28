namespace System.Management.Automation.Language
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class PSConvertBinder : ConvertBinder
    {
        private static readonly Dictionary<Type, PSConvertBinder> _binderCache = new Dictionary<Type, PSConvertBinder>();
        internal int _version;

        private PSConvertBinder(Type type) : base(type, false)
        {
            this._version = 0;
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            bool flag;
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]).WriteToDebugLog(this);
            }
            if (target.Value == AutomationNull.Value)
            {
                return new DynamicMetaObject(Expression.Default(base.Type), target.PSGetTypeRestriction()).WriteToDebugLog(this);
            }
            Type resultType = base.Type;
            LanguagePrimitives.ConversionData conversion = LanguagePrimitives.FigureConversion(target.Value, resultType, out flag);
            if ((errorSuggestion != null) && (target.Value is DynamicObject))
            {
                return errorSuggestion.WriteToDebugLog(this);
            }
            BindingRestrictions restrictions = target.PSGetTypeRestriction().Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, resultType, this._version));
            return new DynamicMetaObject(InvokeConverter(conversion, target.Expression, resultType, flag, ExpressionCache.InvariantCulture), restrictions).WriteToDebugLog(this);
        }

        public static PSConvertBinder Get(Type type)
        {
            PSConvertBinder binder;
            lock (_binderCache)
            {
                if (!_binderCache.TryGetValue(type, out binder))
                {
                    binder = new PSConvertBinder(type);
                    _binderCache.Add(type, binder);
                }
            }
            return binder;
        }

        internal static void InvalidateCache()
        {
            lock (_binderCache)
            {
                foreach (PSConvertBinder binder in _binderCache.Values)
                {
                    binder._version++;
                }
            }
        }

        internal static Expression InvokeConverter(LanguagePrimitives.ConversionData conversion, Expression value, Type resultType, bool debase, Expression formatProvider)
        {
            Expression expression;
            if ((conversion.Rank == ConversionRank.Identity) || (conversion.Rank == ConversionRank.Assignable))
            {
                expression = debase ? Expression.Call(CachedReflectionInfo.PSObject_Base, value) : value;
            }
            else
            {
                Expression expression2;
                Expression nullPSObject;
                if (debase)
                {
                    expression2 = Expression.Call(CachedReflectionInfo.PSObject_Base, value);
                    nullPSObject = value.Cast(typeof(PSObject));
                }
                else
                {
                    expression2 = value.Cast(typeof(object));
                    nullPSObject = ExpressionCache.NullPSObject;
                }
                expression = Expression.Call(Expression.Constant(conversion.Converter), conversion.Converter.GetType().GetMethod("Invoke"), new Expression[] { expression2, Expression.Constant(resultType), ExpressionCache.Constant(true), nullPSObject, formatProvider, ExpressionCache.NullTypeTable });
            }
            if (expression.Type.Equals(resultType) || resultType.Equals(typeof(LanguagePrimitives.InternalPSCustomObject)))
            {
                return expression;
            }
            if (resultType.IsValueType && (Nullable.GetUnderlyingType(resultType) == null))
            {
                return Expression.Unbox(expression, resultType);
            }
            return Expression.Convert(expression, resultType);
        }

        internal static DynamicMetaObject ThrowNoConversion(DynamicMetaObject target, Type toType, DynamicMetaObjectBinder binder, int currentVersion, params DynamicMetaObject[] args)
        {
            Expression expression = Expression.Call(CachedReflectionInfo.LanguagePrimitives_ThrowInvalidCastException, target.Expression.Cast(typeof(object)), Expression.Constant(toType));
            if (!binder.ReturnType.Equals(typeof(void)))
            {
                expression = Expression.Block(expression, Expression.Default(binder.ReturnType));
            }
            return new DynamicMetaObject(expression, target.CombineRestrictions(args).Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(binder, toType, currentVersion)));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "PSConvertBinder [{0}]  ver:{1}", new object[] { ToStringCodeMethods.Type(base.Type, true), this._version });
        }
    }
}

