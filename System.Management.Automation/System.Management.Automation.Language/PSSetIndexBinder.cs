using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;

namespace System.Management.Automation.Language
{
    internal class PSSetIndexBinder : SetIndexBinder
    {
        private readonly static Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints>, PSSetIndexBinder> _binderCache;

        private readonly PSMethodInvocationConstraints _constraints;

        internal int _version;

        static PSSetIndexBinder()
        {
            PSSetIndexBinder._binderCache = new Dictionary<Tuple<CallInfo, PSMethodInvocationConstraints>, PSSetIndexBinder>();
        }

        private PSSetIndexBinder(Tuple<CallInfo, PSMethodInvocationConstraints> tuple)
            : base(tuple.Item1)
        {
            this._constraints = tuple.Item2;
            this._version = 0;
        }

        private DynamicMetaObject CannotIndexTarget(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            BindingRestrictions bindingRestriction = value.PSGetTypeRestriction();
            bindingRestriction = bindingRestriction.Merge(BinderUtils.GetVersionCheck(this, this._version));
            bindingRestriction = bindingRestriction.Merge(BinderUtils.GetLanguageModeCheckIfHasEverUsedConstrainedLanguage());
            Expression[] expressionArray = new Expression[1];
            expressionArray[0] = Expression.Constant(target.LimitType);
            return target.ThrowRuntimeError(indexes, bindingRestriction, "CannotIndex", ParserStrings.CannotIndex, expressionArray);
        }

        public override DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            if (target.HasValue)
            {
                DynamicMetaObject[] dynamicMetaObjectArray = indexes;
                if (!dynamicMetaObjectArray.Where<DynamicMetaObject>((DynamicMetaObject mo) => !mo.HasValue).Any<DynamicMetaObject>() && value.HasValue)
                {
                    if (target.Value as PSObject == null || PSObject.Base(target.Value) == target.Value)
                    {
                        DynamicMetaObject[] dynamicMetaObjectArray1 = indexes;
                        if (!dynamicMetaObjectArray1.Where<DynamicMetaObject>((DynamicMetaObject mo) =>
                        {
                            if ((mo.Value as PSObject == null))
                            {
                                return false;
                            }
                            else
                            {
                                return (PSObject.Base(mo.Value) != mo.Value);
                            }
                        }
                        ).Any<DynamicMetaObject>())
                        {
                            if (target.Value != null)
                            {
                                if ((int)indexes.Length != 1 || indexes[0].Value != null)
                                {
                                    if (!target.LimitType.IsArray)
                                    {
                                        DefaultMemberAttribute defaultMemberAttribute = target.LimitType.GetCustomAttributes<DefaultMemberAttribute>(true).FirstOrDefault<DefaultMemberAttribute>();
                                        if (defaultMemberAttribute == null)
                                        {
                                            DynamicMetaObject dynamicMetaObject = errorSuggestion;
                                            DynamicMetaObject debugLog = dynamicMetaObject;
                                            if (dynamicMetaObject == null)
                                            {
                                                debugLog = this.CannotIndexTarget(target, indexes, value).WriteToDebugLog(this);
                                            }
                                            return debugLog;
                                        }
                                        else
                                        {
                                            return this.InvokeIndexer(target, indexes, value, errorSuggestion, defaultMemberAttribute.MemberName).WriteToDebugLog(this);
                                        }
                                    }
                                    else
                                    {
                                        return this.SetIndexArray(target, indexes, value, errorSuggestion).WriteToDebugLog(this);
                                    }
                                }
                                else
                                {
                                    DynamicMetaObject dynamicMetaObject1 = errorSuggestion;
                                    DynamicMetaObject debugLog1 = dynamicMetaObject1;
                                    if (dynamicMetaObject1 == null)
                                    {
                                        debugLog1 = target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NullArrayIndex", ParserStrings.NullArrayIndex, new Expression[0]).WriteToDebugLog(this);
                                    }
                                    return debugLog1;
                                }
                            }
                            else
                            {
                                DynamicMetaObject dynamicMetaObject2 = errorSuggestion;
                                DynamicMetaObject dynamicMetaObject3 = dynamicMetaObject2;
                                if (dynamicMetaObject2 == null)
                                {
                                    dynamicMetaObject3 = target.ThrowRuntimeError(indexes, BindingRestrictions.Empty, "NullArray", ParserStrings.NullArray, new Expression[0]);
                                }
                                return dynamicMetaObject3.WriteToDebugLog(this);
                            }
                        }
                    }
                    return this.DeferForPSObject(indexes.Prepend<DynamicMetaObject>(target).Append<DynamicMetaObject>(value).ToArray<DynamicMetaObject>()).WriteToDebugLog(this);
                }
            }
            return base.Defer(indexes.Prepend<DynamicMetaObject>(target).Append<DynamicMetaObject>(value).ToArray<DynamicMetaObject>()).WriteToDebugLog(this);
        }

