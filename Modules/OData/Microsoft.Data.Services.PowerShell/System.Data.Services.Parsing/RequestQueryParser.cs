namespace System.Data.Services.Parsing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services;
    using System.Data.Services.Providers;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Spatial;
    using System.Threading;

    internal static class RequestQueryParser
    {
        internal static Expression OrderBy(IDataService service, Expression source, OrderingInfo orderingInfo)
        {
            Expression expression = source;
            bool flag = true;
            foreach (OrderingExpression expression2 in orderingInfo.OrderingExpressions)
            {
                LambdaExpression keySelector = (LambdaExpression) expression2.Expression;
                Type clrType = keySelector.Body.Type;
                service.Provider.CheckIfOrderedType(clrType);
                if (flag)
                {
                    expression = expression2.IsAscending ? expression.QueryableOrderBy(keySelector) : expression.QueryableOrderByDescending(keySelector);
                }
                else
                {
                    expression = expression2.IsAscending ? expression.QueryableThenBy(keySelector) : expression.QueryableThenByDescending(keySelector);
                }
                flag = false;
            }
            return expression;
        }

        private static LambdaExpression ParseLambdaForWhere(IDataService service, RequestDescription requestDescription, Type queryElementType, string expression)
        {
            ParameterExpression parameterForIt = Expression.Parameter(queryElementType, "it");
            ExpressionParser parser = new ExpressionParser(service, requestDescription, parameterForIt, expression);
            return Expression.Lambda(parser.ParseWhere(), new ParameterExpression[] { parameterForIt });
        }

        internal static Expression Where(IDataService service, RequestDescription requestDescription, Expression source, string predicate)
        {
            LambdaExpression expression = ParseLambdaForWhere(service, requestDescription, source.ElementType(), predicate);
            return source.QueryableWhere(expression);
        }

        [DebuggerDisplay("ExpressionParser ({lexer.text})")]
        internal class ExpressionParser
        {
            private static readonly MethodInfo AreByteArraysEqualMethodInfo = typeof(DataServiceProviderMethods).GetMethod("AreByteArraysEqual", BindingFlags.Public | BindingFlags.Static);
            private static readonly MethodInfo AreByteArraysNotEqualMethodInfo = typeof(DataServiceProviderMethods).GetMethod("AreByteArraysNotEqual", BindingFlags.Public | BindingFlags.Static);
            private static readonly MethodInfo BoolCompareMethodInfo = typeof(DataServiceProviderMethods).GetMethods(BindingFlags.Public | BindingFlags.Static).Single<MethodInfo>(m => ((m.Name == "Compare") && (m.GetParameters()[0].ParameterType == typeof(bool))));
            private static readonly MethodInfo BoolCompareMethodInfoNullable = typeof(DataServiceProviderMethods).GetMethods(BindingFlags.Public | BindingFlags.Static).Single<MethodInfo>(m => ((m.Name == "Compare") && (m.GetParameters()[0].ParameterType == typeof(bool?))));
            private SegmentTypeInfo currentSegmentInfo;
            private static readonly Expression[] emptyExpressions = new Expression[0];
            private static readonly ConstantExpression falseLiteral = Expression.Constant(false);
            private static readonly Dictionary<string, FunctionDescription[]> functions = FunctionDescription.CreateFunctions();
            private static readonly MethodInfo GuidCompareMethodInfo = typeof(DataServiceProviderMethods).GetMethods(BindingFlags.Public | BindingFlags.Static).Single<MethodInfo>(m => ((m.Name == "Compare") && (m.GetParameters()[0].ParameterType == typeof(Guid))));
            private static readonly MethodInfo GuidCompareMethodInfoNullable = typeof(DataServiceProviderMethods).GetMethods(BindingFlags.Public | BindingFlags.Static).Single<MethodInfo>(m => ((m.Name == "Compare") && (m.GetParameters()[0].ParameterType == typeof(Guid?))));
            private readonly ParameterExpression it;
            private readonly ExpressionLexer lexer;
            private readonly Dictionary<Expression, string> literals;
            private readonly bool nullPropagationRequired;
            private const int NumericTypeNotIntegral = 1;
            private const int NumericTypeNotNumeric = 0;
            private const int NumericTypeSignedIntegral = 2;
            private const int NumericTypeUnsignedIntegral = 3;
            private readonly Dictionary<string, SegmentTypeInfo> parameterMap;
            private readonly DataServiceProviderWrapper provider;
            private string queryOptionGettingParsed;
            private int recursionDepth;
            private const int RecursionLimit = 800;
            private readonly RequestDescription requestDescription;
            private readonly IDataService service;
            private static readonly MethodInfo StringCompareMethodInfo = typeof(DataServiceProviderMethods).GetMethods(BindingFlags.Public | BindingFlags.Static).Single<MethodInfo>(m => ((m.Name == "Compare") && (m.GetParameters()[0].ParameterType == typeof(string))));
            private WellKnownTextSqlFormatter wellKnownTextFormatter;

            internal ExpressionParser(IDataService service, RequestDescription requestDescription, ParameterExpression parameterForIt, string expression)
            {
                this.service = service;
                this.provider = service.Provider;
                this.nullPropagationRequired = this.provider.NullPropagationRequired;
                this.literals = new Dictionary<Expression, string>(ReferenceEqualityComparer<Expression>.Instance);
                this.requestDescription = requestDescription;
                this.it = parameterForIt;
                this.lexer = new ExpressionLexer(expression);
                this.currentSegmentInfo = new SegmentTypeInfo(this.requestDescription.TargetResourceType, this.requestDescription.TargetResourceSet, parameterForIt, false, false);
                this.parameterMap = new Dictionary<string, SegmentTypeInfo>(EqualityComparer<string>.Default);
                this.parameterMap.Add("$it", this.currentSegmentInfo);
            }

            
            private static void AddInterface(List<Type> types, Type type)
            {
                if (!types.Contains(type))
                {
                    types.Add(type);
                    foreach (Type type2 in type.GetInterfaces())
                    {
                        AddInterface(types, type2);
                    }
                }
            }

            [Conditional("DEBUG")]
            private static void AssertTokenIdentifierIs(Token token, string id, string message)
            {
            }

            internal LambdaExpression BuildSkipTokenFilter(OrderingInfo topLevelOrderingInfo, KeyInstance k)
            {
                ParameterExpression newExpression = Expression.Parameter(this.requestDescription.TargetResourceType.InstanceType, "element");
                Expression left = Expression.Constant(true, typeof(bool));
                Expression expression3 = Expression.Constant(false, typeof(bool));
                foreach (var type in WebUtil.Zip(topLevelOrderingInfo.OrderingExpressions, k.PositionalValues, (x, y) => new { Order = x, Value = y }))
                {
                    Token op = type.Order.IsAscending ? Token.GreaterThan : Token.LessThan;
                    Expression expression4 = ParameterReplacerVisitor.Replace(((LambdaExpression) type.Order.Expression).Body, ((LambdaExpression) type.Order.Expression).Parameters[0], newExpression);
                    Expression right = GenerateLogicalAnd(left, this.GenerateNullAwareComparison(expression4, (string) type.Value, op));
                    expression3 = GenerateLogicalOr(expression3, right);
                    left = GenerateLogicalAnd(left, this.GenerateComparison(expression4, (string) type.Value, Token.EqualsTo));
                }
                return Expression.Lambda(PreparePredicateExpressionForLambda(expression3), new ParameterExpression[] { newExpression });
            }

            private void CheckAndPromoteOperand(Type signatures, string operationName, ref Expression expr, int errorPos)
            {
                MethodBase base2;
                Expression[] args = new Expression[] { expr };
                if (this.FindMethod(signatures, "F", args, out base2) != 1)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_IncompatibleOperand(operationName, WebUtil.GetTypeName(args[0].Type), errorPos));
                }
                expr = args[0];
            }

            private void CheckAndPromoteOperands(Type signatures, string operationName, ref Expression left, ref Expression right, int errorPos)
            {
                MethodBase base2;
                Expression[] args = new Expression[] { left, right };
                if (this.FindMethod(signatures, "F", args, out base2) != 1)
                {
                    throw IncompatibleOperandsError(operationName, left, right, errorPos);
                }
                left = args[0];
                right = args[1];
            }

            private static int CompareConversions(Type source, Type targetA, Type targetB)
            {
                if (targetA != targetB)
                {
                    if (source == targetA)
                    {
                        return 1;
                    }
                    if (source == targetB)
                    {
                        return -1;
                    }
                    bool flag = IsCompatibleWith(targetA, targetB);
                    bool flag2 = IsCompatibleWith(targetB, targetA);
                    if (flag && !flag2)
                    {
                        return 1;
                    }
                    if (flag2 && !flag)
                    {
                        return -1;
                    }
                    bool flag3 = WebUtil.IsNullableType(source);
                    bool flag4 = WebUtil.IsNullableType(targetA);
                    bool flag5 = WebUtil.IsNullableType(targetB);
                    if ((flag3 == flag4) && (flag3 != flag5))
                    {
                        return 1;
                    }
                    if ((flag3 != flag4) && (flag3 == flag5))
                    {
                        return -1;
                    }
                    if (IsSignedIntegralType(targetA) && IsUnsignedIntegralType(targetB))
                    {
                        return 1;
                    }
                    if (IsSignedIntegralType(targetB) && IsUnsignedIntegralType(targetA))
                    {
                        return -1;
                    }
                    if ((targetA != typeof(object)) && (targetB == typeof(object)))
                    {
                        return 1;
                    }
                    if ((targetB != typeof(object)) && (targetA == typeof(object)))
                    {
                        return -1;
                    }
                }
                return 0;
            }

            private Expression ConsiderNullPropagation(Expression element, Expression notNullExpression)
            {
                if ((!this.nullPropagationRequired || (element is ParameterExpression)) || !WebUtil.TypeAllowsNull(element.Type))
                {
                    return notNullExpression;
                }
                if ((element is ConstantExpression) && (element != ExpressionUtils.NullLiteral))
                {
                    return notNullExpression;
                }
                Expression test = Expression.Equal(element, Expression.Constant(null, element.Type));
                Expression expression = notNullExpression;
                if (!WebUtil.TypeAllowsNull(expression.Type))
                {
                    expression = Expression.Convert(expression, typeof(Nullable<>).MakeGenericType(new Type[] { expression.Type }));
                }
                Expression ifTrue = Expression.Constant(null, expression.Type);
                return Expression.Condition(test, ifTrue, expression);
            }

            private Expression ConvertNullCollectionToEmpty(Expression expressionToCheck)
            {
                if (this.nullPropagationRequired)
                {
                    Type iEnumerableElement = BaseServiceProvider.GetIEnumerableElement(expressionToCheck.Type);
                    Expression test = Expression.Equal(expressionToCheck, Expression.Constant(null, expressionToCheck.Type));
                    Expression ifFalse = expressionToCheck;
                    Expression ifTrue = ExpressionUtils.EnumerableEmpty(iEnumerableElement);
                    return Expression.Condition(test, ifTrue, ifFalse, ifTrue.Type);
                }
                return expressionToCheck;
            }

            private Expression CreateLiteral(object value, string text, Type constantType)
            {
                ConstantExpression key = Expression.Constant(value, constantType);
                this.literals.Add(key, text);
                return key;
            }

            private Expression CreateTypeFilterExpression(Expression source, ResourceType targetResourceType)
            {
                source = this.ConvertNullCollectionToEmpty(source);
                if (targetResourceType.CanReflectOnInstanceType)
                {
                    return source.EnumerableOfType(targetResourceType.InstanceType);
                }
                source = Expression.Call(DataServiceProviderMethods.OfTypeIEnumerableMethodInfo.MakeGenericMethod(new Type[] { BaseServiceProvider.GetIEnumerableElement(source.Type), targetResourceType.InstanceType }), source, Expression.Constant(targetResourceType));
                return source.EnumerableCast(targetResourceType.InstanceType);
            }

            private MethodData[] FindApplicableMethods(IEnumerable<MethodBase> methods, Expression[] args)
            {
                List<MethodData> list = new List<MethodData>();
                foreach (MethodBase base2 in methods)
                {
                    MethodData method = new MethodData(base2, base2.GetParameters());
                    if (this.IsApplicable(method, args))
                    {
                        list.Add(method);
                    }
                }
                return list.ToArray();
            }

            private static MethodData[] FindBestApplicableMethods(MethodData[] applicable, Expression[] args)
            {
                List<MethodData> list = new List<MethodData>();
                foreach (MethodData data in applicable)
                {
                    bool flag = true;
                    foreach (MethodData data2 in applicable)
                    {
                        if ((data2 != data) && IsBetterThan(args, data2, data))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        list.Add(data);
                    }
                }
                return list.ToArray();
            }

            private FunctionDescription FindBestFunction(FunctionDescription[] functions, ref Expression[] arguments)
            {
                List<FunctionDescription> list = new List<FunctionDescription>(functions.Length);
                List<Expression[]> list2 = new List<Expression[]>(functions.Length);
                foreach (FunctionDescription description in functions)
                {
                    if (description.ParameterTypes.Length != arguments.Length)
                    {
                        continue;
                    }
                    Expression[] item = new Expression[arguments.Length];
                    bool flag = true;
                    for (int j = 0; j < description.ParameterTypes.Length; j++)
                    {
                        item[j] = this.PromoteExpression(arguments[j], description.ParameterTypes[j], true);
                        if (item[j] == null)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        list.Add(description);
                        list2.Add(item);
                    }
                }
                if (list.Count == 0)
                {
                    return null;
                }
                if (list.Count == 1)
                {
                    arguments = list2[0];
                    return list[0];
                }
                int num2 = -1;
                for (int i = 0; i < list.Count; i++)
                {
                    bool flag2 = true;
                    for (int k = 0; k < list.Count; k++)
                    {
                        if ((i != k) && IsBetterThan(arguments, list[k].ParameterTypes, list[i].ParameterTypes))
                        {
                            flag2 = false;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        num2 = i;
                        break;
                    }
                }
                if (num2 == -1)
                {
                    return null;
                }
                arguments = list2[num2];
                return list[num2];
            }

            private int FindBestMethod(IEnumerable<MethodBase> methods, Expression[] args, out MethodBase method)
            {
                MethodData[] applicable = this.FindApplicableMethods(methods, args);
                if (applicable.Length > 1)
                {
                    applicable = FindBestApplicableMethods(applicable, args);
                }
                int length = applicable.Length;
                method = null;
                if (applicable.Length == 1)
                {
                    MethodData data = applicable[0];
                    bool flag = true;
                    bool flag2 = true;
                    for (int i = 0; i < args.Length; i++)
                    {
                        flag = flag && !IsOpenPropertyExpression(args[i]);
                        flag2 = flag2 && (data.Parameters[i].ParameterType == typeof(object));
                        args[i] = data.Args[i];
                    }
                    method = (flag && flag2) ? null : data.MethodBase;
                    return ((method == null) ? 0 : 1);
                }
                if (((applicable.Length > 1) && (args.Length == 2)) && ((applicable.Length == 2) && (GetNonNullableType(applicable[0].Parameters[0].ParameterType) == GetNonNullableType(applicable[1].Parameters[0].ParameterType))))
                {
                    MethodData data2 = WebUtil.TypeAllowsNull(applicable[0].Parameters[0].ParameterType) ? applicable[0] : applicable[1];
                    args[0] = data2.Args[0];
                    args[1] = data2.Args[1];
                    return this.FindBestMethod(methods, args, out method);
                }
                return length;
            }

            private int FindMethod(Type type, string methodName, Expression[] args, out MethodBase method)
            {
                foreach (Type type2 in SelfAndBaseTypes(type))
                {
                    MemberInfo[] source = type2.FindMembers(MemberTypes.Method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly, Type.FilterName, methodName);
                    int num = this.FindBestMethod(source.Cast<MethodBase>(), args, out method);
                    if (num != 0)
                    {
                        return num;
                    }
                }
                method = null;
                return 0;
            }

            private static Expression GenerateAdd(Expression left, Expression right)
            {
                if (!IsOpenPropertyExpression(left) && !IsOpenPropertyExpression(right))
                {
                    return Expression.Add(left, right);
                }
                return OpenTypeMethods.AddExpression(left, right);
            }

            private Expression GenerateComparison(Expression left, string rightLiteral, Token op)
            {
                ExpressionLexer l = new ExpressionLexer(rightLiteral);
                return this.GenerateComparisonExpression(left, this.ParsePrimaryStart(l), op);
            }

            private Expression GenerateComparisonExpression(Expression left, Expression right, Token op)
            {
                if (left.Type.IsSpatial() || right.Type.IsSpatial())
                {
                    throw IncompatibleOperandsError(op.Text, left, right, op.Position);
                }
                bool isEqualityOperator = op.IsEqualityOperator;
                if ((isEqualityOperator && !left.Type.IsValueType) && !right.Type.IsValueType)
                {
                    if (left.Type != right.Type)
                    {
                        if (!ExpressionUtils.IsNullConstant(left))
                        {
                            if (!ExpressionUtils.IsNullConstant(right))
                            {
                                if (!left.Type.IsAssignableFrom(right.Type))
                                {
                                    if (!right.Type.IsAssignableFrom(left.Type))
                                    {
                                        throw IncompatibleOperandsError(op.Text, left, right, op.Position);
                                    }
                                    left = Expression.Convert(left, right.Type);
                                }
                                else
                                {
                                    right = Expression.Convert(right, left.Type);
                                }
                            }
                            else
                            {
                                right = Expression.Constant(null, left.Type);
                            }
                        }
                        else
                        {
                            left = Expression.Constant(null, right.Type);
                        }
                    }
                }
                else if ((left == ExpressionUtils.NullLiteral) || (right == ExpressionUtils.NullLiteral))
                {
                    if (!isEqualityOperator)
                    {
                        throw ParseError(System.Data.Services.Strings.RequestQueryParser_NullOperatorUnsupported(op.Text, op.Position, this.lexer.ExpressionText));
                    }
                    if (!WebUtil.TypeAllowsNull(left.Type))
                    {
                        left = Expression.Convert(left, typeof(Nullable<>).MakeGenericType(new Type[] { left.Type }));
                    }
                    else if (!WebUtil.TypeAllowsNull(right.Type))
                    {
                        right = Expression.Convert(right, typeof(Nullable<>).MakeGenericType(new Type[] { right.Type }));
                    }
                }
                else
                {
                    Type signatures = isEqualityOperator ? typeof(OperationSignatures.IEqualitySignatures) : typeof(OperationSignatures.IRelationalSignatures);
                    this.CheckAndPromoteOperands(signatures, op.Text, ref left, ref right, op.Position);
                }
                MethodInfo comparisonMethodInfo = null;
                if (!isEqualityOperator)
                {
                    if (left.Type == typeof(string))
                    {
                        comparisonMethodInfo = StringCompareMethodInfo;
                    }
                    else if (left.Type == typeof(bool))
                    {
                        comparisonMethodInfo = BoolCompareMethodInfo;
                    }
                    else if (left.Type == typeof(bool?))
                    {
                        comparisonMethodInfo = BoolCompareMethodInfoNullable;
                    }
                    else if (left.Type == typeof(Guid))
                    {
                        comparisonMethodInfo = GuidCompareMethodInfo;
                    }
                    else if (left.Type == typeof(Guid?))
                    {
                        comparisonMethodInfo = GuidCompareMethodInfoNullable;
                    }
                }
                string text = op.Text;
                if (text != null)
                {
                    if (!(text == "eq"))
                    {
                        if (text == "ne")
                        {
                            left = GenerateNotEqual(left, right);
                            return left;
                        }
                        if (text == "gt")
                        {
                            left = GenerateGreaterThan(left, right, comparisonMethodInfo);
                            return left;
                        }
                        if (text == "ge")
                        {
                            left = GenerateGreaterThanEqual(left, right, comparisonMethodInfo);
                            return left;
                        }
                        if (text == "lt")
                        {
                            left = GenerateLessThan(left, right, comparisonMethodInfo);
                            return left;
                        }
                        if (text == "le")
                        {
                            left = GenerateLessThanEqual(left, right, comparisonMethodInfo);
                        }
                        return left;
                    }
                    left = GenerateEqual(left, right);
                }
                return left;
            }

            private static Expression GenerateDivide(Expression left, Expression right)
            {
                if (!IsOpenPropertyExpression(left) && !IsOpenPropertyExpression(right))
                {
                    return Expression.Divide(left, right);
                }
                return OpenTypeMethods.DivideExpression(left, right);
            }

            private static Expression GenerateEqual(Expression left, Expression right)
            {
                if (IsOpenPropertyExpression(left) || IsOpenPropertyExpression(right))
                {
                    return OpenTypeMethods.EqualExpression(left, right);
                }
                if (left.Type == typeof(byte[]))
                {
                    return Expression.Equal(left, right, false, AreByteArraysEqualMethodInfo);
                }
                return Expression.Equal(left, right);
            }

            private static Expression GenerateGreaterThan(Expression left, Expression right, MethodInfo comparisonMethodInfo)
            {
                if (IsOpenPropertyExpression(left) || IsOpenPropertyExpression(right))
                {
                    return OpenTypeMethods.GreaterThanExpression(left, right);
                }
                if (comparisonMethodInfo != null)
                {
                    left = Expression.Call(null, comparisonMethodInfo, left, right);
                    right = Expression.Constant(0, typeof(int));
                }
                return Expression.GreaterThan(left, right);
            }

            private static Expression GenerateGreaterThanEqual(Expression left, Expression right, MethodInfo comparisonMethodInfo)
            {
                if (IsOpenPropertyExpression(left) || IsOpenPropertyExpression(right))
                {
                    return OpenTypeMethods.GreaterThanOrEqualExpression(left, right);
                }
                if (comparisonMethodInfo != null)
                {
                    left = Expression.Call(null, comparisonMethodInfo, left, right);
                    right = Expression.Constant(0, typeof(int));
                }
                return Expression.GreaterThanOrEqual(left, right);
            }

            private static Expression GenerateLessThan(Expression left, Expression right, MethodInfo comparisonMethodInfo)
            {
                if (IsOpenPropertyExpression(left) || IsOpenPropertyExpression(right))
                {
                    return OpenTypeMethods.LessThanExpression(left, right);
                }
                if (comparisonMethodInfo != null)
                {
                    left = Expression.Call(null, comparisonMethodInfo, left, right);
                    right = Expression.Constant(0, typeof(int));
                }
                return Expression.LessThan(left, right);
            }

            private static Expression GenerateLessThanEqual(Expression left, Expression right, MethodInfo comparisonMethodInfo)
            {
                if (IsOpenPropertyExpression(left) || IsOpenPropertyExpression(right))
                {
                    return OpenTypeMethods.LessThanOrEqualExpression(left, right);
                }
                if (comparisonMethodInfo != null)
                {
                    left = Expression.Call(null, comparisonMethodInfo, left, right);
                    right = Expression.Constant(0, typeof(int));
                }
                return Expression.LessThanOrEqual(left, right);
            }

            private static Expression GenerateLogicalAnd(Expression left, Expression right)
            {
                if (!IsOpenPropertyExpression(left) && !IsOpenPropertyExpression(right))
                {
                    return Expression.AndAlso(left, right);
                }
                return OpenTypeMethods.AndAlsoExpression(left, right);
            }

            private static Expression GenerateLogicalOr(Expression left, Expression right)
            {
                if (!IsOpenPropertyExpression(left) && !IsOpenPropertyExpression(right))
                {
                    return Expression.OrElse(left, right);
                }
                return OpenTypeMethods.OrElseExpression(left, right);
            }

            private static Expression GenerateModulo(Expression left, Expression right)
            {
                if (!IsOpenPropertyExpression(left) && !IsOpenPropertyExpression(right))
                {
                    return Expression.Modulo(left, right);
                }
                return OpenTypeMethods.ModuloExpression(left, right);
            }

            private static Expression GenerateMultiply(Expression left, Expression right)
            {
                if (!IsOpenPropertyExpression(left) && !IsOpenPropertyExpression(right))
                {
                    return Expression.Multiply(left, right);
                }
                return OpenTypeMethods.MultiplyExpression(left, right);
            }

            private static Expression GenerateNegate(Expression expr)
            {
                if (IsOpenPropertyExpression(expr))
                {
                    return OpenTypeMethods.NegateExpression(expr);
                }
                return Expression.Negate(expr);
            }

            private static Expression GenerateNot(Expression expr)
            {
                if (IsOpenPropertyExpression(expr))
                {
                    return OpenTypeMethods.NotExpression(expr);
                }
                if (!(expr.Type == typeof(bool)) && !(expr.Type == typeof(bool?)))
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_NotDoesNotSupportType(expr.Type));
                }
                return Expression.Not(expr);
            }

            private static Expression GenerateNotEqual(Expression left, Expression right)
            {
                if (IsOpenPropertyExpression(left) || IsOpenPropertyExpression(right))
                {
                    return OpenTypeMethods.NotEqualExpression(left, right);
                }
                if (left.Type == typeof(byte[]))
                {
                    return Expression.NotEqual(left, right, false, AreByteArraysNotEqualMethodInfo);
                }
                return Expression.NotEqual(left, right);
            }

            private Expression GenerateNullAwareComparison(Expression left, string rightLiteral, Token op)
            {
                ExpressionLexer l = new ExpressionLexer(rightLiteral);
                Expression expression = this.ParsePrimaryStart(l);
                if (WebUtil.TypeAllowsNull(left.Type))
                {
                    if (!WebUtil.TypeAllowsNull(expression.Type))
                    {
                        expression = Expression.Convert(expression, typeof(Nullable<>).MakeGenericType(new Type[] { expression.Type }));
                    }
                }
                else if (WebUtil.TypeAllowsNull(expression.Type))
                {
                    left = Expression.Convert(left, typeof(Nullable<>).MakeGenericType(new Type[] { left.Type }));
                }
                else
                {
                    return this.GenerateComparisonExpression(left, expression, op);
                }
                switch (op.Text)
                {
                    case "gt":
                        if (left == ExpressionUtils.NullLiteral)
                        {
                            return Expression.Constant(false, typeof(bool));
                        }
                        if (expression == ExpressionUtils.NullLiteral)
                        {
                            return GenerateNotEqual(left, Expression.Constant(null, left.Type));
                        }
                        return GenerateLogicalAnd(GenerateNotEqual(left, Expression.Constant(null, left.Type)), GenerateLogicalOr(GenerateEqual(expression, Expression.Constant(null, expression.Type)), this.GenerateComparisonExpression(left, expression, op)));

                    case "lt":
                        if (expression == ExpressionUtils.NullLiteral)
                        {
                            return Expression.Constant(false, typeof(bool));
                        }
                        if (left == ExpressionUtils.NullLiteral)
                        {
                            return GenerateNotEqual(expression, Expression.Constant(null, expression.Type));
                        }
                        return GenerateLogicalAnd(GenerateNotEqual(expression, Expression.Constant(null, left.Type)), GenerateLogicalOr(GenerateEqual(left, Expression.Constant(null, expression.Type)), this.GenerateComparisonExpression(left, expression, op)));
                }
                throw ParseError(System.Data.Services.Strings.RequestQueryParser_NullOperatorUnsupported(op.Text, op.Position, this.lexer.ExpressionText));
            }

            private static Expression GenerateSubtract(Expression left, Expression right)
            {
                if (!IsOpenPropertyExpression(left) && !IsOpenPropertyExpression(right))
                {
                    return Expression.Subtract(left, right);
                }
                return OpenTypeMethods.SubtractExpression(left, right);
            }

            private static Type GetNonNullableType(Type type)
            {
                return (Nullable.GetUnderlyingType(type) ?? type);
            }

            private static int GetNumericTypeKind(Type type)
            {
                type = GetNonNullableType(type);
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Char:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return 1;

                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        return 2;

                    case TypeCode.Byte:
                        return 3;
                }
                return 0;
            }

            private static Exception IncompatibleOperandsError(string operationName, Expression left, Expression right, int pos)
            {
                return ParseError(System.Data.Services.Strings.RequestQueryParser_IncompatibleOperands(operationName, WebUtil.GetTypeName(left.Type), WebUtil.GetTypeName(right.Type), pos));
            }

            private bool IsApplicable(MethodData method, Expression[] args)
            {
                if (method.Parameters.Length != args.Length)
                {
                    return false;
                }
                Expression[] expressionArray = new Expression[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    ParameterInfo info = method.Parameters[i];
                    Expression expression = this.PromoteExpression(args[i], info.ParameterType, false);
                    if (expression == null)
                    {
                        return false;
                    }
                    expressionArray[i] = expression;
                }
                method.Args = expressionArray;
                return true;
            }

            private static bool IsBetterThan(Expression[] args, IEnumerable<Type> firstCandidate, IEnumerable<Type> secondCandidate)
            {
                bool flag = false;
                using (IEnumerator<Type> enumerator = firstCandidate.GetEnumerator())
                {
                    using (IEnumerator<Type> enumerator2 = secondCandidate.GetEnumerator())
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            enumerator.MoveNext();
                            enumerator2.MoveNext();
                            int num2 = CompareConversions(args[i].Type, enumerator.Current, enumerator2.Current);
                            if (num2 < 0)
                            {
                                return false;
                            }
                            if (num2 > 0)
                            {
                                flag = true;
                            }
                        }
                    }
                }
                return flag;
            }

            private static bool IsBetterThan(Expression[] args, MethodData m1, MethodData m2)
            {
                return IsBetterThan(args, m1.ParameterTypes, m2.ParameterTypes);
            }

            private static bool IsCompatibleWith(Type source, Type target)
            {
                if (source == target)
                {
                    return true;
                }
                if (!target.IsValueType)
                {
                    return target.IsAssignableFrom(source);
                }
                Type nonNullableType = GetNonNullableType(source);
                Type type = GetNonNullableType(target);
                TypeCode code = nonNullableType.IsEnum ? TypeCode.Object : Type.GetTypeCode(nonNullableType);
                TypeCode code2 = type.IsEnum ? TypeCode.Object : Type.GetTypeCode(type);
                switch (code)
                {
                    case TypeCode.SByte:
                        switch (code2)
                        {
                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Byte:
                        switch (code2)
                        {
                            case TypeCode.Byte:
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Int16:
                        switch (code2)
                        {
                            case TypeCode.Int16:
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Int32:
                        switch (code2)
                        {
                            case TypeCode.Int32:
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Int64:
                        switch (code2)
                        {
                            case TypeCode.Int64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                return true;
                        }
                        break;

                    case TypeCode.Single:
                        switch (code2)
                        {
                            case TypeCode.Single:
                            case TypeCode.Double:
                                return true;
                        }
                        break;

                    default:
                        if (nonNullableType == type)
                        {
                            return true;
                        }
                        break;
                }
                return (target == typeof(object));
            }

            private static bool IsOpenExpression(Expression input)
            {
                input = StripObjectConvert(input);
                switch (input.NodeType)
                {
                    case ExpressionType.Subtract:
                    case ExpressionType.Add:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.NotEqual:
                    case ExpressionType.OrElse:
                    {
                        BinaryExpression expression2 = (BinaryExpression) input;
                        return ((expression2.Method != null) && (expression2.Method.DeclaringType == typeof(OpenTypeMethods)));
                    }
                    case ExpressionType.TypeIs:
                    case ExpressionType.Constant:
                    case ExpressionType.Convert:
                    case ExpressionType.MemberAccess:
                        return false;

                    case ExpressionType.Call:
                        return (((MethodCallExpression) input).Method.DeclaringType == typeof(OpenTypeMethods));

                    case ExpressionType.Conditional:
                        return IsOpenExpression(((ConditionalExpression) input).IfFalse);

                    case ExpressionType.Negate:
                    case ExpressionType.Not:
                    {
                        UnaryExpression expression = (UnaryExpression) input;
                        return ((expression.Method != null) && (expression.Method.DeclaringType == typeof(OpenTypeMethods)));
                    }
                }
                return false;
            }

            private static bool IsOpenPropertyExpression(Expression expression)
            {
                return (((expression != ExpressionUtils.NullLiteral) && (expression.Type == typeof(object))) && IsOpenExpression(expression));
            }

            private static bool IsSignedIntegralType(Type type)
            {
                return (GetNumericTypeKind(type) == 2);
            }

            private static bool IsUnsignedIntegralType(Type type)
            {
                return (GetNumericTypeKind(type) == 3);
            }

            private Expression ParseAdditive()
            {
                this.RecurseEnter();
                Expression left = this.ParseMultiplicative();
                while (this.CurrentToken.IdentifierIs("add") || this.CurrentToken.IdentifierIs("sub"))
                {
                    Token currentToken = this.CurrentToken;
                    this.lexer.NextToken();
                    Expression right = this.ParseMultiplicative();
                    if (currentToken.IdentifierIs("add"))
                    {
                        this.CheckAndPromoteOperands(typeof(OperationSignatures.IAddSignatures), currentToken.Text, ref left, ref right, currentToken.Position);
                        left = GenerateAdd(left, right);
                    }
                    else
                    {
                        this.CheckAndPromoteOperands(typeof(OperationSignatures.ISubtractSignatures), currentToken.Text, ref left, ref right, currentToken.Position);
                        left = GenerateSubtract(left, right);
                    }
                }
                this.RecurseLeave();
                return left;
            }

            private Expression ParseAnyAll(Expression source, string functionName, ResourceType resourceType, ResourceSetWrapper container)
            {
                Type iEnumerableElement = BaseServiceProvider.GetIEnumerableElement(source.Type);
                if (this.lexer.CurrentToken.Id == TokenId.CloseParen)
                {
                    if (functionName == "all")
                    {
                        throw ParseError(System.Data.Services.Strings.RequestQueryParser_AllWithoutAPredicateIsNotSupported(this.lexer.Position));
                    }
                    this.lexer.NextToken();
                    return source.EnumerableAny();
                }
                string identifier = this.lexer.CurrentToken.GetIdentifier();
                this.lexer.NextToken();
                this.lexer.ValidateToken(TokenId.Colon);
                this.lexer.NextToken();
                int position = this.lexer.Position;
                ParameterExpression parameter = Expression.Parameter(iEnumerableElement, identifier);
                if (resourceType.ResourceTypeKind == ResourceTypeKind.Collection)
                {
                    resourceType = ((CollectionResourceType) resourceType).ItemType;
                }
                SegmentTypeInfo info = new SegmentTypeInfo(resourceType, container, parameter, false, false);
                if (this.parameterMap.ContainsKey(identifier))
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_DeclaredRangeVariable(identifier));
                }
                this.parameterMap.Add(identifier, info);
                Expression body = PreparePredicateExpressionForLambda(this.ParseExpression());
                if (body.Type != typeof(bool))
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_ExpressionTypeMismatch(WebUtil.GetTypeName(typeof(bool)), position));
                }
                this.parameterMap.Remove(identifier);
                this.lexer.ValidateToken(TokenId.CloseParen);
                this.lexer.NextToken();
                LambdaExpression predicate = Expression.Lambda(body, new ParameterExpression[] { parameter });
                if (functionName == "any")
                {
                    return source.EnumerableAny(predicate);
                }
                return source.EnumerableAll(predicate);
            }

            private Expression[] ParseArgumentList()
            {
                if (this.CurrentToken.Id != TokenId.OpenParen)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_OpenParenExpected(this.CurrentToken.Position));
                }
                this.lexer.NextToken();
                Expression[] expressionArray = (this.CurrentToken.Id != TokenId.CloseParen) ? this.ParseArguments() : emptyExpressions;
                if (this.CurrentToken.Id != TokenId.CloseParen)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_CloseParenOrCommaExpected(this.CurrentToken.Position));
                }
                this.lexer.NextToken();
                return expressionArray;
            }

            private Expression[] ParseArguments()
            {
                List<Expression> list = new List<Expression>();
                while (true)
                {
                    list.Add(this.ParseExpression());
                    if (this.CurrentToken.Id != TokenId.Comma)
                    {
                        break;
                    }
                    this.lexer.NextToken();
                }
                return list.ToArray();
            }

            private Expression ParseComparison()
            {
                this.RecurseEnter();
                Expression left = this.ParseAdditive();
                while (this.CurrentToken.IsComparisonOperator)
                {
                    Token currentToken = this.CurrentToken;
                    this.lexer.NextToken();
                    Expression right = this.ParseAdditive();
                    left = this.GenerateComparisonExpression(left, right, currentToken);
                }
                this.RecurseLeave();
                return left;
            }

            private static Exception ParseError(string message)
            {
                return DataServiceException.CreateSyntaxError(message);
            }

            private Expression ParseExpression()
            {
                this.RecurseEnter();
                Expression expression = this.ParseLogicalOr();
                this.RecurseLeave();
                return expression;
            }

            private Expression ParseIdentifier()
            {
                this.ValidateToken(TokenId.Identifier);
                if (this.lexer.ExpandIdentifierAsFunction())
                {
                    return this.ParseIdentifierAsFunction();
                }
                if (this.InsideSubScope)
                {
                    return this.ParseSubScopeMemberAccess();
                }
                this.currentSegmentInfo = this.parameterMap["$it"];
                return this.ParseMemberAccess(this.it);
            }

            private Expression ParseIdentifierAsFunction()
            {
                FunctionDescription[] descriptionArray;
                Expression expression3;
                Token currentToken = this.CurrentToken;
                if (!functions.TryGetValue(currentToken.Text, out descriptionArray))
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_UnknownFunction(currentToken.Text, currentToken.Position));
                }
                this.lexer.NextToken();
                Expression[] expressionArray = this.ParseArgumentList();
                Expression[] arguments = this.nullPropagationRequired ? expressionArray : ((Expression[]) expressionArray.Clone());
                Expression expression = (arguments.Length > 1) ? arguments[1] : ((arguments.Length > 0) ? arguments[0] : null);
                bool flag = false;
                if (!descriptionArray[0].IsTypeCheckOrCast)
                {
                    foreach (Expression expression2 in arguments)
                    {
                        if (IsOpenPropertyExpression(expression2))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                FunctionDescription description = null;
                if (!flag)
                {
                    Expression[] expressionArray4;
                    description = this.FindBestFunction(descriptionArray, ref arguments);
                    if (description == null)
                    {
                        throw ParseError(System.Data.Services.Strings.RequestQueryParser_NoApplicableFunction(currentToken.Text, currentToken.Position, FunctionDescription.BuildSignatureList(currentToken.Text, descriptionArray)));
                    }
                    if ((this.InsideSubScope && description.IsTypeCheckOrCast) && (arguments.Length == 1))
                    {
                        throw ParseError(System.Data.Services.Strings.RequestQueryParser_UnaryTypeCheckOrTypeCastNotSupportedInSubScope(currentToken.Text, currentToken.Position));
                    }
                    if (this.nullPropagationRequired && description.IsTypeCast)
                    {
                        Expression expression4 = arguments[arguments.Length - 1];
                        if (expression4.Type == typeof(Type))
                        {
                            Type type = (Type) ((ConstantExpression) expression4).Value;
                            if (!WebUtil.TypeAllowsNull(type))
                            {
                                arguments[arguments.Length - 1] = Expression.Constant(typeof(Nullable<>).MakeGenericType(new Type[] { type }));
                            }
                        }
                    }
                    if ((description.ConversionFunction == new Func<Expression, Expression[], Expression>(FunctionDescription.BinaryIsOfResourceType)) || (description.ConversionFunction == new Func<Expression, Expression[], Expression>(FunctionDescription.BinaryCastResourceType)))
                    {
                        expressionArray4 = new Expression[arguments.Length + 1];
                        for (int i = 0; i < arguments.Length; i++)
                        {
                            expressionArray4[i] = arguments[i];
                        }
                        expressionArray4[arguments.Length] = Expression.Constant(IsOpenPropertyExpression(arguments[0]), typeof(bool));
                    }
                    else
                    {
                        expressionArray4 = arguments;
                    }
                    expression3 = description.ConversionFunction(this.it, expressionArray4);
                }
                else
                {
                    foreach (FunctionDescription description2 in descriptionArray)
                    {
                        if (description2.ParameterTypes.Length == arguments.Length)
                        {
                            description = description2;
                            break;
                        }
                    }
                    Expression[] expressionArray3 = new Expression[arguments.Length];
                    for (int j = 0; j < expressionArray3.Length; j++)
                    {
                        if (IsOpenPropertyExpression(arguments[j]))
                        {
                            expressionArray3[j] = arguments[j];
                        }
                        else
                        {
                            expressionArray3[j] = Expression.Convert(arguments[j], typeof(object));
                        }
                    }
                    if (description == null)
                    {
                        throw ParseError(System.Data.Services.Strings.RequestQueryParser_NoApplicableFunction(currentToken.Text, currentToken.Position, FunctionDescription.BuildSignatureList(currentToken.Text, descriptionArray)));
                    }
                    expression3 = description.InvokeOpenTypeMethod(expressionArray3);
                }
                if (this.nullPropagationRequired && description.RequiresNullPropagation)
                {
                    for (int k = 0; k < expressionArray.Length; k++)
                    {
                        expression3 = this.ConsiderNullPropagation(expressionArray[k], expression3);
                    }
                }
                if (description.IsTypeCast)
                {
                    this.currentSegmentInfo = new SegmentTypeInfo(WebUtil.TryResolveResourceType(this.provider, (string) ((ConstantExpression) expression).Value), this.currentSegmentInfo.ResourceSet, this.currentSegmentInfo.Parameter, this.currentSegmentInfo.IsCollection, false);
                }
                return expression3;
            }

            private Expression ParseLogicalAnd()
            {
                this.RecurseEnter();
                Expression left = this.ParseComparison();
                while (this.TokenIdentifierIs("and"))
                {
                    Token currentToken = this.CurrentToken;
                    this.lexer.NextToken();
                    Expression right = this.ParseComparison();
                    this.CheckAndPromoteOperands(typeof(OperationSignatures.ILogicalSignatures), currentToken.Text, ref left, ref right, currentToken.Position);
                    left = GenerateLogicalAnd(left, right);
                }
                this.RecurseLeave();
                return left;
            }

            private Expression ParseLogicalOr()
            {
                this.RecurseEnter();
                Expression left = this.ParseLogicalAnd();
                while (this.TokenIdentifierIs("or"))
                {
                    Token currentToken = this.CurrentToken;
                    this.lexer.NextToken();
                    Expression right = this.ParseLogicalAnd();
                    this.CheckAndPromoteOperands(typeof(OperationSignatures.ILogicalSignatures), currentToken.Text, ref left, ref right, currentToken.Position);
                    left = GenerateLogicalOr(left, right);
                }
                this.RecurseLeave();
                return left;
            }

            private Expression ParseMemberAccess(Expression instance)
            {
                int position = this.lexer.Position;
                string str = this.lexer.ReadDottedIdentifier();
                if (this.currentSegmentInfo.IsCollection)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_DisallowMemberAccessForResourceSetReference(str, position));
                }
                ResourceProperty navigationProperty = (this.currentSegmentInfo.ResourceType == null) ? null : this.currentSegmentInfo.ResourceType.TryResolvePropertyName(str, ResourcePropertyKind.Stream);
                ResourceSetWrapper container = (((this.currentSegmentInfo.ResourceSet == null) || (navigationProperty == null)) || (navigationProperty.TypeKind != ResourceTypeKind.EntityType)) ? null : this.provider.GetContainer(this.currentSegmentInfo.ResourceSet, this.currentSegmentInfo.ResourceType, navigationProperty);
                if (navigationProperty != null)
                {
                    Expression expression;
                    if ((navigationProperty.TypeKind == ResourceTypeKind.EntityType) && (container == null))
                    {
                        throw DataServiceException.CreateBadRequestError(System.Data.Services.Strings.BadRequest_InvalidPropertyNameSpecified(str, this.currentSegmentInfo.ResourceType.FullName));
                    }
                    if (navigationProperty.CanReflectOnInstanceTypeProperty)
                    {
                        expression = Expression.Property(instance, this.currentSegmentInfo.ResourceType.GetPropertyInfo(navigationProperty));
                    }
                    else
                    {
                        expression = Expression.Convert(Expression.Call(null, DataServiceProviderMethods.GetValueMethodInfo, instance, Expression.Constant(navigationProperty)), navigationProperty.Type);
                    }
                    Expression expression2 = this.ConsiderNullPropagation(instance, expression);
                    if (container != null)
                    {
                        bool singleResult = navigationProperty.Kind == ResourcePropertyKind.ResourceReference;
                        DataServiceConfiguration.CheckResourceRightsForRead(container, singleResult);
                        Expression expression3 = DataServiceConfiguration.ComposeQueryInterceptors(this.service, container);
                        if (expression3 != null)
                        {
                            expression2 = RequestQueryProcessor.ComposePropertyNavigation(expression2, (LambdaExpression) expression3, this.provider.NullPropagationRequired, singleResult);
                        }
                    }
                    if ((navigationProperty.Kind == ResourcePropertyKind.Collection) || (navigationProperty.Kind == ResourcePropertyKind.ResourceSetReference))
                    {
                        if (((this.queryOptionGettingParsed == "$filter") && (this.CurrentToken.Id == TokenId.Slash)) && this.service.Configuration.DataServiceBehavior.AcceptAnyAllRequests)
                        {
                            position = this.lexer.Position;
                            this.lexer.NextToken();
                            Expression expression4 = this.TryParseAnyAll(expression2, navigationProperty.ResourceType, container, false);
                            if (expression4 != null)
                            {
                                return expression4;
                            }
                        }
                        if (navigationProperty.Kind == ResourcePropertyKind.Collection)
                        {
                            throw ParseError(System.Data.Services.Strings.RequestQueryParser_CollectionPropertyNotSupportedInExpression(navigationProperty.Name));
                        }
                    }
                    this.currentSegmentInfo = new SegmentTypeInfo(navigationProperty.ResourceType, container, this.currentSegmentInfo.Parameter, navigationProperty.Kind == ResourcePropertyKind.ResourceSetReference, false);
                    return expression2;
                }
                ResourceType resourceType = WebUtil.ResolveTypeIdentifier(this.provider, str, this.currentSegmentInfo.ResourceType, this.currentSegmentInfo.IsTypeIdentifier);
                if (resourceType != null)
                {
                    bool isTypeIdentifier = true;
                    this.currentSegmentInfo = new SegmentTypeInfo(resourceType, this.currentSegmentInfo.ResourceSet, this.currentSegmentInfo.Parameter, this.currentSegmentInfo.IsCollection, isTypeIdentifier);
                    Expression expression5 = ExpressionUtils.GenerateTypeAsExpression(instance, resourceType);
                    if (this.CurrentToken.Id != TokenId.Slash)
                    {
                        throw ParseError(System.Data.Services.Strings.RequestQueryProcessor_QueryParametersPathCannotEndInTypeIdentifier(this.queryOptionGettingParsed, resourceType.FullName));
                    }
                    this.requestDescription.VerifyProtocolVersion(RequestDescription.Version3Dot0, this.service);
                    this.lexer.NextToken();
                    return this.ParseMemberAccess(expression5);
                }
                if (str.Contains<char>('.'))
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_UnknownResourceType(str, position));
                }
                resourceType = this.currentSegmentInfo.ResourceType;
                this.currentSegmentInfo = new SegmentTypeInfo(null, null, this.currentSegmentInfo.Parameter, false, false);
                if ((resourceType != null) && !resourceType.IsOpenType)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_UnknownProperty(str, resourceType.FullName, position));
                }
                Expression notNullExpression = Expression.Call(null, OpenTypeMethods.GetValueOpenPropertyMethodInfo, instance, Expression.Constant(str));
                return this.ConsiderNullPropagation(instance, notNullExpression);
            }

            private Expression ParseMultiplicative()
            {
                this.RecurseEnter();
                Expression left = this.ParseUnary();
                while ((this.CurrentToken.IdentifierIs("mul") || this.CurrentToken.IdentifierIs("div")) || this.CurrentToken.IdentifierIs("mod"))
                {
                    Token currentToken = this.CurrentToken;
                    this.lexer.NextToken();
                    Expression right = this.ParseUnary();
                    this.CheckAndPromoteOperands(typeof(OperationSignatures.IArithmeticSignatures), currentToken.Text, ref left, ref right, currentToken.Position);
                    if (currentToken.IdentifierIs("mul"))
                    {
                        left = GenerateMultiply(left, right);
                    }
                    else
                    {
                        if (currentToken.IdentifierIs("div"))
                        {
                            left = GenerateDivide(left, right);
                            continue;
                        }
                        left = GenerateModulo(left, right);
                    }
                }
                this.RecurseLeave();
                return left;
            }

            private static Expression ParseNullLiteral(ExpressionLexer l)
            {
                l.NextToken();
                return ExpressionUtils.NullLiteral;
            }

            private static object ParseNumber(string text, Type type)
            {
                switch (Type.GetTypeCode(GetNonNullableType(type)))
                {
                    case TypeCode.SByte:
                        sbyte num;
                        if (!sbyte.TryParse(text, out num))
                        {
                            break;
                        }
                        return num;

                    case TypeCode.Byte:
                        byte num2;
                        if (!byte.TryParse(text, out num2))
                        {
                            break;
                        }
                        return num2;

                    case TypeCode.Int16:
                        short num3;
                        if (!short.TryParse(text, out num3))
                        {
                            break;
                        }
                        return num3;

                    case TypeCode.Int32:
                        int num4;
                        if (!int.TryParse(text, out num4))
                        {
                            break;
                        }
                        return num4;

                    case TypeCode.Int64:
                        long num5;
                        if (!long.TryParse(text, out num5))
                        {
                            break;
                        }
                        return num5;

                    case TypeCode.Single:
                        float num6;
                        if (!float.TryParse(text, out num6))
                        {
                            break;
                        }
                        return num6;

                    case TypeCode.Double:
                        double num7;
                        if (!double.TryParse(text, out num7))
                        {
                            break;
                        }
                        return num7;

                    case TypeCode.Decimal:
                        decimal num8;
                        if (!decimal.TryParse(text, out num8))
                        {
                            break;
                        }
                        return num8;
                }
                return null;
            }

            internal IEnumerable<OrderingExpression> ParseOrdering()
            {
                Expression expression;
                this.queryOptionGettingParsed = "$orderby";
                List<OrderingExpression> list = new List<OrderingExpression>();
            Label_0011:
                expression = this.ParseExpression();
                bool isAscending = true;
                if (this.TokenIdentifierIs("asc"))
                {
                    this.lexer.NextToken();
                }
                else if (this.TokenIdentifierIs("desc"))
                {
                    this.lexer.NextToken();
                    isAscending = false;
                }
                list.Add(new OrderingExpression(expression, isAscending));
                if (this.CurrentToken.Id == TokenId.Comma)
                {
                    this.lexer.NextToken();
                    goto Label_0011;
                }
                this.ValidateToken(TokenId.End);
                return list;
            }

            private Expression ParseParenExpression()
            {
                if (this.CurrentToken.Id != TokenId.OpenParen)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_OpenParenExpected(this.CurrentToken.Position));
                }
                this.lexer.NextToken();
                Expression expression = this.ParseExpression();
                if (this.CurrentToken.Id != TokenId.CloseParen)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_CloseParenOrOperatorExpected(this.CurrentToken.Position));
                }
                this.lexer.NextToken();
                return expression;
            }

            private Expression ParsePrimary()
            {
                this.RecurseEnter();
                Expression instance = this.ParsePrimaryStart(this.lexer);
                while (this.CurrentToken.Id == TokenId.Slash)
                {
                    this.lexer.NextToken();
                    instance = this.ParseMemberAccess(instance);
                }
                this.RecurseLeave();
                return instance;
            }

            private Expression ParsePrimaryStart(ExpressionLexer l)
            {
                switch (l.CurrentToken.Id)
                {
                    case TokenId.Identifier:
                        return this.ParseIdentifier();

                    case TokenId.NullLiteral:
                        return ParseNullLiteral(l);

                    case TokenId.BooleanLiteral:
                        return this.ParseTypedLiteral(typeof(bool), "Edm.Boolean", l);

                    case TokenId.StringLiteral:
                        return this.ParseTypedLiteral(typeof(string), "Edm.String", l);

                    case TokenId.IntegerLiteral:
                        return this.ParseTypedLiteral(typeof(int), "Edm.Int32", l);

                    case TokenId.Int64Literal:
                        return this.ParseTypedLiteral(typeof(long), "Edm.Int64", l);

                    case TokenId.SingleLiteral:
                        return this.ParseTypedLiteral(typeof(float), "Edm.Single", l);

                    case TokenId.DateTimeLiteral:
                        return this.ParseTypedLiteral(typeof(DateTime), "Edm.DateTime", l);

                    case TokenId.DecimalLiteral:
                        return this.ParseTypedLiteral(typeof(decimal), "Edm.Decimal", l);

                    case TokenId.DoubleLiteral:
                        return this.ParseTypedLiteral(typeof(double), "Edm.Double", l);

                    case TokenId.GuidLiteral:
                        return this.ParseTypedLiteral(typeof(Guid), "Edm.Guid", l);

                    case TokenId.BinaryLiteral:
                        return this.ParseTypedLiteral(typeof(byte[]), "Edm.Binary", l);

                    case TokenId.DateTimeOffsetLiteral:
                        return this.ParseTypedLiteral(typeof(DateTimeOffset), "Edm.DateTimeOffset", l);

                    case TokenId.TimeLiteral:
                        return this.ParseTypedLiteral(typeof(TimeSpan), "Edm.Time", l);

                    case TokenId.OpenParen:
                        return this.ParseParenExpression();

                    case TokenId.GeographylLiteral:
                        return this.ParseWellKnownText<Geography>("geography", l);

                    case TokenId.GeometryLiteral:
                        return this.ParseWellKnownText<Geometry>("geometry", l);
                }
                throw ParseError(System.Data.Services.Strings.RequestQueryParser_ExpressionExpected(l.CurrentToken.Position));
            }

            internal object ParseSkipTokenLiteral(string literal)
            {
                this.queryOptionGettingParsed = "$skiptoken";
                ExpressionLexer l = new ExpressionLexer(literal);
                Expression expression = this.ParsePrimaryStart(l);
                if (expression.NodeType != ExpressionType.Constant)
                {
                    throw new InvalidOperationException(System.Data.Services.Strings.RequsetQueryParser_ExpectingLiteralInSkipToken(literal));
                }
                return ((ConstantExpression) expression).Value;
            }

            private Expression ParseSubScopeMemberAccess()
            {
                SegmentTypeInfo info;
                string identifier = this.lexer.CurrentToken.GetIdentifier();
                this.lexer.NextToken();
                if (!this.parameterMap.TryGetValue(identifier, out info))
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_UnknownIdentifier(identifier));
                }
                ParameterExpression parameter = info.Parameter;
                if (this.lexer.CurrentToken.Id == TokenId.Slash)
                {
                    this.lexer.NextToken();
                    this.currentSegmentInfo = info;
                    return this.ParseMemberAccess(parameter);
                }
                return parameter;
            }

            private Expression ParseTypedLiteral(Type targetType, string targetTypeName, ExpressionLexer l)
            {
                object obj2;
                if (!WebConvert.TryKeyStringToPrimitive(l.CurrentToken.Text, targetType, out obj2))
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_UnrecognizedLiteral(targetTypeName, l.CurrentToken.Text, l.CurrentToken.Position));
                }
                Expression expression = this.CreateLiteral(obj2, l.CurrentToken.Text, targetType);
                l.NextToken();
                return expression;
            }

            private Expression ParseUnary()
            {
                this.RecurseEnter();
                if ((this.CurrentToken.Id == TokenId.Minus) || this.CurrentToken.IdentifierIs("not"))
                {
                    Token currentToken = this.CurrentToken;
                    this.lexer.NextToken();
                    if ((currentToken.Id == TokenId.Minus) && ExpressionLexer.IsNumeric(this.CurrentToken.Id))
                    {
                        Token token2 = this.CurrentToken;
                        token2.Text = "-" + token2.Text;
                        token2.Position = currentToken.Position;
                        this.CurrentToken = token2;
                        this.RecurseLeave();
                        return this.ParsePrimary();
                    }
                    Expression expr = this.ParseUnary();
                    if (currentToken.Id == TokenId.Minus)
                    {
                        this.CheckAndPromoteOperand(typeof(OperationSignatures.INegationSignatures), currentToken.Text, ref expr, currentToken.Position);
                        expr = GenerateNegate(expr);
                    }
                    else
                    {
                        this.CheckAndPromoteOperand(typeof(OperationSignatures.INotSignatures), currentToken.Text, ref expr, currentToken.Position);
                        expr = GenerateNot(expr);
                    }
                    this.RecurseLeave();
                    return expr;
                }
                this.RecurseLeave();
                return this.ParsePrimary();
            }

            private Expression ParseWellKnownText<T>(string prefix, ExpressionLexer l) where T: class, ISpatial
            {
                if (!this.service.Configuration.DataServiceBehavior.AcceptSpatialLiteralsInQuery)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_SpatialNotSupported);
                }
                string text = l.CurrentToken.Text;
                if (this.wellKnownTextFormatter == null)
                {
                    this.wellKnownTextFormatter = WellKnownTextSqlFormatter.Create(true);
                }
                T spatialValue = WebConvert.ParseSpatialLiteral<T>(text, prefix, this.wellKnownTextFormatter);
                Type publicSpatialBaseType = null;
                TryGetPublicSpatialBaseType(spatialValue, out publicSpatialBaseType);
                Expression expression = this.CreateLiteral(spatialValue, l.CurrentToken.Text, publicSpatialBaseType);
                l.NextToken();
                return expression;
            }

            internal Expression ParseWhere()
            {
                this.queryOptionGettingParsed = "$filter";
                int position = this.lexer.Position;
                Expression expression = PreparePredicateExpressionForLambda(this.ParseExpression());
                if (expression.Type != typeof(bool))
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_ExpressionTypeMismatch(WebUtil.GetTypeName(typeof(bool)), position));
                }
                this.lexer.ValidateToken(TokenId.End);
                return expression;
            }

            private static Expression PreparePredicateExpressionForLambda(Expression expr)
            {
                if (IsOpenPropertyExpression(expr))
                {
                    expr = OpenTypeMethods.EqualExpression(expr, Expression.Constant(true, typeof(object)));
                    expr = Expression.Convert(expr, typeof(bool));
                    return expr;
                }
                if (ExpressionUtils.IsNullConstant(expr))
                {
                    expr = falseLiteral;
                    return expr;
                }
                if (expr.Type == typeof(bool?))
                {
                    expr = Expression.Condition(Expression.Equal(expr, Expression.Constant(null, typeof(bool?))), falseLiteral, Expression.Property(expr, "Value"));
                }
                return expr;
            }

            private Expression PromoteExpression(Expression expr, Type type, bool exact)
            {
                if (expr.Type == type)
                {
                    return expr;
                }
                ConstantExpression key = expr as ConstantExpression;
                if (key != null)
                {
                    if (key == ExpressionUtils.NullLiteral)
                    {
                        if (WebUtil.TypeAllowsNull(type))
                        {
                            return Expression.Constant(null, type);
                        }
                    }
                    else
                    {
                        string str;
                        if (this.literals.TryGetValue(key, out str))
                        {
                            Type nonNullableType = GetNonNullableType(type);
                            object instanceType = null;
                            if ((key.Type == typeof(string)) && ((nonNullableType == typeof(Type)) || (nonNullableType == typeof(ResourceType))))
                            {
                                if (WebConvert.TryRemoveQuotes(ref str))
                                {
                                    ResourceType type3 = WebUtil.TryResolveResourceType(this.provider, str);
                                    if (type3 != null)
                                    {
                                        if (nonNullableType == typeof(Type))
                                        {
                                            if (type3.CanReflectOnInstanceType)
                                            {
                                                instanceType = type3.InstanceType;
                                            }
                                        }
                                        else if (!type3.CanReflectOnInstanceType)
                                        {
                                            instanceType = type3;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                switch (Type.GetTypeCode(key.Type))
                                {
                                    case TypeCode.Int32:
                                    case TypeCode.Int64:
                                        instanceType = ParseNumber(str, nonNullableType);
                                        break;

                                    case TypeCode.Double:
                                        if (nonNullableType == typeof(decimal))
                                        {
                                            instanceType = ParseNumber(str, nonNullableType);
                                        }
                                        break;
                                }
                            }
                            if (instanceType != null)
                            {
                                return Expression.Constant(instanceType, type);
                            }
                        }
                    }
                }
                if (IsCompatibleWith(expr.Type, type))
                {
                    if (!type.IsValueType && (!exact || (!(type != typeof(object)) && !expr.Type.IsValueType)))
                    {
                        return expr;
                    }
                    return Expression.Convert(expr, type);
                }
                if (WebUtil.IsNullableType(expr.Type) && type.IsValueType)
                {
                    Expression expression2 = Expression.Property(expr, "Value");
                    return this.PromoteExpression(expression2, type, exact);
                }
                return null;
            }

            private void RecurseEnter()
            {
                WebUtil.RecurseEnterQueryParser(800, ref this.recursionDepth);
            }

            private void RecurseLeave()
            {
                WebUtil.RecurseLeave(ref this.recursionDepth);
            }

            private static IEnumerable<Type> SelfAndBaseClasses(Type type)
            {
                while (true)
                {
                    if (type == null)
                    {
                        yield break;
                    }
                    yield return type;
                    type = type.BaseType;
                }
            }

            private static IEnumerable<Type> SelfAndBaseTypes(Type type)
            {
                if (type.IsInterface)
                {
                    List<Type> types = new List<Type>();
                    AddInterface(types, type);
                    return types;
                }
                return SelfAndBaseClasses(type);
            }

            private static Expression StripObjectConvert(Expression input)
            {
                while ((input.NodeType == ExpressionType.Convert) && (input.Type == typeof(object)))
                {
                    UnaryExpression expression = (UnaryExpression) input;
                    input = expression.Operand;
                }
                return input;
            }

            private bool TokenIdentifierIs(string id)
            {
                return this.CurrentToken.IdentifierIs(id);
            }

            private static bool TryGetPublicSpatialBaseType(object spatialValue, out Type publicSpatialBaseType)
            {
                publicSpatialBaseType = spatialValue.GetType();
                while (!publicSpatialBaseType.IsPublic && (publicSpatialBaseType != null))
                {
                    publicSpatialBaseType = publicSpatialBaseType.BaseType;
                }
                return (publicSpatialBaseType != null);
            }

            private Expression TryParseAnyAll(Expression source, ResourceType resourceType, ResourceSetWrapper container, bool expressionIsOfType = false)
            {
                if (this.CurrentToken.Id == TokenId.Identifier)
                {
                    int position = this.lexer.Position;
                    string functionName = this.lexer.ReadDottedIdentifier();
                    switch (functionName)
                    {
                        case "any":
                        case "all":
                            if (this.lexer.CurrentToken.Id == TokenId.OpenParen)
                            {
                                this.requestDescription.VerifyProtocolVersion(RequestDescription.Version3Dot0, this.service);
                                this.requestDescription.VerifyRequestVersion(RequestDescription.Version3Dot0, this.service);
                                this.lexer.NextToken();
                                if (!expressionIsOfType)
                                {
                                    source = this.ConvertNullCollectionToEmpty(source);
                                }
                                return this.ParseAnyAll(source, functionName, resourceType, container);
                            }
                            break;

                        default:
                        {
                            ResourceType targetResourceType = WebUtil.ResolveTypeIdentifier(this.provider, functionName, resourceType, expressionIsOfType);
                            if (targetResourceType != null)
                            {
                                if (this.CurrentToken.Id != TokenId.Slash)
                                {
                                    throw ParseError(System.Data.Services.Strings.RequestQueryProcessor_QueryParametersPathCannotEndInTypeIdentifier(this.queryOptionGettingParsed, targetResourceType.FullName));
                                }
                                this.lexer.NextToken();
                                Expression expression = this.CreateTypeFilterExpression(source, targetResourceType);
                                return this.TryParseAnyAll(expression, targetResourceType, container, true);
                            }
                            break;
                        }
                    }
                    if (resourceType.ResourceTypeKind == ResourceTypeKind.EntityType)
                    {
                        throw ParseError(System.Data.Services.Strings.RequestQueryParser_DisallowMemberAccessForResourceSetReference(functionName, position));
                    }
                }
                return null;
            }

            private void ValidateToken(TokenId t)
            {
                if (this.CurrentToken.Id != t)
                {
                    throw ParseError(System.Data.Services.Strings.RequestQueryParser_SyntaxError(this.CurrentToken.Position));
                }
            }

            private Token CurrentToken
            {
                get
                {
                    return this.lexer.CurrentToken;
                }
                set
                {
                    this.lexer.CurrentToken = value;
                }
            }

            private bool InsideSubScope
            {
                get
                {
                    return (this.parameterMap.Count > 1);
                }
            }

            

            [DebuggerDisplay("MethodData {methodBase}")]
            private class MethodData
            {
                private Expression[] args;
                private readonly System.Reflection.MethodBase methodBase;
                private readonly ParameterInfo[] parameters;

                public MethodData(System.Reflection.MethodBase method, ParameterInfo[] parameters)
                {
                    this.methodBase = method;
                    this.parameters = parameters;
                }

                public Expression[] Args
                {
                    get
                    {
                        return this.args;
                    }
                    set
                    {
                        this.args = value;
                    }
                }

                public System.Reflection.MethodBase MethodBase
                {
                    get
                    {
                        return this.methodBase;
                    }
                }

                public ParameterInfo[] Parameters
                {
                    get
                    {
                        return this.parameters;
                    }
                }

                public IEnumerable<Type> ParameterTypes
                {
                    get
                    {
                        foreach (ParameterInfo iteratorVariable0 in this.Parameters)
                        {
                            yield return iteratorVariable0.ParameterType;
                        }
                    }
                }

                
            }

            private class SegmentTypeInfo
            {
                public SegmentTypeInfo(System.Data.Services.Providers.ResourceType resourceType, ResourceSetWrapper resourceSet, ParameterExpression parameter, bool isCollection, bool isTypeIdentifier = false)
                {
                    this.ResourceType = resourceType;
                    this.ResourceSet = resourceSet;
                    this.Parameter = parameter;
                    this.IsCollection = isCollection;
                    this.IsTypeIdentifier = isTypeIdentifier;
                }

                public bool IsCollection { get; private set; }

                public bool IsTypeIdentifier { get; private set; }

                public ParameterExpression Parameter { get; private set; }

                public ResourceSetWrapper ResourceSet { get; private set; }

                public System.Data.Services.Providers.ResourceType ResourceType { get; private set; }
            }
        }
    }
}

