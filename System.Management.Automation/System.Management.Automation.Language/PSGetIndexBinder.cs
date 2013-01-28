namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class PSGetIndexBinder : GetIndexBinder
    {
        private readonly bool _allowSlicing;
        private static readonly Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints, bool>, PSGetIndexBinder> _binderCache = new Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints, bool>, PSGetIndexBinder>();
        private readonly PSMethodInvocationConstraints _constraints;
        internal int _version;

        private PSGetIndexBinder(Tuple<CallInfo, PSMethodInvocationConstraints, bool> tuple) : base(tuple.Item1)
        {
            this._constraints = tuple.Item2;
            this._allowSlicing = tuple.Item3;
            this._version = 0;
        }

        internal static bool CanIndexFromEndWithNegativeIndex(DynamicMetaObject target)
        {
            Type limitType = target.LimitType;
            return (((limitType.IsArray || limitType.Equals(typeof(string))) || limitType.Equals(typeof(StringBuilder))) || (typeof(IList).IsAssignableFrom(limitType) || (typeof(OrderedDictionary).IsAssignableFrom(limitType) || (from i in limitType.GetInterfaces()
                where i.IsGenericType && i.GetGenericTypeDefinition().Equals(typeof(IList<>))
                select i).Any<Type>())));
        }

        private DynamicMetaObject CannotIndexTarget(DynamicMetaObject target, DynamicMetaObject[] indexes)
        {
            return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.ArrayOps_GetNonIndexable, target.Expression.Cast(typeof(object)), Expression.NewArrayInit(typeof(object), (IEnumerable<Expression>) (from d in indexes select d.Expression.Cast(typeof(object))))), target.CombineRestrictions(indexes).Merge(BinderUtils.GetVersionCheck(this, this._version)).Merge(BinderUtils.GetLanguageModeCheckIfHasEverUsedConstrainedLanguage()));
        }

        private DynamicMetaObject CheckForSlicing(DynamicMetaObject target, DynamicMetaObject[] indexes)
        {
            if (this._allowSlicing)
            {
                if (indexes.Length > 1)
                {
                    PSGetIndexBinder nonSlicingBinder = Get(1, this._constraints, false);
                    return new DynamicMetaObject(Expression.NewArrayInit(typeof(object), (IEnumerable<Expression>) (from i in indexes select Expression.Dynamic(nonSlicingBinder, typeof(object), target.Expression, i.Expression))), target.CombineRestrictions(indexes));
                }
                DynamicMetaObject obj2 = PSEnumerableBinder.IsEnumerable(indexes[0]);
                if (obj2 != null)
                {
                    return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.EnumerableOps_SlicingIndex, target.Expression.Cast(typeof(object)), obj2.Expression.Cast(typeof(IEnumerator)), Expression.Constant(this.GetNonSlicingIndexer())), target.CombineRestrictions(new DynamicMetaObject[] { obj2 }));
                }
            }
            return null;
        }

        internal static Expression ConvertIndex(DynamicMetaObject index, Type resultType)
        {
            bool flag;
            LanguagePrimitives.ConversionData conversion = LanguagePrimitives.FigureConversion(index.Value, resultType, out flag);
            if (conversion.Rank != ConversionRank.None)
            {
                return PSConvertBinder.InvokeConverter(conversion, index.Expression, resultType, flag, ExpressionCache.InvariantCulture);
            }
            return null;
        }

        public override DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            if (target.HasValue && !(from mo in indexes
                where !mo.HasValue
                select mo).Any<DynamicMetaObject>())
            {
                if ((!(target.Value is PSObject) || (PSObject.Base(target.Value) == target.Value)) && !(from mo in indexes
                    where (mo.Value is PSObject) && (PSObject.Base(mo.Value) != mo.Value)
                    select mo).Any<DynamicMetaObject>())
                {
                    if (target.Value == null)
                    {
                        return (errorSuggestion ?? target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NullArray", ParserStrings.NullArray, new Expression[0])).WriteToDebugLog(this);
                    }
                    if (((indexes.Length == 1) && (indexes[0].Value == null)) && this._allowSlicing)
                    {
                        return (errorSuggestion ?? target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NullArrayIndex", ParserStrings.NullArrayIndex, new Expression[0])).WriteToDebugLog(this);
                    }
                    if (target.LimitType.IsArray)
                    {
                        return this.GetIndexArray(target, indexes, errorSuggestion).WriteToDebugLog(this);
                    }
                    foreach (Type type in target.LimitType.GetInterfaces())
                    {
                        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                        {
                            DynamicMetaObject obj2 = this.GetIndexDictionary(target, indexes, type);
                            if (obj2 != null)
                            {
                                return obj2.WriteToDebugLog(this);
                            }
                        }
                    }
                    DefaultMemberAttribute attribute = target.LimitType.GetCustomAttributes<DefaultMemberAttribute>(true).FirstOrDefault<DefaultMemberAttribute>();
                    if (attribute != null)
                    {
                        return this.InvokeIndexer(target, indexes, errorSuggestion, attribute.MemberName).WriteToDebugLog(this);
                    }
                    return (errorSuggestion ?? this.CannotIndexTarget(target, indexes).WriteToDebugLog(this));
                }
                return this.DeferForPSObject(indexes.Prepend<DynamicMetaObject>(target).ToArray<DynamicMetaObject>()).WriteToDebugLog(this);
            }
            return base.Defer(indexes.Prepend<DynamicMetaObject>(target).ToArray<DynamicMetaObject>()).WriteToDebugLog(this);
        }

        public static PSGetIndexBinder Get(int argCount, PSMethodInvocationConstraints constraints, bool allowSlicing = true)
        {
            lock (_binderCache)
            {
                PSGetIndexBinder binder;
                Tuple<CallInfo, PSMethodInvocationConstraints, bool> key = Tuple.Create<CallInfo, PSMethodInvocationConstraints, bool>(new CallInfo(argCount, new string[0]), constraints, allowSlicing);
                if (!_binderCache.TryGetValue(key, out binder))
                {
                    binder = new PSGetIndexBinder(key);
                    _binderCache.Add(key, binder);
                }
                return binder;
            }
        }

        private DynamicMetaObject GetIndexArray(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            Array array = (Array) target.Value;
            if (array.Rank > 1)
            {
                return this.GetIndexMultiDimensionArray(target, indexes, errorSuggestion);
            }
            if (indexes.Length > 1)
            {
                if (!this._allowSlicing && (errorSuggestion == null))
                {
                    return this.CannotIndexTarget(target, indexes);
                }
                return this.InvokeSlicingIndexer(target, indexes);
            }
            DynamicMetaObject obj2 = this.CheckForSlicing(target, indexes);
            if (obj2 != null)
            {
                return obj2;
            }
            Expression expression = ConvertIndex(indexes[0], typeof(int));
            if (expression == null)
            {
                return (errorSuggestion ?? PSConvertBinder.ThrowNoConversion(target, typeof(int), this, this._version, indexes));
            }
            return this.IndexWithNegativeChecks(new DynamicMetaObject(target.Expression.Cast(target.LimitType), target.PSGetTypeRestriction()), new DynamicMetaObject(expression, indexes[0].PSGetTypeRestriction()), target.LimitType.GetProperty("Length"), (t, i) => Expression.ArrayIndex(t, i).Cast(typeof(object)));
        }

        private DynamicMetaObject GetIndexDictionary(DynamicMetaObject target, DynamicMetaObject[] indexes, Type idictionary)
        {
            bool flag;
            if (indexes.Length > 1)
            {
                return null;
            }
            MethodInfo method = idictionary.GetMethod("TryGetValue");
            ParameterInfo[] parameters = method.GetParameters();
            Type parameterType = parameters[0].ParameterType;
            LanguagePrimitives.ConversionData conversion = LanguagePrimitives.FigureConversion(indexes[0].Value, parameterType, out flag);
            if (conversion.Rank == ConversionRank.None)
            {
                return null;
            }
            if (indexes[0].LimitType.IsArray && !parameterType.IsArray)
            {
                return null;
            }
            BindingRestrictions restrictions = target.CombineRestrictions(indexes).Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, parameterType, this._version));
            Expression expression = PSConvertBinder.InvokeConverter(conversion, indexes[0].Expression, parameterType, flag, ExpressionCache.InvariantCulture);
            ParameterExpression expression2 = Expression.Parameter(parameters[1].ParameterType.GetElementType(), "outParam");
            return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { expression2 }, new Expression[] { Expression.Condition(Expression.Call(target.Expression.Cast(idictionary), method, expression, expression2), expression2.Cast(typeof(object)), this.GetNullResult()) }), restrictions);
        }

        private DynamicMetaObject GetIndexMultiDimensionArray(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion)
        {
            Array array = (Array) target.Value;
            if (indexes.Length == 1)
            {
                if (PSEnumerableBinder.IsEnumerable(indexes[0]) == null)
                {
                    return target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NeedMultidimensionalIndex", ParserStrings.NeedMultidimensionalIndex, new Expression[] { ExpressionCache.Constant(array.Rank), Expression.Dynamic(PSToStringBinder.Get(), typeof(string), indexes[0].Expression, ExpressionCache.GetExecutionContextFromTLS) });
                }
                return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.ArrayOps_GetMDArrayValueOrSlice, Expression.Convert(target.Expression, typeof(Array)), indexes[0].Expression.Cast(typeof(object))), target.CombineRestrictions(indexes));
            }
            IEnumerable<Expression> source = from index in indexes
                select ConvertIndex(index, typeof(int)) into i
                where i != null
                select i;
            if (source.Count<Expression>() == indexes.Length)
            {
                return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.ArrayOps_GetMDArrayValue, Expression.Convert(target.Expression, typeof(Array)), Expression.NewArrayInit(typeof(int), source), ExpressionCache.Constant(!this._allowSlicing)), target.CombineRestrictions(indexes));
            }
            if (this._allowSlicing)
            {
                return this.InvokeSlicingIndexer(target, indexes);
            }
            return (errorSuggestion ?? this.CannotIndexTarget(target, indexes));
        }

        private Func<object, object, object> GetNonSlicingIndexer()
        {
            ParameterExpression expression = Expression.Parameter(typeof(object));
            ParameterExpression expression2 = Expression.Parameter(typeof(object));
            return Expression.Lambda<Func<object, object, object>>(Expression.Dynamic(Get(1, this._constraints, false), typeof(object), expression, expression2), new ParameterExpression[] { expression, expression2 }).Compile();
        }

        private Expression GetNullResult()
        {
            if (!this._allowSlicing)
            {
                return ExpressionCache.AutomationNullConstant;
            }
            return ExpressionCache.NullConstant;
        }

        private DynamicMetaObject IndexWithNegativeChecks(DynamicMetaObject target, DynamicMetaObject index, PropertyInfo lengthProperty, Func<Expression, Expression, Expression> generateIndexOperation)
        {
            ParameterExpression expression2;
            ParameterExpression expression3;
            ParameterExpression left = Expression.Parameter(target.LimitType, "target");
            Expression expr = Expression.Block(new ParameterExpression[] { left, expression2 = Expression.Parameter(typeof(int), "len"), expression3 = Expression.Parameter(typeof(int), "index") }, new Expression[] { Expression.Assign(left, target.Expression.Cast(target.LimitType)), Expression.Assign(expression2, Expression.Property(left, lengthProperty)), Expression.Assign(expression3, index.Expression), Expression.IfThen(Expression.LessThan(expression3, ExpressionCache.Constant(0)), Expression.Assign(expression3, Expression.Add(expression3, expression2))), generateIndexOperation(left, expression3) });
            return new DynamicMetaObject(this.SafeIndexResult(expr), target.CombineRestrictions(new DynamicMetaObject[] { index }));
        }

        internal static void InvalidateCache()
        {
            lock (_binderCache)
            {
                foreach (PSGetIndexBinder binder in _binderCache.Values)
                {
                    binder._version++;
                }
            }
        }

        private DynamicMetaObject InvokeIndexer(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion, string methodName)
        {
            Func<Expression, Expression, Expression> generateIndexOperation = null;
            MethodInfo getter = PSInvokeMemberBinder.FindBestMethod(target, indexes, "get_" + methodName, false, this._constraints);
            if (getter == null)
            {
                return (this.CheckForSlicing(target, indexes) ?? (errorSuggestion ?? this.CannotIndexTarget(target, indexes)));
            }
            ParameterInfo[] parameters = getter.GetParameters();
            if (parameters.Length != indexes.Length)
            {
                if ((parameters.Length == 1) && this._allowSlicing)
                {
                    return this.InvokeSlicingIndexer(target, indexes);
                }
                return (errorSuggestion ?? this.CannotIndexTarget(target, indexes));
            }
            if (parameters.Length == 1)
            {
                DynamicMetaObject obj2 = this.CheckForSlicing(target, indexes);
                if (obj2 != null)
                {
                    return obj2;
                }
            }
            Expression[] arguments = new Expression[parameters.Length];
            for (int j = 0; j < parameters.Length; j++)
            {
                Type parameterType = parameters[j].ParameterType;
                arguments[j] = ConvertIndex(indexes[j], parameterType);
                if (arguments[j] == null)
                {
                    return (errorSuggestion ?? PSConvertBinder.ThrowNoConversion(target, parameterType, this, this._version, indexes));
                }
            }
            if (((parameters.Length == 1) && parameters[0].ParameterType.Equals(typeof(int))) && CanIndexFromEndWithNegativeIndex(target))
            {
                PropertyInfo lengthProperty = target.LimitType.GetProperty("Count") ?? target.LimitType.GetProperty("Length");
                if (lengthProperty != null)
                {
                    if (generateIndexOperation == null)
                    {
                        generateIndexOperation = (t, i) => Expression.Call(t, getter, new Expression[] { i }).Cast(typeof(object));
                    }
                    return this.IndexWithNegativeChecks(new DynamicMetaObject(target.Expression.Cast(target.LimitType), target.PSGetTypeRestriction()), new DynamicMetaObject(arguments[0], indexes[0].PSGetTypeRestriction()), lengthProperty, generateIndexOperation);
                }
            }
            return new DynamicMetaObject(this.SafeIndexResult(Expression.Call(target.Expression.Cast(target.LimitType), getter, arguments)), target.CombineRestrictions(indexes).Merge(BinderUtils.GetVersionCheck(this, this._version)).Merge(BinderUtils.GetLanguageModeCheckIfHasEverUsedConstrainedLanguage()));
        }

        private DynamicMetaObject InvokeSlicingIndexer(DynamicMetaObject target, DynamicMetaObject[] indexes)
        {
            return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.ArrayOps_SlicingIndex, target.Expression.Cast(typeof(object)), Expression.NewArrayInit(typeof(object), (IEnumerable<Expression>) (from dmo in indexes select dmo.Expression.Cast(typeof(object)))), Expression.Constant(this.GetNonSlicingIndexer())), target.CombineRestrictions(indexes));
        }

        private Expression SafeIndexResult(Expression expr)
        {
            ParameterExpression variable = Expression.Parameter(typeof(Exception));
            return Expression.TryCatch(expr.Cast(typeof(object)), new CatchBlock[] { Expression.Catch(variable, Expression.Block(Expression.Call(CachedReflectionInfo.CommandProcessorBase_CheckForSevereException, variable), Expression.IfThen(Compiler.IsStrictMode(3, null), Expression.Rethrow()), this.GetNullResult())) });
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "PSGetIndexBinder indexCount={0}{1}{2} ver:{3}", new object[] { base.CallInfo.ArgumentCount, this._allowSlicing ? "" : " slicing disallowed", (this._constraints == null) ? "" : (" constraints: " + this._constraints.ToString()), this._version });
        }
    }
}