        public static PSSetIndexBinder Get(int argCount, PSMethodInvocationConstraints constraints = null)
        {
            PSSetIndexBinder pSSetIndexBinder = null;
            PSSetIndexBinder pSSetIndexBinder1;
            lock (PSSetIndexBinder._binderCache)
            {
                Tuple<CallInfo, PSMethodInvocationConstraints> tuple = Tuple.Create<CallInfo, PSMethodInvocationConstraints>(new CallInfo(argCount, new string[0]), constraints);
                if (!PSSetIndexBinder._binderCache.TryGetValue(tuple, out pSSetIndexBinder))
                {
                    pSSetIndexBinder = new PSSetIndexBinder(tuple);
                    PSSetIndexBinder._binderCache.Add(tuple, pSSetIndexBinder);
                }
                pSSetIndexBinder1 = pSSetIndexBinder;
            }
            return pSSetIndexBinder1;
        }

        private DynamicMetaObject IndexWithNegativeChecks(DynamicMetaObject target, DynamicMetaObject index, DynamicMetaObject value, PropertyInfo lengthProperty, Func<Expression, Expression, Expression, Expression> generateIndexOperation)
        {
            DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
            dynamicMetaObjectArray[0] = index;
            BindingRestrictions bindingRestriction = target.CombineRestrictions(dynamicMetaObjectArray).Merge(value.Restrictions);
            bindingRestriction = bindingRestriction.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, target.LimitType, this._version));
            ParameterExpression parameterExpression = Expression.Parameter(target.LimitType, "target");
            ParameterExpression parameterExpression1 = Expression.Parameter(typeof(int), "len");
            Expression expression = value.Expression;
            ParameterExpression parameterExpression2 = Expression.Parameter(expression.Type, "value");
            ParameterExpression parameterExpression3 = Expression.Parameter(typeof(int), "index");
            ParameterExpression[] parameterExpressionArray = new ParameterExpression[4];
            parameterExpressionArray[0] = parameterExpression;
            parameterExpressionArray[1] = parameterExpression2;
            parameterExpressionArray[2] = parameterExpression1;
            parameterExpressionArray[3] = parameterExpression3;
            Expression[] expressionArray = new Expression[7];
            expressionArray[0] = Expression.Assign(parameterExpression, target.Expression.Cast(target.LimitType));
            expressionArray[1] = Expression.Assign(parameterExpression2, expression);
            expressionArray[2] = Expression.Assign(parameterExpression1, Expression.Property(parameterExpression, lengthProperty));
            expressionArray[3] = Expression.Assign(parameterExpression3, index.Expression);
            expressionArray[4] = Expression.IfThen(Expression.LessThan(parameterExpression3, ExpressionCache.Constant(0)), Expression.Assign(parameterExpression3, Expression.Add(parameterExpression3, parameterExpression1)));
            expressionArray[5] = generateIndexOperation(parameterExpression, parameterExpression3, parameterExpression2);
            expressionArray[6] = parameterExpression2.Cast(typeof(object));
            return new DynamicMetaObject(Expression.Block(parameterExpressionArray, expressionArray), bindingRestriction);
        }

        internal static void InvalidateCache()
        {
            lock (PSSetIndexBinder._binderCache)
            {
                foreach (PSSetIndexBinder value in PSSetIndexBinder._binderCache.Values)
                {
                    PSSetIndexBinder pSSetIndexBinder = value;
                    pSSetIndexBinder._version = pSSetIndexBinder._version + 1;
                }
            }
        }

        private DynamicMetaObject InvokeIndexer(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion, string methodName)
        {
            DynamicMetaObject dynamicMetaObject;
            Func<Expression, Expression, Expression, Expression> func = null;
            MethodInfo methodInfo = PSInvokeMemberBinder.FindBestMethod(target, indexes.Append<DynamicMetaObject>(value), string.Concat("set_", methodName), false, this._constraints);
            if (methodInfo != null)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                if ((int)parameters.Length == (int)indexes.Length + 1)
                {
                    Expression[] expressionArray = new Expression[(int)parameters.Length];
                    int num = 0;
                    while (num < (int)parameters.Length)
                    {
                        Type parameterType = parameters[num].ParameterType;
                        Expression[] expressionArray1 = expressionArray;
                        int num1 = num;
                        if (num == (int)parameters.Length - 1)
                        {
                            dynamicMetaObject = value;
                        }
                        else
                        {
                            dynamicMetaObject = indexes[num];
                        }
                        expressionArray1[num1] = PSGetIndexBinder.ConvertIndex(dynamicMetaObject, parameterType);
                        if (expressionArray[num] != null)
                        {
                            num++;
                        }
                        else
                        {
                            DynamicMetaObject dynamicMetaObject1 = errorSuggestion;
                            DynamicMetaObject dynamicMetaObject2 = dynamicMetaObject1;
                            if (dynamicMetaObject1 == null)
                            {
                                dynamicMetaObject2 = PSConvertBinder.ThrowNoConversion(target, parameterType, this, this._version, indexes.Append<DynamicMetaObject>(value).ToArray<DynamicMetaObject>());
                            }
                            return dynamicMetaObject2;
                        }
                    }
                    if ((int)parameters.Length == 2 && parameters[0].ParameterType.Equals(typeof(int)) && target.Value as IDictionary == null)
                    {
                        PropertyInfo property = target.LimitType.GetProperty("Length");
                        PropertyInfo propertyInfo = property;
                        if (property == null)
                        {
                            propertyInfo = target.LimitType.GetProperty("Count");
                        }
                        PropertyInfo propertyInfo1 = propertyInfo;
                        if (propertyInfo1 != null)
                        {
                            PSSetIndexBinder pSSetIndexBinder = this;
                            DynamicMetaObject dynamicMetaObject3 = new DynamicMetaObject(target.Expression.Cast(target.LimitType), target.PSGetTypeRestriction());
                            DynamicMetaObject dynamicMetaObject4 = new DynamicMetaObject(expressionArray[0], indexes[0].PSGetTypeRestriction());
                            DynamicMetaObject dynamicMetaObject5 = new DynamicMetaObject(expressionArray[1], value.PSGetTypeRestriction());
                            PropertyInfo propertyInfo2 = propertyInfo1;
                            if (func == null)
                            {
                                func = (Expression t, Expression i, Expression v) => Expression.Call(t, methodInfo, i, v);
                            }
                            return pSSetIndexBinder.IndexWithNegativeChecks(dynamicMetaObject3, dynamicMetaObject4, dynamicMetaObject5, propertyInfo2, func);
                        }
                    }
                    BindingRestrictions bindingRestriction = target.CombineRestrictions(indexes).Merge(value.PSGetTypeRestriction());
                    bindingRestriction = bindingRestriction.Merge(BinderUtils.GetVersionCheck(this, this._version));
                    bindingRestriction = bindingRestriction.Merge(BinderUtils.GetLanguageModeCheckIfHasEverUsedConstrainedLanguage());
                    Expression expression = expressionArray[(int)expressionArray.Length - 1];
                    ParameterExpression parameterExpression = Expression.Parameter(expression.Type, "value");
                    expressionArray[(int)expressionArray.Length - 1] = parameterExpression;
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    parameterExpressionArray[0] = parameterExpression;
                    Expression[] expressionArray2 = new Expression[3];
                    expressionArray2[0] = Expression.Assign(parameterExpression, expression);
                    expressionArray2[1] = Expression.Call(target.Expression.Cast(target.LimitType), methodInfo, expressionArray);
                    expressionArray2[2] = parameterExpression.Cast(typeof(object));
                    return new DynamicMetaObject(Expression.Block(parameterExpressionArray, expressionArray2), bindingRestriction);
                }
                else
                {
                    DynamicMetaObject dynamicMetaObject6 = errorSuggestion;
                    DynamicMetaObject dynamicMetaObject7 = dynamicMetaObject6;
                    if (dynamicMetaObject6 == null)
                    {
                        dynamicMetaObject7 = this.CannotIndexTarget(target, indexes, value);
                    }
                    return dynamicMetaObject7;
                }
            }
            else
            {
                DynamicMetaObject dynamicMetaObject8 = errorSuggestion;
                DynamicMetaObject dynamicMetaObject9 = dynamicMetaObject8;
                if (dynamicMetaObject8 == null)
                {
                    dynamicMetaObject9 = this.CannotIndexTarget(target, indexes, value);
                }
                return dynamicMetaObject9;
            }
        }

        private DynamicMetaObject SetIndexArray(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            Array arrays = (Array)target.Value;
            if (arrays.Rank <= 1)
            {
                if ((int)indexes.Length <= 1)
                {
                    Expression expression = PSGetIndexBinder.ConvertIndex(indexes[0], typeof(int));
                    if (expression != null)
                    {
                        Type elementType = target.LimitType.GetElementType();
                        Expression expression1 = PSGetIndexBinder.ConvertIndex(value, elementType);
                        if (expression1 != null)
                        {
                            PSSetIndexBinder pSSetIndexBinder = this;
                            DynamicMetaObject dynamicMetaObject = new DynamicMetaObject(target.Expression.Cast(target.LimitType), target.PSGetTypeRestriction());
                            DynamicMetaObject dynamicMetaObject1 = new DynamicMetaObject(expression, indexes[0].PSGetTypeRestriction());
                            DynamicMetaObject dynamicMetaObject2 = new DynamicMetaObject(expression1, value.PSGetTypeRestriction());
                            PropertyInfo property = target.LimitType.GetProperty("Length");
                            return pSSetIndexBinder.IndexWithNegativeChecks(dynamicMetaObject, dynamicMetaObject1, dynamicMetaObject2, property, (Expression t, Expression i, Expression v) =>
                            {
                                Expression[] expressionArray = new Expression[1];
                                expressionArray[0] = i;
                                return Expression.Assign(Expression.ArrayAccess(t, expressionArray), v);
                            }
                            );
                        }
                        else
                        {
                            DynamicMetaObject dynamicMetaObject3 = errorSuggestion;
                            DynamicMetaObject dynamicMetaObject4 = dynamicMetaObject3;
                            if (dynamicMetaObject3 == null)
                            {
                                dynamicMetaObject4 = PSConvertBinder.ThrowNoConversion(value, elementType, this, this._version, indexes.Prepend<DynamicMetaObject>(target).OfType<DynamicMetaObject>().ToArray<DynamicMetaObject>());
                            }
                            return dynamicMetaObject4;
                        }
                    }
                    else
                    {
                        DynamicMetaObject dynamicMetaObject5 = errorSuggestion;
                        DynamicMetaObject dynamicMetaObject6 = dynamicMetaObject5;
                        if (dynamicMetaObject5 == null)
                        {
                            DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[2];
                            dynamicMetaObjectArray[0] = target;
                            dynamicMetaObjectArray[1] = value;
                            dynamicMetaObject6 = PSConvertBinder.ThrowNoConversion(indexes[0], typeof(int), this, this._version, dynamicMetaObjectArray);
                        }
                        return dynamicMetaObject6;
                    }
                }
                else
                {
                    DynamicMetaObject dynamicMetaObject7 = errorSuggestion;
                    DynamicMetaObject dynamicMetaObject8 = dynamicMetaObject7;
                    if (dynamicMetaObject7 == null)
                    {
                        DynamicMetaObject dynamicMetaObject9 = target;
                        DynamicMetaObject[] dynamicMetaObjectArray1 = indexes;
                        BindingRestrictions bindingRestriction = value.PSGetTypeRestriction();
                        string str = "ArraySliceAssignmentFailed";
                        string arraySliceAssignmentFailed = ParserStrings.ArraySliceAssignmentFailed;
                        Expression[] expressionArray1 = new Expression[1];
                        Expression[] expressionArray2 = expressionArray1;
                        int num = 0;
                        MethodInfo arrayOpsIndexStringMessage = CachedReflectionInfo.ArrayOps_IndexStringMessage;
                        Type type = typeof(object);
                        DynamicMetaObject[] dynamicMetaObjectArray2 = indexes;
                        expressionArray2[num] = Expression.Call(arrayOpsIndexStringMessage, Expression.NewArrayInit(type, dynamicMetaObjectArray2.Select<DynamicMetaObject, Expression>((DynamicMetaObject i) => i.Expression.Cast(typeof(object)))));
                        dynamicMetaObject8 = dynamicMetaObject9.ThrowRuntimeError(dynamicMetaObjectArray1, bindingRestriction, str, arraySliceAssignmentFailed, expressionArray1);
                    }
                    return dynamicMetaObject8;
                }
            }
            else
            {
                return this.SetIndexMultiDimensionArray(target, indexes, value, errorSuggestion);
            }
        }

        private DynamicMetaObject SetIndexMultiDimensionArray(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            Type elementType = target.LimitType.GetElementType();
            Expression expression = PSGetIndexBinder.ConvertIndex(value, elementType);
            if (expression != null)
            {
                if ((int)indexes.Length != 1)
                {
                    Array arrays = (Array)target.Value;
                    if ((int)indexes.Length == arrays.Rank)
                    {
                        Expression[] expressionArray = new Expression[(int)indexes.Length];
                        int num = 0;
                        while (num < (int)indexes.Length)
                        {
                            expressionArray[num] = PSGetIndexBinder.ConvertIndex(indexes[num], typeof(int));
                            if (expressionArray[num] != null)
                            {
                                num++;
                            }
                            else
                            {
                                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                                dynamicMetaObjectArray[0] = indexes[num];
                                return PSConvertBinder.ThrowNoConversion(indexes[num], typeof(int), this, this._version, indexes.Except<DynamicMetaObject>(dynamicMetaObjectArray).Append<DynamicMetaObject>(target).Append<DynamicMetaObject>(value).ToArray<DynamicMetaObject>());
                            }
                        }
                        return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.ArrayOps_SetMDArrayValue, target.Expression.Cast(typeof(Array)), Expression.NewArrayInit(typeof(int), expressionArray), expression.Cast(typeof(object))), target.CombineRestrictions(indexes).Merge(value.PSGetTypeRestriction()));
                    }
                    else
                    {
                        DynamicMetaObject dynamicMetaObject = errorSuggestion;
                        DynamicMetaObject dynamicMetaObject1 = dynamicMetaObject;
                        if (dynamicMetaObject == null)
                        {
                            DynamicMetaObject dynamicMetaObject2 = target;
                            DynamicMetaObject[] dynamicMetaObjectArray1 = indexes;
                            BindingRestrictions bindingRestriction = value.PSGetTypeRestriction();
                            string str = "NeedMultidimensionalIndex";
                            string needMultidimensionalIndex = ParserStrings.NeedMultidimensionalIndex;
                            Expression[] expressionArray1 = new Expression[2];
                            expressionArray1[0] = ExpressionCache.Constant(arrays.Rank);
                            Expression[] expressionArray2 = expressionArray1;
                            int num1 = 1;
                            MethodInfo arrayOpsIndexStringMessage = CachedReflectionInfo.ArrayOps_IndexStringMessage;
                            Type type = typeof(object);
                            DynamicMetaObject[] dynamicMetaObjectArray2 = indexes;
                            expressionArray2[num1] = Expression.Call(arrayOpsIndexStringMessage, Expression.NewArrayInit(type, dynamicMetaObjectArray2.Select<DynamicMetaObject, Expression>((DynamicMetaObject i) => i.Expression.Cast(typeof(object)))));
                            dynamicMetaObject1 = dynamicMetaObject2.ThrowRuntimeError(dynamicMetaObjectArray1, bindingRestriction, str, needMultidimensionalIndex, expressionArray1);
                        }
                        return dynamicMetaObject1;
                    }
                }
                else
                {
                    Expression expression1 = PSGetIndexBinder.ConvertIndex(indexes[0], typeof(int[]));
                    if (expression1 != null)
                    {
                        return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.ArrayOps_SetMDArrayValue, target.Expression.Cast(typeof(Array)), expression1, expression.Cast(typeof(object))), target.CombineRestrictions(indexes).Merge(value.PSGetTypeRestriction()));
                    }
                    else
                    {
                        DynamicMetaObject dynamicMetaObject3 = errorSuggestion;
                        DynamicMetaObject dynamicMetaObject4 = dynamicMetaObject3;
                        if (dynamicMetaObject3 == null)
                        {
                            DynamicMetaObject[] dynamicMetaObjectArray3 = new DynamicMetaObject[2];
                            dynamicMetaObjectArray3[0] = target;
                            dynamicMetaObjectArray3[1] = value;
                            dynamicMetaObject4 = PSConvertBinder.ThrowNoConversion(indexes[0], typeof(int[]), this, this._version, dynamicMetaObjectArray3);
                        }
                        return dynamicMetaObject4;
                    }
                }
            }
            else
            {
                DynamicMetaObject dynamicMetaObject5 = errorSuggestion;
                DynamicMetaObject dynamicMetaObject6 = dynamicMetaObject5;
                if (dynamicMetaObject5 == null)
                {
                    dynamicMetaObject6 = PSConvertBinder.ThrowNoConversion(value, elementType, this, this._version, indexes.Prepend<DynamicMetaObject>(target).ToArray<DynamicMetaObject>());
                }
                return dynamicMetaObject6;
            }
        }

        public override string ToString()
        {
            object obj;
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            string str = "PSSetIndexBinder indexCnt={0}{1} ver:{2}";
            object[] argumentCount = new object[3];
            argumentCount[0] = base.CallInfo.ArgumentCount;
            object[] objArray = argumentCount;
            int num = 1;
            if (this._constraints == null)
            {
                obj = "";
            }
            else
            {
                obj = string.Concat(" constraints: ", this._constraints.ToString());
            }
            objArray[num] = obj;
            argumentCount[2] = this._version;
            return string.Format(invariantCulture, str, argumentCount);
        }
    }
}