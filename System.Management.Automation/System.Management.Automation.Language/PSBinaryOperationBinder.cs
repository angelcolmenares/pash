using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Reflection;
using System.Threading;

namespace System.Management.Automation.Language
{
    internal class PSBinaryOperationBinder : BinaryOperationBinder
    {
        private readonly static Dictionary<Tuple<ExpressionType, bool, bool>, PSBinaryOperationBinder> _binderCache;

        private readonly bool _ignoreCase;

        private readonly bool _scalarCompare;

        internal int _version;

        private Func<object, object, bool> _compareDelegate;

        static PSBinaryOperationBinder()
        {
            PSBinaryOperationBinder._binderCache = new Dictionary<Tuple<ExpressionType, bool, bool>, PSBinaryOperationBinder>();
        }

        private PSBinaryOperationBinder(ExpressionType operation, bool ignoreCase, bool scalarCompare)
            : base(operation)
        {
            this._ignoreCase = ignoreCase;
            this._scalarCompare = scalarCompare;
            this._version = 0;
        }

        private DynamicMetaObject BinaryAdd(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            Expression expression;
            if (target.Value != null)
            {
                if (target.LimitType.IsNumericOrPrimitive() && !target.LimitType.Equals(typeof(char)))
                {
                    DynamicMetaObject argAsNumericOrPrimitive = PSBinaryOperationBinder.GetArgAsNumericOrPrimitive(arg, target.LimitType);
                    if (argAsNumericOrPrimitive == null)
                    {
                        if (arg.LimitType.Equals(typeof(string)))
                        {
                            return this.BinaryNumericStringOp(target, arg);
                        }
                    }
                    else
                    {
                        return this.BinaryNumericOp("Add", target, argAsNumericOrPrimitive);
                    }
                }
                Expression expression1 = null;
                if (target.LimitType != typeof(string))
                {
                    if (target.LimitType == typeof(char))
                    {
                        Expression[] expressionArray = new Expression[2];
                        expressionArray[0] = target.Expression.Cast(typeof(char));
                        expressionArray[1] = ExpressionCache.Constant(1);
                        expression1 = Expression.New(CachedReflectionInfo.String_ctor_char_int, expressionArray);
                    }
                }
                else
                {
                    expression1 = target.Expression.Cast(typeof(string));
                }
                if (expression1 == null)
                {
                    DynamicMetaObject dynamicMetaObject = PSEnumerableBinder.IsEnumerable(target);
                    if (dynamicMetaObject == null)
                    {
                        if (target.Value as IDictionary == null)
                        {
                            return PSBinaryOperationBinder.CallImplicitOp("op_Addition", target, arg, "+", errorSuggestion);
                        }
                        else
                        {
                            if (arg.Value as IDictionary == null)
                            {
                                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                                dynamicMetaObjectArray[0] = arg;
                                return target.ThrowRuntimeError(dynamicMetaObjectArray, BindingRestrictions.Empty, "AddHashTableToNonHashTable", ParserStrings.AddHashTableToNonHashTable, new Expression[0]);
                            }
                            else
                            {
                                DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                                dynamicMetaObjectArray1[0] = arg;
                                return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.HashtableOps_Add, target.Expression.Cast(typeof(IDictionary)), arg.Expression.Cast(typeof(IDictionary))), target.CombineRestrictions(dynamicMetaObjectArray1));
                            }
                        }
                    }
                    else
                    {
                        DynamicMetaObject dynamicMetaObject1 = PSEnumerableBinder.IsEnumerable(arg);
                        if (dynamicMetaObject1 == null)
                        {
                            expression = Expression.Call(CachedReflectionInfo.EnumerableOps_AddObject, ExpressionCache.GetExecutionContextFromTLS, dynamicMetaObject.Expression.Cast(typeof(IEnumerator)), arg.Expression.Cast(typeof(object)));
                        }
                        else
                        {
                            expression = Expression.Call(CachedReflectionInfo.EnumerableOps_AddEnumerable, ExpressionCache.GetExecutionContextFromTLS, dynamicMetaObject.Expression.Cast(typeof(IEnumerator)), dynamicMetaObject1.Expression.Cast(typeof(IEnumerator)));
                        }
                        DynamicMetaObject[] dynamicMetaObjectArray2 = new DynamicMetaObject[1];
                        dynamicMetaObjectArray2[0] = arg;
                        return new DynamicMetaObject(expression, target.CombineRestrictions(dynamicMetaObjectArray2));
                    }
                }
                else
                {
                    DynamicMetaObject[] dynamicMetaObjectArray3 = new DynamicMetaObject[1];
                    dynamicMetaObjectArray3[0] = arg;
                    return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.String_Concat_String, expression1, PSToStringBinder.InvokeToString(ExpressionCache.GetExecutionContextFromTLS, arg.Expression)), target.CombineRestrictions(dynamicMetaObjectArray3));
                }
            }
            else
            {
                DynamicMetaObject[] dynamicMetaObjectArray4 = new DynamicMetaObject[1];
                dynamicMetaObjectArray4[0] = arg;
                return new DynamicMetaObject(arg.Expression.Cast(typeof(object)), target.CombineRestrictions(dynamicMetaObjectArray4));
            }
        }

        private DynamicMetaObject BinaryBitwiseAnd(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return this.BinaryBitwiseOp(target, arg, errorSuggestion, new Func<Expression, Expression, Expression>(Expression.And), "op_BitwiseAnd", "-band", "BAnd");
        }

        private DynamicMetaObject BinaryBitwiseOp(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion, Func<Expression, Expression, Expression> exprGenerator, string implicitMethodName, string errorOperatorName, string methodName)
        {
            Type bitwiseOpType;
            Type type;
            Type type1;
            DynamicMetaObject argAsNumericOrPrimitive;
            DynamicMetaObject dynamicMetaObject;
            Type enumUnderlyingType;
            Type limitType;
            TypeCode typeCode;
            TypeCode typeCode1;
            if (target.Value != null || arg.Value != null)
            {
                if (target.LimitType.IsEnum)
                {
                    enumUnderlyingType = target.LimitType.GetEnumUnderlyingType();
                }
                else
                {
                    enumUnderlyingType = target.LimitType;
                }
                Type type2 = enumUnderlyingType;
                if (arg.LimitType.IsEnum)
                {
                    limitType = arg.LimitType.GetEnumUnderlyingType();
                }
                else
                {
                    limitType = arg.LimitType;
                }
                Type type3 = limitType;
                if (type2.IsNumericOrPrimitive() || type3.IsNumericOrPrimitive())
                {
                    TypeCode typeCode2 = LanguagePrimitives.GetTypeCode(type2);
                    TypeCode typeCode3 = LanguagePrimitives.GetTypeCode(type3);
                    if (typeCode2 >= typeCode3)
                    {
                        typeCode = typeCode2;
                    }
                    else
                    {
                        typeCode = typeCode3;
                    }
                    TypeCode typeCode4 = typeCode;
                    if (type2.IsNumericOrPrimitive())
                    {
                        if (type3.IsNumericOrPrimitive())
                        {
                            argAsNumericOrPrimitive = target;
                            dynamicMetaObject = arg;
                        }
                        else
                        {
                            bitwiseOpType = PSBinaryOperationBinder.GetBitwiseOpType(typeCode2);
                            argAsNumericOrPrimitive = target;
                            dynamicMetaObject = PSBinaryOperationBinder.GetArgAsNumericOrPrimitive(arg, bitwiseOpType);
                        }
                    }
                    else
                    {
                        bitwiseOpType = PSBinaryOperationBinder.GetBitwiseOpType(typeCode3);
                        argAsNumericOrPrimitive = PSBinaryOperationBinder.GetArgAsNumericOrPrimitive(target, bitwiseOpType);
                        dynamicMetaObject = arg;
                    }
                    if (typeCode4 != TypeCode.Decimal)
                    {
                        if (typeCode4 == TypeCode.Double || typeCode4 == TypeCode.Single)
                        {
                            type = typeof(DoubleOps);
                            type1 = typeof(double);
                            DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                            dynamicMetaObjectArray[0] = dynamicMetaObject;
                            return new DynamicMetaObject(Expression.Call(type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic), argAsNumericOrPrimitive.Expression.Cast(argAsNumericOrPrimitive.LimitType).Convert(type1), dynamicMetaObject.Expression.Cast(dynamicMetaObject.LimitType).Convert(type1)), argAsNumericOrPrimitive.CombineRestrictions(dynamicMetaObjectArray));
                        }
                        else
                        {
                            if (typeCode2 >= typeCode3)
                            {
                                typeCode1 = typeCode2;
                            }
                            else
                            {
                                typeCode1 = typeCode3;
                            }
                            bitwiseOpType = PSBinaryOperationBinder.GetBitwiseOpType(typeCode1);
                            if (argAsNumericOrPrimitive != null && dynamicMetaObject != null)
                            {
                                DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                                dynamicMetaObjectArray1[0] = dynamicMetaObject;
                                return new DynamicMetaObject(exprGenerator(argAsNumericOrPrimitive.Expression.Cast(target.LimitType).Cast(bitwiseOpType), dynamicMetaObject.Expression.Cast(dynamicMetaObject.LimitType).Cast(bitwiseOpType)).Cast(typeof(object)), argAsNumericOrPrimitive.CombineRestrictions(dynamicMetaObjectArray1));
                            }
                        }
                    }
                    else
                    {
                        type = typeof(DecimalOps);
                        type1 = typeof(decimal);
                        DynamicMetaObject[] dynamicMetaObjectArray2 = new DynamicMetaObject[1];
                        dynamicMetaObjectArray2[0] = dynamicMetaObject;
                        return new DynamicMetaObject(Expression.Call(type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic), argAsNumericOrPrimitive.Expression.Cast(argAsNumericOrPrimitive.LimitType).Convert(type1), dynamicMetaObject.Expression.Cast(dynamicMetaObject.LimitType).Convert(type1)), argAsNumericOrPrimitive.CombineRestrictions(dynamicMetaObjectArray2));
                    }
                }
                if (target.LimitType.Equals(typeof(string)) || arg.LimitType.Equals(typeof(string)))
                {
                    return this.BinaryNumericStringOp(target, arg);
                }
                else
                {
                    return PSBinaryOperationBinder.CallImplicitOp(implicitMethodName, target, arg, errorOperatorName, errorSuggestion);
                }
            }
            else
            {
                DynamicMetaObject[] dynamicMetaObjectArray3 = new DynamicMetaObject[1];
                dynamicMetaObjectArray3[0] = arg;
                return new DynamicMetaObject(ExpressionCache.Constant(0).Cast(typeof(object)), target.CombineRestrictions(dynamicMetaObjectArray3));
            }
        }

        private DynamicMetaObject BinaryBitwiseOr(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return this.BinaryBitwiseOp(target, arg, errorSuggestion, new Func<Expression, Expression, Expression>(Expression.Or), "op_BitwiseOr", "-bor", "BOr");
        }

        private DynamicMetaObject BinaryBitwiseXor(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return this.BinaryBitwiseOp(target, arg, errorSuggestion, new Func<Expression, Expression, Expression>(Expression.ExclusiveOr), "op_ExclusiveOr", "-bxor", "BXor");
        }

        private DynamicMetaObject BinaryComparision(DynamicMetaObject target, DynamicMetaObject arg, Func<Expression, Expression> toResult)
        {
            bool flag = false;
            Expression expression;
            Expression boxedTrue;
            Expression expression1;
            Expression stringComparisonInvariantCultureIgnoreCase;
            if (!target.LimitType.Equals(typeof(string)))
            {
                Type limitType = target.LimitType;
                LanguagePrimitives.ConversionData conversionDatum = LanguagePrimitives.FigureConversion(arg.Value, limitType, out flag);
                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                dynamicMetaObjectArray[0] = arg;
                BindingRestrictions bindingRestriction = target.CombineRestrictions(dynamicMetaObjectArray);
                bindingRestriction = bindingRestriction.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, limitType, this._version));
                if (conversionDatum.Rank != ConversionRank.Identity)
                {
                    ParameterExpression parameterExpression = Expression.Parameter(typeof(InvalidCastException));
                    CatchBlock[] catchBlockArray = new CatchBlock[1];
                    Expression[] expressionArray = new Expression[3];
                    expressionArray[0] = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), target.Expression, ExpressionCache.GetExecutionContextFromTLS);
                    expressionArray[1] = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), arg.Expression, ExpressionCache.GetExecutionContextFromTLS);
                    expressionArray[2] = Expression.Property(parameterExpression, CachedReflectionInfo.Exception_Message);
                    catchBlockArray[0] = Expression.Catch(parameterExpression, Compiler.ThrowRuntimeErrorWithInnerException("ComparisonFailure", Expression.Constant(ExtendedTypeSystem.ComparisonFailure), parameterExpression, limitType, expressionArray));
                    expression = Expression.TryCatch(PSConvertBinder.InvokeConverter(conversionDatum, arg.Expression, limitType, flag, ExpressionCache.InvariantCulture), catchBlockArray);
                }
                else
                {
                    expression = arg.Expression;
                }
                if (target.LimitType.Equals(arg.LimitType))
                {
                    Type[] interfaces = target.Value.GetType().GetInterfaces();
                    int num = 0;
                    while (num < (int)interfaces.Length)
                    {
                        Type type = interfaces[num];
                        if (!type.IsGenericType || !(type.GetGenericTypeDefinition() == typeof(IComparable<>)))
                        {
                            num++;
                        }
                        else
                        {
                            Expression[] expressionArray1 = new Expression[1];
                            expressionArray1[0] = expression.Cast(arg.LimitType);
                            DynamicMetaObject dynamicMetaObject = new DynamicMetaObject(toResult(Expression.Call(Expression.Convert(target.Expression, type), type.GetMethod("CompareTo"), expressionArray1)).Cast(typeof(object)), bindingRestriction);
                            return dynamicMetaObject;
                        }
                    }
                }
                if (target.Value as IComparable == null)
                {
                    Expression[] expression2 = new Expression[1];
                    expression2[0] = target.Expression;
                    Expression expression3 = Compiler.ThrowRuntimeError("NotIcomparable", ExtendedTypeSystem.NotIcomparable, this.ReturnType, expression2);
                    Expression[] expressionArray2 = new Expression[1];
                    expressionArray2[0] = arg.Expression.Cast(typeof(object));
                    MethodCallExpression methodCallExpression = Expression.Call(target.Expression.Cast(typeof(object)), CachedReflectionInfo.Object_Equals, expressionArray2);
                    if (base.Operation == ExpressionType.GreaterThanOrEqual || base.Operation == ExpressionType.LessThanOrEqual)
                    {
                        boxedTrue = ExpressionCache.BoxedTrue;
                    }
                    else
                    {
                        boxedTrue = ExpressionCache.BoxedFalse;
                    }
                    return new DynamicMetaObject(Expression.Condition(methodCallExpression, boxedTrue, expression3), bindingRestriction);
                }
                else
                {
                    Expression[] expressionArray3 = new Expression[1];
                    expressionArray3[0] = expression.Cast(typeof(object));
                    return new DynamicMetaObject(toResult(Expression.Call(target.Expression.Cast(typeof(IComparable)), CachedReflectionInfo.IComparable_CompareTo, expressionArray3)).Cast(typeof(object)), bindingRestriction);
                }
            }
            else
            {
                Expression expression4 = target.Expression.Cast(typeof(string));
                if (!arg.LimitType.Equals(typeof(string)))
                {
                    expression1 = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), arg.Expression, ExpressionCache.GetExecutionContextFromTLS);
                }
                else
                {
                    expression1 = arg.Expression.Cast(typeof(string));
                }
                Expression expression5 = expression1;
                MethodInfo stringCompare = CachedReflectionInfo.String_Compare;
                Expression expression6 = expression4;
                Expression expression7 = expression5;
                if (this._ignoreCase)
                {
                    stringComparisonInvariantCultureIgnoreCase = ExpressionCache.StringComparisonInvariantCultureIgnoreCase;
                }
                else
                {
                    stringComparisonInvariantCultureIgnoreCase = ExpressionCache.StringComparisonInvariantCulture;
                }
                MethodCallExpression methodCallExpression1 = Expression.Call(stringCompare, expression6, expression7, stringComparisonInvariantCultureIgnoreCase);
                DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                dynamicMetaObjectArray1[0] = arg;
                return new DynamicMetaObject(toResult(methodCallExpression1).Cast(typeof(object)), target.CombineRestrictions(dynamicMetaObjectArray1));
            }
        }

        private DynamicMetaObject BinaryComparisonCommon(DynamicMetaObject targetAsEnumerator, DynamicMetaObject target, DynamicMetaObject arg)
        {
            if (targetAsEnumerator == null || this._scalarCompare)
            {
                if (target.LimitType.IsNumeric())
                {
                    DynamicMetaObject argAsNumericOrPrimitive = PSBinaryOperationBinder.GetArgAsNumericOrPrimitive(arg, target.LimitType);
                    if (argAsNumericOrPrimitive == null)
                    {
                        if (arg.LimitType.Equals(typeof(string)))
                        {
                            return this.BinaryNumericStringOp(target, arg);
                        }
                    }
                    else
                    {
                        string str = null;
                        ExpressionType operation = base.Operation;
                        switch (operation)
                        {
                            case ExpressionType.Equal:
                                {
                                    str = "CompareEq";
                                    return this.BinaryNumericOp(str, target, argAsNumericOrPrimitive);
                                }
                            case ExpressionType.ExclusiveOr:
                            case ExpressionType.Invoke:
                            case ExpressionType.Lambda:
                            case ExpressionType.LeftShift:
                                {
                                    return this.BinaryNumericOp(str, target, argAsNumericOrPrimitive);
                                }
                            case ExpressionType.GreaterThan:
                                {
                                    str = "CompareGt";
                                    return this.BinaryNumericOp(str, target, argAsNumericOrPrimitive);
                                }
                            case ExpressionType.GreaterThanOrEqual:
                                {
                                    str = "CompareGe";
                                    return this.BinaryNumericOp(str, target, argAsNumericOrPrimitive);
                                }
                            case ExpressionType.LessThan:
                                {
                                    str = "CompareLt";
                                    return this.BinaryNumericOp(str, target, argAsNumericOrPrimitive);
                                }
                            case ExpressionType.LessThanOrEqual:
                                {
                                    str = "CompareLe";
                                    return this.BinaryNumericOp(str, target, argAsNumericOrPrimitive);
                                }
                            default:
                                {
                                    if (operation == ExpressionType.NotEqual)
                                    {
                                        str = "CompareNe";
                                        return this.BinaryNumericOp(str, target, argAsNumericOrPrimitive);
                                    }
                                    else
                                    {
                                        return this.BinaryNumericOp(str, target, argAsNumericOrPrimitive);
                                    }
                                }
                        }
                    }
                }
                return null;
            }
            else
            {
                return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.EnumerableOps_Compare, targetAsEnumerator.Expression, arg.Expression.Cast(typeof(object)), Expression.Constant(this.GetScalarCompareDelegate())), targetAsEnumerator.Restrictions.Merge(arg.PSGetTypeRestriction()));
            }
        }

        private DynamicMetaObject BinaryDivide(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return this.BinarySubDivOrRem(target, arg, errorSuggestion, "Divide", "op_Division", "/");
        }

        private DynamicMetaObject BinaryEqualityComparison(DynamicMetaObject target, DynamicMetaObject arg)
        {
            bool flag = false;
            Func<Expression, Expression> func;
            MethodInfo charOpsCompareIeq;
            Expression expression;
            if (base.Operation == ExpressionType.NotEqual)
            {
                func = new Func<Expression, Expression>(Expression.Not);
            }
            else
            {
                func = (Expression e) => e;
            }
            Func<Expression, Expression> func1 = func;
            if (!target.LimitType.Equals(typeof(string)))
            {
                if (target.LimitType.Equals(typeof(char)) && this._ignoreCase)
                {
                    Expression expression1 = null;
                    BindingRestrictions bindingRestriction = null;
                    if (!arg.LimitType.Equals(typeof(char)))
                    {
                        string value = arg.Value as string;
                        if (value != null && value.Length == 1)
                        {
                            Expression[] expressionArray = new Expression[1];
                            expressionArray[0] = ExpressionCache.Constant(0);
                            expression1 = Expression.Call(arg.Expression.Cast(typeof(string)), CachedReflectionInfo.String_get_Chars, expressionArray);
                            bindingRestriction = arg.PSGetTypeRestriction().Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(Expression.Property(arg.Expression.Cast(typeof(string)), CachedReflectionInfo.String_Length), ExpressionCache.Constant(1))));
                        }
                    }
                    else
                    {
                        expression1 = arg.Expression;
                        bindingRestriction = arg.PSGetTypeRestriction();
                    }
                    if (expression1 != null)
                    {
                        if (base.Operation == ExpressionType.Equal)
                        {
                            charOpsCompareIeq = CachedReflectionInfo.CharOps_CompareIeq;
                        }
                        else
                        {
                            charOpsCompareIeq = CachedReflectionInfo.CharOps_CompareIne;
                        }
                        return new DynamicMetaObject(Expression.Call(charOpsCompareIeq, target.Expression.Cast(typeof(char)), expression1.Cast(typeof(char))), target.PSGetTypeRestriction().Merge(bindingRestriction));
                    }
                }
                Expression[] expressionArray1 = new Expression[1];
                expressionArray1[0] = arg.Expression.Cast(typeof(object));
                Expression expression2 = Expression.Call(target.Expression.Cast(typeof(object)), CachedReflectionInfo.Object_Equals, expressionArray1);
                Type limitType = target.LimitType;
                LanguagePrimitives.ConversionData conversionDatum = LanguagePrimitives.FigureConversion(arg.Value, limitType, out flag);
                if (conversionDatum.Rank != ConversionRank.Identity)
                {
                    DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                    dynamicMetaObjectArray[0] = arg;
                    BindingRestrictions bindingRestriction1 = target.CombineRestrictions(dynamicMetaObjectArray);
                    bindingRestriction1 = bindingRestriction1.Merge(BinderUtils.GetOptionalVersionAndLanguageCheckForType(this, limitType, this._version));
                    ParameterExpression parameterExpression = Expression.Parameter(typeof(bool));
                    Expression[] expressionArray2 = new Expression[1];
                    expressionArray2[0] = PSConvertBinder.InvokeConverter(conversionDatum, arg.Expression, limitType, flag, ExpressionCache.InvariantCulture).Cast(typeof(object));
                    Expression expression3 = Expression.Call(target.Expression.Cast(typeof(object)), CachedReflectionInfo.Object_Equals, expressionArray2);
                    ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                    parameterExpressionArray[0] = parameterExpression;
                    Expression[] expressionArray3 = new Expression[3];
                    expressionArray3[0] = Expression.Assign(parameterExpression, expression2);
                    CatchBlock[] catchBlockArray = new CatchBlock[1];
                    catchBlockArray[0] = Expression.Catch(typeof(InvalidCastException), Expression.Assign(parameterExpression, ExpressionCache.Constant(false)));
                    expressionArray3[1] = Expression.IfThen(Expression.Not(parameterExpression), Expression.TryCatch(Expression.Assign(parameterExpression, expression3), catchBlockArray));
                    expressionArray3[2] = func1(parameterExpression);
                    BlockExpression blockExpression = Expression.Block(parameterExpressionArray, expressionArray3);
                    return new DynamicMetaObject(blockExpression.Cast(typeof(object)), bindingRestriction1);
                }
                else
                {
                    DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                    dynamicMetaObjectArray1[0] = arg;
                    return new DynamicMetaObject(func1(expression2).Cast(typeof(object)), target.CombineRestrictions(dynamicMetaObjectArray1));
                }
            }
            else
            {
                Expression expression4 = target.Expression.Cast(typeof(string));
                if (!arg.LimitType.Equals(typeof(string)))
                {
                    expression = Expression.Dynamic(PSToStringBinder.Get(), typeof(string), arg.Expression, ExpressionCache.GetExecutionContextFromTLS);
                }
                else
                {
                    expression = arg.Expression.Cast(typeof(string));
                }
                Expression expression5 = expression;
                DynamicMetaObject[] dynamicMetaObjectArray2 = new DynamicMetaObject[1];
                dynamicMetaObjectArray2[0] = arg;
                return new DynamicMetaObject(func1(Compiler.CallStringEquals(expression4, expression5, this._ignoreCase)).Cast(typeof(object)), target.CombineRestrictions(dynamicMetaObjectArray2));
            }
        }

        private DynamicMetaObject BinaryMultiply(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            Expression expression;
            Expression expression1;
            if (target.Value != null)
            {
                if (target.LimitType.IsNumeric())
                {
                    DynamicMetaObject argAsNumericOrPrimitive = PSBinaryOperationBinder.GetArgAsNumericOrPrimitive(arg, target.LimitType);
                    if (argAsNumericOrPrimitive == null)
                    {
                        if (arg.LimitType.Equals(typeof(string)))
                        {
                            return this.BinaryNumericStringOp(target, arg);
                        }
                    }
                    else
                    {
                        return this.BinaryNumericOp("Multiply", target, argAsNumericOrPrimitive);
                    }
                }
                if (!target.LimitType.Equals(typeof(string)))
                {
                    DynamicMetaObject dynamicMetaObject = PSEnumerableBinder.IsEnumerable(target);
                    if (dynamicMetaObject == null)
                    {
                        return PSBinaryOperationBinder.CallImplicitOp("op_Multiply", target, arg, "*", errorSuggestion);
                    }
                    else
                    {
                        if (arg.LimitType.Equals(typeof(string)))
                        {
                            expression = PSBinaryOperationBinder.ConvertStringToNumber(arg.Expression, typeof(int)).Convert(typeof(int));
                        }
                        else
                        {
                            expression = arg.CastOrConvert(typeof(int));
                        }
                        Expression expression2 = expression;
                        if (!target.LimitType.IsArray)
                        {
                            DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                            dynamicMetaObjectArray[0] = arg;
                            return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.EnumerableOps_Multiply, dynamicMetaObject.Expression, expression2), target.CombineRestrictions(dynamicMetaObjectArray));
                        }
                        else
                        {
                            Type elementType = target.LimitType.GetElementType();
                            Type[] typeArray = new Type[1];
                            typeArray[0] = elementType;
                            DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                            dynamicMetaObjectArray1[0] = arg;
                            return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.ArrayOps_Multiply.MakeGenericMethod(typeArray), target.Expression.Cast(elementType.MakeArrayType()), expression2), target.CombineRestrictions(dynamicMetaObjectArray1));
                        }
                    }
                }
                else
                {
                    if (arg.LimitType.Equals(typeof(string)))
                    {
                        expression1 = PSBinaryOperationBinder.ConvertStringToNumber(arg.Expression, typeof(int)).Convert(typeof(int));
                    }
                    else
                    {
                        expression1 = arg.CastOrConvert(typeof(int));
                    }
                    Expression expression3 = expression1;
                    DynamicMetaObject[] dynamicMetaObjectArray2 = new DynamicMetaObject[1];
                    dynamicMetaObjectArray2[0] = arg;
                    return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.StringOps_Multiply, target.Expression.Cast(typeof(string)), expression3), target.CombineRestrictions(dynamicMetaObjectArray2));
                }
            }
            else
            {
                return new DynamicMetaObject(ExpressionCache.NullConstant, target.PSGetTypeRestriction());
            }
        }

        private DynamicMetaObject BinaryNumericOp(string methodName, DynamicMetaObject target, DynamicMetaObject arg)
        {
            TypeCode typeCode;
            Expression boxedFalse;
            Type type = null;
            Type type1 = null;
            TypeCode typeCode1 = LanguagePrimitives.GetTypeCode(target.LimitType);
            TypeCode typeCode2 = LanguagePrimitives.GetTypeCode(arg.LimitType);
            if (typeCode1 >= typeCode2)
            {
                typeCode = typeCode1;
            }
            else
            {
                typeCode = typeCode2;
            }
            TypeCode typeCode3 = typeCode;
            if (typeCode3 > TypeCode.Int32)
            {
                if (typeCode3 > TypeCode.UInt32)
                {
                    if (typeCode3 > TypeCode.Int64)
                    {
                        if (typeCode3 > TypeCode.UInt64)
                        {
                            if (typeCode3 != TypeCode.Decimal)
                            {
                                type = typeof(DoubleOps);
                                type1 = typeof(double);
                            }
                            else
                            {
                                if (methodName.StartsWith("Compare", StringComparison.Ordinal))
                                {
                                    if (!LanguagePrimitives.IsFloating(typeCode1))
                                    {
                                        if (LanguagePrimitives.IsFloating(typeCode2))
                                        {
                                            DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                                            dynamicMetaObjectArray[0] = arg;
                                            return new DynamicMetaObject(Expression.Call(typeof(DecimalOps).GetMethod(string.Concat(methodName, "2"), BindingFlags.Static | BindingFlags.NonPublic), target.Expression.Cast(target.LimitType).Cast(typeof(decimal)), arg.Expression.Cast(arg.LimitType).Cast(typeof(double))), target.CombineRestrictions(dynamicMetaObjectArray));
                                        }
                                    }
                                    else
                                    {
                                        DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                                        dynamicMetaObjectArray1[0] = arg;
                                        return new DynamicMetaObject(Expression.Call(typeof(DecimalOps).GetMethod(string.Concat(methodName, "1"), BindingFlags.Static | BindingFlags.NonPublic), target.Expression.Cast(target.LimitType).Cast(typeof(double)), arg.Expression.Cast(arg.LimitType).Cast(typeof(decimal))), target.CombineRestrictions(dynamicMetaObjectArray1));
                                    }
                                }
                                type = typeof(DecimalOps);
                                type1 = typeof(decimal);
                            }
                        }
                        else
                        {
                            if (!LanguagePrimitives.IsSignedInteger(typeCode1))
                            {
                                if (LanguagePrimitives.IsSignedInteger(typeCode2))
                                {
                                    arg = PSBinaryOperationBinder.FigureSignedUnsignedInt(arg, typeCode2, out type, out type1);
                                }
                            }
                            else
                            {
                                target = PSBinaryOperationBinder.FigureSignedUnsignedInt(target, typeCode1, out type, out type1);
                            }
                            if (type == null)
                            {
                                type = typeof(ULongOps);
                                type1 = typeof(ulong);
                            }
                        }
                    }
                    else
                    {
                        type = typeof(LongOps);
                        type1 = typeof(long);
                    }
                }
                else
                {
                    if (!LanguagePrimitives.IsSignedInteger(typeCode1))
                    {
                        if (LanguagePrimitives.IsSignedInteger(typeCode2))
                        {
                            arg = PSBinaryOperationBinder.FigureSignedUnsignedInt(arg, typeCode2, out type, out type1);
                        }
                    }
                    else
                    {
                        target = PSBinaryOperationBinder.FigureSignedUnsignedInt(target, typeCode1, out type, out type1);
                    }
                    if (type == null)
                    {
                        type = typeof(UIntOps);
                        type1 = typeof(int);
                    }
                }
            }
            else
            {
                type = typeof(IntOps);
                type1 = typeof(int);
            }
            Expression expression = Expression.Call(type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic), target.Expression.Cast(target.LimitType).Cast(type1), arg.Expression.Cast(arg.LimitType).Cast(type1));
            if (base.Operation == ExpressionType.Equal || base.Operation == ExpressionType.NotEqual)
            {
                Expression expression1 = expression;
                CatchBlock[] catchBlockArray = new CatchBlock[1];
                CatchBlock[] catchBlockArray1 = catchBlockArray;
                int num = 0;
                Type type2 = typeof(InvalidCastException);
                if (base.Operation == ExpressionType.Equal)
                {
                    boxedFalse = ExpressionCache.BoxedFalse;
                }
                else
                {
                    boxedFalse = ExpressionCache.BoxedTrue;
                }
                catchBlockArray1[num] = Expression.Catch(type2, boxedFalse);
                expression = Expression.TryCatch(expression1, catchBlockArray);
            }
            DynamicMetaObject[] dynamicMetaObjectArray2 = new DynamicMetaObject[1];
            dynamicMetaObjectArray2[0] = arg;
            return new DynamicMetaObject(expression, target.CombineRestrictions(dynamicMetaObjectArray2));
        }

        private DynamicMetaObject BinaryNumericStringOp(DynamicMetaObject target, DynamicMetaObject arg)
        {
            Expression number;
            Expression boxedFalse;
            List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();
            List<Expression> expressions = new List<Expression>();
            Expression expression = target.Expression;
            if (target.LimitType.Equals(typeof(string)))
            {
                expression = PSBinaryOperationBinder.ConvertStringToNumber(target.Expression, arg.LimitType);
            }
            if (arg.LimitType.Equals(typeof(string)))
            {
                number = PSBinaryOperationBinder.ConvertStringToNumber(arg.Expression, target.LimitType);
            }
            else
            {
                number = arg.Expression;
            }
            Expression expression1 = number;
            expressions.Add(Expression.Dynamic(PSBinaryOperationBinder.Get(base.Operation, true, false), typeof(object), expression, expression1));
            Expression expression2 = Expression.Block(parameterExpressions, expressions);
            if (base.Operation == ExpressionType.Equal || base.Operation == ExpressionType.NotEqual)
            {
                Expression expression3 = expression2;
                CatchBlock[] catchBlockArray = new CatchBlock[1];
                CatchBlock[] catchBlockArray1 = catchBlockArray;
                int num = 0;
                Type type = typeof(InvalidCastException);
                if (base.Operation == ExpressionType.Equal)
                {
                    boxedFalse = ExpressionCache.BoxedFalse;
                }
                else
                {
                    boxedFalse = ExpressionCache.BoxedTrue;
                }
                catchBlockArray1[num] = Expression.Catch(type, boxedFalse);
                expression2 = Expression.TryCatch(expression3, catchBlockArray);
            }
            DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
            dynamicMetaObjectArray[0] = arg;
            return new DynamicMetaObject(expression2, target.CombineRestrictions(dynamicMetaObjectArray));
        }

        private DynamicMetaObject BinaryRemainder(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return this.BinarySubDivOrRem(target, arg, errorSuggestion, "Remainder", "op_Modulus", "%");
        }

        private DynamicMetaObject BinarySub(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return this.BinarySubDivOrRem(target, arg, errorSuggestion, "Sub", "op_Subtraction", "-");
        }

        private DynamicMetaObject BinarySubDivOrRem(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion, string numericOpMethodName, string implicitOpMethodName, string errorOperatorText)
        {
            if (target.Value == null)
            {
                target = new DynamicMetaObject(ExpressionCache.Constant(0), target.PSGetTypeRestriction(), (object)0);
            }
            if (target.LimitType.IsNumericOrPrimitive())
            {
                DynamicMetaObject argAsNumericOrPrimitive = PSBinaryOperationBinder.GetArgAsNumericOrPrimitive(arg, target.LimitType);
                if (argAsNumericOrPrimitive == null)
                {
                    if (arg.LimitType.Equals(typeof(string)))
                    {
                        return this.BinaryNumericStringOp(target, arg);
                    }
                }
                else
                {
                    return this.BinaryNumericOp(numericOpMethodName, target, argAsNumericOrPrimitive);
                }
            }
            if (!target.LimitType.Equals(typeof(string)))
            {
                return PSBinaryOperationBinder.CallImplicitOp(implicitOpMethodName, target, arg, errorOperatorText, errorSuggestion);
            }
            else
            {
                return this.BinaryNumericStringOp(target, arg);
            }
        }

        private static DynamicMetaObject CallImplicitOp(string methodName, DynamicMetaObject target, DynamicMetaObject arg, string errorOperator, DynamicMetaObject errorSuggestion)
        {
            if (errorSuggestion == null || target.Value as DynamicObject == null)
            {
                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                dynamicMetaObjectArray[0] = arg;
                return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.ParserOps_ImplicitOp, target.Expression.Cast(typeof(object)), arg.Expression.Cast(typeof(object)), Expression.Constant(methodName), ExpressionCache.NullExtent, Expression.Constant(errorOperator)), target.CombineRestrictions(dynamicMetaObjectArray));
            }
            else
            {
                return errorSuggestion;
            }
        }

        private DynamicMetaObject CompareEQ(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            Expression boxedTrue;
            if (target.Value != null)
            {
                DynamicMetaObject dynamicMetaObject = PSEnumerableBinder.IsEnumerable(target);
                if (dynamicMetaObject != null || arg.Value != null)
                {
                    DynamicMetaObject dynamicMetaObject1 = this.BinaryComparisonCommon(dynamicMetaObject, target, arg);
                    DynamicMetaObject dynamicMetaObject2 = dynamicMetaObject1;
                    if (dynamicMetaObject1 == null)
                    {
                        dynamicMetaObject2 = this.BinaryEqualityComparison(target, arg);
                    }
                    return dynamicMetaObject2;
                }
                else
                {
                    DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                    dynamicMetaObjectArray[0] = arg;
                    return new DynamicMetaObject(ExpressionCache.BoxedFalse, target.CombineRestrictions(dynamicMetaObjectArray));
                }
            }
            else
            {
                if (arg.Value == null)
                {
                    boxedTrue = ExpressionCache.BoxedTrue;
                }
                else
                {
                    boxedTrue = ExpressionCache.BoxedFalse;
                }
                DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                dynamicMetaObjectArray1[0] = arg;
                return new DynamicMetaObject(boxedTrue, target.CombineRestrictions(dynamicMetaObjectArray1));
            }
        }

        private DynamicMetaObject CompareGE(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            Expression boxedFalse;
            DynamicMetaObject dynamicMetaObject = PSEnumerableBinder.IsEnumerable(target);
            if (dynamicMetaObject != null || target.Value != null && arg.Value != null)
            {
                DynamicMetaObject dynamicMetaObject1 = this.BinaryComparisonCommon(dynamicMetaObject, target, arg);
                DynamicMetaObject dynamicMetaObject2 = dynamicMetaObject1;
                if (dynamicMetaObject1 == null)
                {
                    PSBinaryOperationBinder pSBinaryOperationBinder = this;
                    DynamicMetaObject dynamicMetaObject3 = target;
                    DynamicMetaObject dynamicMetaObject4 = arg;
                    dynamicMetaObject2 = pSBinaryOperationBinder.BinaryComparision(dynamicMetaObject3, dynamicMetaObject4, (Expression e) => Expression.GreaterThanOrEqual(e, ExpressionCache.Constant(0)));
                }
                return dynamicMetaObject2;
            }
            else
            {
                if (target.LimitType.IsNumeric())
                {
                    boxedFalse = PSBinaryOperationBinder.CompareWithZero(target, new Func<Expression, Expression, Expression>(Expression.GreaterThanOrEqual));
                }
                else
                {
                    if (arg.LimitType.IsNumeric())
                    {
                        boxedFalse = PSBinaryOperationBinder.CompareWithZero(arg, new Func<Expression, Expression, Expression>(Expression.LessThan));
                    }
                    else
                    {
                        if (arg.Value != null)
                        {
                            boxedFalse = ExpressionCache.BoxedFalse;
                        }
                        else
                        {
                            boxedFalse = ExpressionCache.BoxedTrue;
                        }
                    }
                }
                Expression expression = boxedFalse;
                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                dynamicMetaObjectArray[0] = arg;
                return new DynamicMetaObject(expression, target.CombineRestrictions(dynamicMetaObjectArray));
            }
        }

        private DynamicMetaObject CompareGT(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            Expression boxedTrue;
            DynamicMetaObject dynamicMetaObject = PSEnumerableBinder.IsEnumerable(target);
            if (dynamicMetaObject != null || target.Value != null && arg.Value != null)
            {
                DynamicMetaObject dynamicMetaObject1 = this.BinaryComparisonCommon(dynamicMetaObject, target, arg);
                DynamicMetaObject dynamicMetaObject2 = dynamicMetaObject1;
                if (dynamicMetaObject1 == null)
                {
                    PSBinaryOperationBinder pSBinaryOperationBinder = this;
                    DynamicMetaObject dynamicMetaObject3 = target;
                    DynamicMetaObject dynamicMetaObject4 = arg;
                    dynamicMetaObject2 = pSBinaryOperationBinder.BinaryComparision(dynamicMetaObject3, dynamicMetaObject4, (Expression e) => Expression.GreaterThan(e, ExpressionCache.Constant(0)));
                }
                return dynamicMetaObject2;
            }
            else
            {
                if (target.LimitType.IsNumeric())
                {
                    boxedTrue = PSBinaryOperationBinder.CompareWithZero(target, new Func<Expression, Expression, Expression>(Expression.GreaterThanOrEqual));
                }
                else
                {
                    if (arg.LimitType.IsNumeric())
                    {
                        boxedTrue = PSBinaryOperationBinder.CompareWithZero(arg, new Func<Expression, Expression, Expression>(Expression.LessThan));
                    }
                    else
                    {
                        if (target.Value != null)
                        {
                            boxedTrue = ExpressionCache.BoxedTrue;
                        }
                        else
                        {
                            boxedTrue = ExpressionCache.BoxedFalse;
                        }
                    }
                }
                Expression expression = boxedTrue;
                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                dynamicMetaObjectArray[0] = arg;
                return new DynamicMetaObject(expression, target.CombineRestrictions(dynamicMetaObjectArray));
            }
        }

        private DynamicMetaObject CompareLE(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            Expression boxedFalse;
            DynamicMetaObject dynamicMetaObject = PSEnumerableBinder.IsEnumerable(target);
            if (dynamicMetaObject != null || target.Value != null && arg.Value != null)
            {
                DynamicMetaObject dynamicMetaObject1 = this.BinaryComparisonCommon(dynamicMetaObject, target, arg);
                DynamicMetaObject dynamicMetaObject2 = dynamicMetaObject1;
                if (dynamicMetaObject1 == null)
                {
                    PSBinaryOperationBinder pSBinaryOperationBinder = this;
                    DynamicMetaObject dynamicMetaObject3 = target;
                    DynamicMetaObject dynamicMetaObject4 = arg;
                    dynamicMetaObject2 = pSBinaryOperationBinder.BinaryComparision(dynamicMetaObject3, dynamicMetaObject4, (Expression e) => Expression.LessThanOrEqual(e, ExpressionCache.Constant(0)));
                }
                return dynamicMetaObject2;
            }
            else
            {
                if (target.LimitType.IsNumeric())
                {
                    boxedFalse = PSBinaryOperationBinder.CompareWithZero(target, new Func<Expression, Expression, Expression>(Expression.LessThan));
                }
                else
                {
                    if (arg.LimitType.IsNumeric())
                    {
                        boxedFalse = PSBinaryOperationBinder.CompareWithZero(arg, new Func<Expression, Expression, Expression>(Expression.GreaterThanOrEqual));
                    }
                    else
                    {
                        if (target.Value != null)
                        {
                            boxedFalse = ExpressionCache.BoxedFalse;
                        }
                        else
                        {
                            boxedFalse = ExpressionCache.BoxedTrue;
                        }
                    }
                }
                Expression expression = boxedFalse;
                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                dynamicMetaObjectArray[0] = arg;
                return new DynamicMetaObject(expression, target.CombineRestrictions(dynamicMetaObjectArray));
            }
        }

        private DynamicMetaObject CompareLT(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            Expression boxedTrue;
            DynamicMetaObject dynamicMetaObject = PSEnumerableBinder.IsEnumerable(target);
            if (dynamicMetaObject != null || target.Value != null && arg.Value != null)
            {
                DynamicMetaObject dynamicMetaObject1 = this.BinaryComparisonCommon(dynamicMetaObject, target, arg);
                DynamicMetaObject dynamicMetaObject2 = dynamicMetaObject1;
                if (dynamicMetaObject1 == null)
                {
                    PSBinaryOperationBinder pSBinaryOperationBinder = this;
                    DynamicMetaObject dynamicMetaObject3 = target;
                    DynamicMetaObject dynamicMetaObject4 = arg;
                    dynamicMetaObject2 = pSBinaryOperationBinder.BinaryComparision(dynamicMetaObject3, dynamicMetaObject4, (Expression e) => Expression.LessThan(e, ExpressionCache.Constant(0)));
                }
                return dynamicMetaObject2;
            }
            else
            {
                if (target.LimitType.IsNumeric())
                {
                    boxedTrue = PSBinaryOperationBinder.CompareWithZero(target, new Func<Expression, Expression, Expression>(Expression.LessThan));
                }
                else
                {
                    if (arg.LimitType.IsNumeric())
                    {
                        boxedTrue = PSBinaryOperationBinder.CompareWithZero(arg, new Func<Expression, Expression, Expression>(Expression.GreaterThanOrEqual));
                    }
                    else
                    {
                        if (arg.Value != null)
                        {
                            boxedTrue = ExpressionCache.BoxedTrue;
                        }
                        else
                        {
                            boxedTrue = ExpressionCache.BoxedFalse;
                        }
                    }
                }
                Expression expression = boxedTrue;
                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                dynamicMetaObjectArray[0] = arg;
                return new DynamicMetaObject(expression, target.CombineRestrictions(dynamicMetaObjectArray));
            }
        }

        private DynamicMetaObject CompareNE(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            Expression boxedFalse;
            if (target.Value != null)
            {
                DynamicMetaObject dynamicMetaObject = PSEnumerableBinder.IsEnumerable(target);
                if (dynamicMetaObject != null || arg.Value != null)
                {
                    DynamicMetaObject dynamicMetaObject1 = this.BinaryComparisonCommon(dynamicMetaObject, target, arg);
                    DynamicMetaObject dynamicMetaObject2 = dynamicMetaObject1;
                    if (dynamicMetaObject1 == null)
                    {
                        dynamicMetaObject2 = this.BinaryEqualityComparison(target, arg);
                    }
                    return dynamicMetaObject2;
                }
                else
                {
                    DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                    dynamicMetaObjectArray[0] = arg;
                    return new DynamicMetaObject(ExpressionCache.BoxedTrue, target.CombineRestrictions(dynamicMetaObjectArray));
                }
            }
            else
            {
                if (arg.Value == null)
                {
                    boxedFalse = ExpressionCache.BoxedFalse;
                }
                else
                {
                    boxedFalse = ExpressionCache.BoxedTrue;
                }
                DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                dynamicMetaObjectArray1[0] = arg;
                return new DynamicMetaObject(boxedFalse, target.CombineRestrictions(dynamicMetaObjectArray1));
            }
        }

        private static Expression CompareWithZero(DynamicMetaObject target, Func<Expression, Expression, Expression> comparer)
        {
            return comparer(target.Expression.Cast(target.LimitType), ExpressionCache.Constant(0).Cast(target.LimitType)).Cast(typeof(object));
        }

        internal static Expression ConvertStringToNumber(Expression expr, Type toType)
        {
            if (!toType.IsNumeric())
            {
                toType = typeof(int);
            }
            return Expression.Call(CachedReflectionInfo.Parser_ScanNumber, expr.Cast(typeof(string)), Expression.Constant(toType));
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            object[] objArray;
            DynamicMetaObject[] dynamicMetaObjectArray;
            DynamicMetaObject dynamicMetaObject;
            DynamicMetaObject dynamicMetaObject1;
            if (!target.HasValue || !arg.HasValue)
            {
                DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                dynamicMetaObjectArray1[0] = arg;
                return base.Defer(target, dynamicMetaObjectArray1).WriteToDebugLog(this);
            }
            else
            {
                if ((target.Value as PSObject == null || PSObject.Base(target.Value) == target.Value) && (arg.Value as PSObject == null || PSObject.Base(arg.Value) == arg.Value) || base.Operation == ExpressionType.Add && PSEnumerableBinder.IsEnumerable(target) != null)
                {
                    ExpressionType operation = base.Operation;
                    if (operation > ExpressionType.Multiply)
                    {
                        if (operation == ExpressionType.NotEqual)
                        {
                            return this.CompareNE(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.Or)
                        {
                            return this.BinaryBitwiseOr(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        if (operation == ExpressionType.RightShift)
                        {
                            return this.RightShift(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.Subtract)
                        {
                            return this.BinarySub(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                    }
                    else
                    {
                        if (operation == ExpressionType.Add)
                        {
                            return this.BinaryAdd(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.AddChecked)
                        {
                            dynamicMetaObject = errorSuggestion;
                            dynamicMetaObject1 = dynamicMetaObject;
                            if (dynamicMetaObject == null)
                            {
                                objArray = new object[1];
                                objArray[0] = "Unimplemented operaton";
                                dynamicMetaObjectArray = new DynamicMetaObject[1];
                                dynamicMetaObjectArray[0] = arg;
                                dynamicMetaObject1 = new DynamicMetaObject(Compiler.CreateThrow(typeof(object), typeof(PSNotImplementedException), objArray), target.CombineRestrictions(dynamicMetaObjectArray));
                            }
                            return dynamicMetaObject1.WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.And)
                        {
                            return this.BinaryBitwiseAnd(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        if (operation == ExpressionType.Divide)
                        {
                            return this.BinaryDivide(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.Equal)
                        {
                            return this.CompareEQ(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.ExclusiveOr)
                        {
                            return this.BinaryBitwiseXor(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.GreaterThan)
                        {
                            return this.CompareGT(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.GreaterThanOrEqual)
                        {
                            return this.CompareGE(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.Invoke || operation == ExpressionType.Lambda || operation == ExpressionType.ListInit || operation == ExpressionType.MemberAccess || operation == ExpressionType.MemberInit)
                        {
                            dynamicMetaObject = errorSuggestion;
                            dynamicMetaObject1 = dynamicMetaObject;
                            if (dynamicMetaObject == null)
                            {
                                objArray = new object[1];
                                objArray[0] = "Unimplemented operaton";
                                dynamicMetaObjectArray = new DynamicMetaObject[1];
                                dynamicMetaObjectArray[0] = arg;
                                dynamicMetaObject1 = new DynamicMetaObject(Compiler.CreateThrow(typeof(object), typeof(PSNotImplementedException), objArray), target.CombineRestrictions(dynamicMetaObjectArray));
                            }
                            return dynamicMetaObject1.WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.LeftShift)
                        {
                            return this.LeftShift(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.LessThan)
                        {
                            return this.CompareLT(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.LessThanOrEqual)
                        {
                            return this.CompareLE(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.Modulo)
                        {
                            return this.BinaryRemainder(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                        else if (operation == ExpressionType.Multiply)
                        {
                            return this.BinaryMultiply(target, arg, errorSuggestion).WriteToDebugLog(this);
                        }
                    }
                    dynamicMetaObject = errorSuggestion;
                    dynamicMetaObject1 = dynamicMetaObject;
                    if (dynamicMetaObject == null)
                    {
                        objArray = new object[1];
                        objArray[0] = "Unimplemented operaton";
                        dynamicMetaObjectArray = new DynamicMetaObject[1];
                        dynamicMetaObjectArray[0] = arg;
                        dynamicMetaObject1 = new DynamicMetaObject(Compiler.CreateThrow(typeof(object), typeof(PSNotImplementedException), objArray), target.CombineRestrictions(dynamicMetaObjectArray));
                    }
                    return dynamicMetaObject1.WriteToDebugLog(this);
                }
                else
                {
                    DynamicMetaObject[] dynamicMetaObjectArray2 = new DynamicMetaObject[2];
                    dynamicMetaObjectArray2[0] = target;
                    dynamicMetaObjectArray2[1] = arg;
                    return this.DeferForPSObject(dynamicMetaObjectArray2).WriteToDebugLog(this);
                }
            }
        }

        private static DynamicMetaObject FigureSignedUnsignedInt(DynamicMetaObject obj, TypeCode typeCode, out Type opImplType, out Type argType)
        {
            if (!PSBinaryOperationBinder.IsValueNegative(obj.Value, typeCode))
            {
                opImplType = null;
                argType = null;
                return new DynamicMetaObject(obj.Expression, obj.PSGetTypeRestriction().Merge(BindingRestrictions.GetExpressionRestriction(Expression.GreaterThanOrEqual(obj.Expression.Cast(obj.LimitType), PSBinaryOperationBinder.TypedZero(typeCode)))), obj.Value);
            }
            else
            {
                opImplType = typeof(DoubleOps);
                argType = typeof(double);
                return new DynamicMetaObject(obj.Expression, obj.PSGetTypeRestriction().Merge(BindingRestrictions.GetExpressionRestriction(Expression.LessThan(obj.Expression.Cast(obj.LimitType), PSBinaryOperationBinder.TypedZero(typeCode)))), obj.Value);
            }
        }

        internal static PSBinaryOperationBinder Get(ExpressionType operation, bool ignoreCase = true, bool scalarCompare = false)
        {
            PSBinaryOperationBinder pSBinaryOperationBinder = null;
            lock (PSBinaryOperationBinder._binderCache)
            {
                Tuple<ExpressionType, bool, bool> tuple = Tuple.Create<ExpressionType, bool, bool>(operation, ignoreCase, scalarCompare);
                if (!PSBinaryOperationBinder._binderCache.TryGetValue(tuple, out pSBinaryOperationBinder))
                {
                    pSBinaryOperationBinder = new PSBinaryOperationBinder(operation, ignoreCase, scalarCompare);
                    PSBinaryOperationBinder._binderCache.Add(tuple, pSBinaryOperationBinder);
                }
            }
            return pSBinaryOperationBinder;
        }

        private static DynamicMetaObject GetArgAsNumericOrPrimitive(DynamicMetaObject arg, Type targetType)
        {
            bool flag = false;
            if (arg.Value != null)
            {
                bool flag1 = false;
                if (arg.LimitType.IsNumericOrPrimitive())
                {
                    if (!targetType.Equals(typeof(decimal)) || !arg.LimitType.Equals(typeof(bool)))
                    {
                        return arg;
                    }
                    else
                    {
                        flag1 = true;
                    }
                }
                LanguagePrimitives.ConversionData conversionDatum = LanguagePrimitives.FigureConversion(arg.Value, targetType, out flag);
                if (conversionDatum.Rank == ConversionRank.ImplicitCast || flag1 || arg.LimitType.IsEnum)
                {
                    return new DynamicMetaObject(PSConvertBinder.InvokeConverter(conversionDatum, arg.Expression, targetType, flag, ExpressionCache.InvariantCulture), arg.PSGetTypeRestriction());
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new DynamicMetaObject(ExpressionCache.Constant(0), arg.PSGetTypeRestriction(), (object)0);
            }
        }

        private static Type GetBitwiseOpType(TypeCode opTypeCode)
        {
            Type type;
            if (opTypeCode > TypeCode.Int32)
            {
                if (opTypeCode > TypeCode.UInt32)
                {
                    if (opTypeCode > TypeCode.Int64)
                    {
                        type = typeof(ulong);
                    }
                    else
                    {
                        type = typeof(long);
                    }
                }
                else
                {
                    type = typeof(int);
                }
            }
            else
            {
                type = typeof(int);
            }
            return type;
        }

        private string GetOperatorText()
        {
            ExpressionType operation = base.Operation;
            if (operation > ExpressionType.Multiply)
            {
                switch (operation)
                {
                    case ExpressionType.NotEqual:
                        if (this._ignoreCase)
                        {
                            return TokenKind.Ine.Text();
                        }
                        else
                        {
                            return TokenKind.Cne.Text();
                        }
                    case ExpressionType.Or:
                        return TokenKind.Bor.Text();
                    default:
                        {
                            switch (operation)
                            {
                                case ExpressionType.RightShift:
                                    {
                                        return TokenKind.Shr.Text();
                                    }
                                case ExpressionType.Subtract:
                                    {
                                        return TokenKind.Minus.Text();
                                    }
                            }
                        }
                        break;
                }
            }
            else
            {
                if (operation == ExpressionType.Add)
                {
                    return TokenKind.Plus.Text();
                }
                else if (operation == ExpressionType.AddChecked)
                {
                    return "";
                }
                else if (operation == ExpressionType.And)
                {
                    return TokenKind.Band.Text();
                }
                if (operation == ExpressionType.Divide)
                {
                    return TokenKind.Divide.Text();
                }
                else if (operation == ExpressionType.Equal)
                {
                    if (this._ignoreCase)
                    {
                        return TokenKind.Ieq.Text();
                    }
                    else
                    {
                        return TokenKind.Ceq.Text();
                    }
                }
                else if (operation == ExpressionType.ExclusiveOr)
                {
                    return TokenKind.Bxor.Text();
                }
                else if (operation == ExpressionType.GreaterThan)
                {
                    if (this._ignoreCase)
                    {
                        return TokenKind.Igt.Text();
                    }
                    else
                    {
                        return TokenKind.Cgt.Text();
                    }
                }
                else if (operation == ExpressionType.GreaterThanOrEqual)
                {
                    if (this._ignoreCase)
                    {
                        return TokenKind.Ige.Text();
                    }
                    else
                    {
                        return TokenKind.Cge.Text();
                    }
                }
                else if (operation == ExpressionType.Invoke || operation == ExpressionType.Lambda || operation == ExpressionType.ListInit || operation == ExpressionType.MemberAccess || operation == ExpressionType.MemberInit)
                {
                    return "";
                }
                else if (operation == ExpressionType.LeftShift)
                {
                    return TokenKind.Shl.Text();
                }
                else if (operation == ExpressionType.LessThan)
                {
                    if (this._ignoreCase)
                    {
                        return TokenKind.Ilt.Text();
                    }
                    else
                    {
                        return TokenKind.Clt.Text();
                    }
                }
                else if (operation == ExpressionType.LessThanOrEqual)
                {
                    if (this._ignoreCase)
                    {
                        return TokenKind.Ile.Text();
                    }
                    else
                    {
                        return TokenKind.Cle.Text();
                    }
                }
                else if (operation == ExpressionType.Modulo)
                {
                    return TokenKind.Rem.Text();
                }
                else if (operation == ExpressionType.Multiply)
                {
                    return TokenKind.Multiply.Text();
                }
            }
            return "";
        }

        private Func<object, object, bool> GetScalarCompareDelegate()
        {
            if (this._compareDelegate == null)
            {
                ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "lval");
                ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object), "rval");
                ParameterExpression[] parameterExpressionArray = new ParameterExpression[2];
                parameterExpressionArray[0] = parameterExpression;
                parameterExpressionArray[1] = parameterExpression1;
                Func<object, object, bool> func = Expression.Lambda<Func<object, object, bool>>(Expression.Dynamic(PSBinaryOperationBinder.Get(base.Operation, this._ignoreCase, true), typeof(object), parameterExpression, parameterExpression1).Cast(typeof(bool)), parameterExpressionArray).Compile();
                Interlocked.CompareExchange<Func<object, object, bool>>(ref this._compareDelegate, func, null);
            }
            return this._compareDelegate;
        }

        internal static void InvalidateCache()
        {
            lock (PSBinaryOperationBinder._binderCache)
            {
                foreach (PSBinaryOperationBinder value in PSBinaryOperationBinder._binderCache.Values)
                {
                    PSBinaryOperationBinder pSBinaryOperationBinder = value;
                    pSBinaryOperationBinder._version = pSBinaryOperationBinder._version + 1;
                }
            }
        }

        private static bool IsValueNegative(object value, TypeCode typeCode)
        {
            TypeCode typeCode1 = typeCode;
            switch (typeCode1)
            {
                case TypeCode.SByte:
                    {
                        return (sbyte)value < 0;
                    }
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    {
                        return true;
                    }
                case TypeCode.Int16:
                    {
                        return (short)value < 0;
                    }
                case TypeCode.Int32:
                    {
                        return (int)value < 0;
                    }
                case TypeCode.Int64:
                    {
                        return (long)value < (long)0;
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        private DynamicMetaObject LeftShift(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return this.Shift(target, arg, errorSuggestion, "op_LeftShift", new Func<Expression, Expression, Expression>(Expression.LeftShift));
        }

        private DynamicMetaObject RightShift(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            return this.Shift(target, arg, errorSuggestion, "op_RightShift", new Func<Expression, Expression, Expression>(Expression.RightShift));
        }

        private DynamicMetaObject Shift(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion, string userOp, Func<Expression, Expression, Expression> exprGenerator)
        {
            bool flag = false;
            Type type;
            Type type1;
            byte num;
            if (target.Value != null)
            {
                if (target.LimitType.Equals(typeof(string)) || arg.LimitType.Equals(typeof(string)))
                {
                    return this.BinaryNumericStringOp(target, arg);
                }
                else
                {
                    TypeCode typeCode = LanguagePrimitives.GetTypeCode(target.LimitType);
                    if (target.LimitType.IsNumeric())
                    {
                        Type type2 = typeof(int);
                        LanguagePrimitives.ConversionData conversionDatum = LanguagePrimitives.FigureConversion(arg.Value, type2, out flag);
                        if (conversionDatum.Rank != ConversionRank.None)
                        {
                            Expression expression = PSConvertBinder.InvokeConverter(conversionDatum, arg.Expression, type2, flag, ExpressionCache.InvariantCulture);
                            if (typeCode == TypeCode.Decimal || typeCode == TypeCode.Double || typeCode == TypeCode.Single)
                            {
                                if (typeCode == TypeCode.Decimal)
                                {
                                    type = typeof(DecimalOps);
                                }
                                else
                                {
                                    type = typeof(DoubleOps);
                                }
                                Type type3 = type;
                                if (typeCode == TypeCode.Decimal)
                                {
                                    type1 = typeof(decimal);
                                }
                                else
                                {
                                    type1 = typeof(double);
                                }
                                Type type4 = type1;
                                string str = userOp.Substring(3);
                                DynamicMetaObject[] dynamicMetaObjectArray = new DynamicMetaObject[1];
                                dynamicMetaObjectArray[0] = arg;
                                return new DynamicMetaObject(Expression.Call(type3.GetMethod(str, BindingFlags.Static | BindingFlags.NonPublic), target.Expression.Cast(type4), expression), target.CombineRestrictions(dynamicMetaObjectArray));
                            }
                            else
                            {
                                Expression expression1 = target.Expression.Cast(target.LimitType);
                                Expression expression2 = expression;
                                if (typeCode < TypeCode.Int64)
                                {
                                    num = 31;
                                }
                                else
                                {
                                    num = 63;
                                }
                                expression = Expression.And(expression2, Expression.Constant(num, typeof(int)));
                                DynamicMetaObject[] dynamicMetaObjectArray1 = new DynamicMetaObject[1];
                                dynamicMetaObjectArray1[0] = arg;
                                return new DynamicMetaObject(exprGenerator(expression1, expression).Cast(typeof(object)), target.CombineRestrictions(dynamicMetaObjectArray1));
                            }
                        }
                        else
                        {
                            return PSConvertBinder.ThrowNoConversion(arg, typeof(int), this, this._version, new DynamicMetaObject[0]);
                        }
                    }
                    else
                    {
                        return PSBinaryOperationBinder.CallImplicitOp(userOp, target, arg, this.GetOperatorText(), errorSuggestion);
                    }
                }
            }
            else
            {
                return new DynamicMetaObject(ExpressionCache.Constant(0).Convert(typeof(object)), target.PSGetTypeRestriction());
            }
        }

        public override string ToString()
        {
            object obj;
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            string str = "PSBinaryOperationBinder {0}{1} ver:{2}";
            object[] operatorText = new object[3];
            operatorText[0] = this.GetOperatorText();
            object[] objArray = operatorText;
            int num = 1;
            if (this._scalarCompare)
            {
                obj = " scalarOnly";
            }
            else
            {
                obj = "";
            }
            objArray[num] = obj;
            operatorText[2] = this._version;
            return string.Format(invariantCulture, str, operatorText);
        }

        private static Expression TypedZero(TypeCode typeCode)
        {
            TypeCode typeCode1 = typeCode;
            switch (typeCode1)
            {
                case TypeCode.SByte:
                    {
                        return Expression.Constant((sbyte)0);
                    }
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                    {
                        return null;
                    }
                case TypeCode.Int16:
                    {
                        return Expression.Constant((short)0);
                    }
                case TypeCode.Int32:
                    {
                        return ExpressionCache.Constant(0);
                    }
                case TypeCode.Int64:
                    {
                        return Expression.Constant((long)0);
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}