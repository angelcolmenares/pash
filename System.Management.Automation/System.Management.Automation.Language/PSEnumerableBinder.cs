namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Xml;

    internal class PSEnumerableBinder : ConvertBinder
    {
        private static readonly PSEnumerableBinder _binder = new PSEnumerableBinder();
        private static readonly Type ComObjectType = typeof(object).Assembly.GetType("System.__ComObject");
        private static readonly object[] EmptyArray = new object[0];

        private PSEnumerableBinder() : base(typeof(IEnumerator), false)
        {
        }

        public override DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]).WriteToDebugLog(this);
            }
            if (target.Value == AutomationNull.Value)
            {
                return new DynamicMetaObject(Expression.Call(Expression.Constant(EmptyArray), typeof(Array).GetMethod("GetEnumerator")), BindingRestrictions.GetInstanceRestriction(target.Expression, AutomationNull.Value)).WriteToDebugLog(this);
            }
            object obj2 = PSObject.Base(target.Value);
            if (((obj2 != null) && !(obj2 is string)) && !(obj2 is PSObject))
            {
                if (obj2.GetType().IsArray)
                {
                    return new DynamicMetaObject(MaybeDebase(this, e => Expression.Call(Expression.Convert(e, typeof(Array)), typeof(Array).GetMethod("GetEnumerator")), target), GetRestrictions(target)).WriteToDebugLog(this);
                }
                if ((obj2 is IDictionary) || (obj2 is System.Xml.XmlNode))
                {
                    return (errorSuggestion ?? this.NullResult(target)).WriteToDebugLog(this);
                }
                if (obj2 is DataTable)
                {
                    return new DynamicMetaObject(MaybeDebase(this, delegate (Expression e) {
                        ParameterExpression expression;
                        ParameterExpression expression2;
                        return Expression.Block(new ParameterExpression[] { expression = Expression.Parameter(typeof(DataTable), "table"), expression2 = Expression.Parameter(typeof(DataRowCollection), "rows") }, new Expression[] { Expression.Assign(expression, e.Cast(typeof(DataTable))), Expression.Condition(Expression.NotEqual(Expression.Assign(expression2, Expression.Property(expression, "Rows")), ExpressionCache.NullConstant), Expression.Call(expression2, typeof(DataRowCollection).GetMethod("GetEnumerator")), ExpressionCache.NullEnumerator) });
                    }, target), GetRestrictions(target)).WriteToDebugLog(this);
                }
                if (IsComObject(obj2))
                {
                    return new DynamicMetaObject(MaybeDebase(this, e => Expression.Call(CachedReflectionInfo.EnumerableOps_GetCOMEnumerator, e), target), GetRestrictions(target)).WriteToDebugLog(this);
                }
                if (obj2 is IEnumerable)
                {
                    Type[] interfaces = obj2.GetType().GetInterfaces();
                    for (int j = 0; j < interfaces.Length; j++)
                    {
                        Func<Expression, Expression> generator = null;
                        Type i = interfaces[j];
                        if (i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                        {
                            if (generator == null)
                            {
                                generator = e => Expression.Call(CachedReflectionInfo.EnumerableOps_GetGenericEnumerator.MakeGenericMethod(new Type[] { i.GetGenericArguments()[0] }), Expression.Convert(e, i));
                            }
                            return new DynamicMetaObject(MaybeDebase(this, generator, target), GetRestrictions(target)).WriteToDebugLog(this);
                        }
                    }
                    return new DynamicMetaObject(MaybeDebase(this, e => Expression.Call(CachedReflectionInfo.EnumerableOps_GetEnumerator, Expression.Convert(e, typeof(IEnumerable))), target), GetRestrictions(target)).WriteToDebugLog(this);
                }
                if (obj2 is IEnumerator)
                {
                    return new DynamicMetaObject(MaybeDebase(this, e => e.Cast(typeof(IEnumerator)), target), GetRestrictions(target)).WriteToDebugLog(this);
                }
            }
            return (errorSuggestion ?? this.NullResult(target)).WriteToDebugLog(this);
        }

        internal static PSEnumerableBinder Get()
        {
            return _binder;
        }

        internal static BindingRestrictions GetRestrictions(DynamicMetaObject target)
        {
            if (target.Value is PSObject)
            {
                return BindingRestrictions.GetTypeRestriction(target.Expression, typeof(PSObject));
            }
            return target.PSGetTypeRestriction();
        }

        internal static bool IsComObject(object obj)
        {
            return ((obj != null) && ComObjectType.IsAssignableFrom(obj.GetType()));
        }

        internal static DynamicMetaObject IsEnumerable(DynamicMetaObject target)
        {
            DynamicMetaObject obj2 = Get().FallbackConvert(target, DynamicMetaObjectExtensions.FakeError);
            if (obj2 != DynamicMetaObjectExtensions.FakeError)
            {
                return obj2;
            }
            return null;
        }

        internal static bool IsStaticTypePossiblyEnumerable(Type type)
        {
            if ((!type.Equals(typeof(object)) && !type.Equals(typeof(PSObject))) && !type.IsArray)
            {
                if ((type.Equals(typeof(string)) || typeof(IDictionary).IsAssignableFrom(type)) || typeof(System.Xml.XmlNode).IsAssignableFrom(type))
                {
                    return false;
                }
                if ((type.IsSealed && !typeof(IEnumerable).IsAssignableFrom(type)) && !typeof(IEnumerator).IsAssignableFrom(type))
                {
                    return false;
                }
            }
            return true;
        }

        internal static Expression MaybeDebase(DynamicMetaObjectBinder binder, Func<Expression, Expression> generator, DynamicMetaObject target)
        {
            ParameterExpression expression;
            if (!(target.Value is PSObject))
            {
                return generator(target.Expression);
            }
            object obj2 = PSObject.Base(target.Value);
            return Expression.Block(new ParameterExpression[] { expression = Expression.Parameter(typeof(object), "value") }, new Expression[] { Expression.Assign(expression, Expression.Call(CachedReflectionInfo.PSObject_Base, target.Expression)), Expression.Condition((obj2 == null) ? ((Expression) Expression.AndAlso(Expression.Equal(expression, ExpressionCache.NullConstant), Expression.Not(Expression.Equal(target.Expression, ExpressionCache.AutomationNullConstant)))) : ((Expression) Expression.TypeEqual(expression, obj2.GetType())), generator(expression), binder.GetUpdateExpression(binder.ReturnType)) });
        }

        private DynamicMetaObject NullResult(DynamicMetaObject target)
        {
            return new DynamicMetaObject(MaybeDebase(this, e => ExpressionCache.NullEnumerator, target), GetRestrictions(target));
        }

        public override string ToString()
        {
            return "ToEnumerable";
        }
    }
}

