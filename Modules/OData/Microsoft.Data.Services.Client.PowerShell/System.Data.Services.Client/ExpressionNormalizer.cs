namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client.Metadata;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class ExpressionNormalizer : DataServiceALinqExpressionVisitor
    {
        private readonly Dictionary<Expression, Pattern> _patterns = new Dictionary<Expression, Pattern>(ReferenceEqualityComparer<Expression>.Instance);
        private const bool LiftToNull = false;
        private readonly Dictionary<Expression, Expression> normalizerRewrites;
        private static readonly MethodInfo s_relationalOperatorPlaceholderMethod = typeof(ExpressionNormalizer).GetMethod("RelationalOperatorPlaceholder", false, true);

        private ExpressionNormalizer(Dictionary<Expression, Expression> normalizerRewrites)
        {
            this.normalizerRewrites = normalizerRewrites;
        }

        private Expression CreateCompareExpression(Expression left, Expression right)
        {
            Expression expression = Expression.Condition(CreateRelationalOperator(ExpressionType.Equal, left, right), Expression.Constant(0), Expression.Condition(CreateRelationalOperator(ExpressionType.GreaterThan, left, right), Expression.Constant(1), Expression.Constant(-1)));
            this._patterns[expression] = new ComparePattern(left, right);
            return expression;
        }

        private static BinaryExpression CreateRelationalOperator(ExpressionType op, Expression left, Expression right)
        {
            BinaryExpression expression;
            TryCreateRelationalOperator(op, left, right, out expression);
            return expression;
        }

        private static bool HasPredicateArgument(MethodCallExpression callExpression, out int argumentOrdinal)
        {
            SequenceMethod method;
            argumentOrdinal = 0;
            bool flag = false;
            if ((2 <= callExpression.Arguments.Count) && ReflectionUtil.TryIdentifySequenceMethod(callExpression.Method, out method))
            {
                SequenceMethod method2 = method;
                if (method2 <= SequenceMethod.SkipWhileOrdinal)
                {
                    switch (method2)
                    {
                        case SequenceMethod.Where:
                        case SequenceMethod.WhereOrdinal:
                        case SequenceMethod.TakeWhile:
                        case SequenceMethod.TakeWhileOrdinal:
                        case SequenceMethod.SkipWhile:
                        case SequenceMethod.SkipWhileOrdinal:
                            goto Label_00B4;

                        case SequenceMethod.Skip:
                            return flag;
                    }
                    return flag;
                }
                switch (method2)
                {
                    case SequenceMethod.FirstPredicate:
                    case SequenceMethod.FirstOrDefaultPredicate:
                    case SequenceMethod.LastPredicate:
                    case SequenceMethod.LastOrDefaultPredicate:
                    case SequenceMethod.SinglePredicate:
                    case SequenceMethod.SingleOrDefaultPredicate:
                    case SequenceMethod.AnyPredicate:
                    case SequenceMethod.All:
                    case SequenceMethod.CountPredicate:
                    case SequenceMethod.LongCountPredicate:
                        goto Label_00B4;

                    case SequenceMethod.FirstOrDefault:
                    case SequenceMethod.Last:
                    case SequenceMethod.LastOrDefault:
                    case SequenceMethod.Single:
                    case SequenceMethod.SingleOrDefault:
                        return flag;

                    case SequenceMethod.Count:
                    case SequenceMethod.LongCount:
                        return flag;
                }
            }
            return flag;
        Label_00B4:
            argumentOrdinal = 1;
            return true;
        }

        private static bool IsConstantZero(Expression expression)
        {
            return ((expression.NodeType == ExpressionType.Constant) && ((ConstantExpression) expression).Value.Equals(0));
        }

        internal static Expression Normalize(Expression expression, Dictionary<Expression, Expression> rewrites)
        {
            ExpressionNormalizer normalizer = new ExpressionNormalizer(rewrites);
            return normalizer.Visit(expression);
        }

        private static MethodCallExpression NormalizeEnumerableSource(MethodCallExpression callExpression)
        {
            SequenceMethod method;
            MethodInfo info = callExpression.Method;
            if (!ReflectionUtil.TryIdentifySequenceMethod(callExpression.Method, out method) || (!ReflectionUtil.IsAnyAllMethod(method) && (method != SequenceMethod.OfType)))
            {
                return callExpression;
            }
            Expression operand = callExpression.Arguments[0];
            while (ExpressionType.Convert == operand.NodeType)
            {
                operand = ((UnaryExpression) operand).Operand;
            }
            if (operand == callExpression.Arguments[0])
            {
                return callExpression;
            }
            if ((method != SequenceMethod.Any) && (method != SequenceMethod.OfType))
            {
                return Expression.Call(info, operand, callExpression.Arguments[1]);
            }
            return Expression.Call(info, operand);
        }

        private static MethodCallExpression NormalizePredicateArgument(MethodCallExpression callExpression)
        {
            int num;
            Expression expression2;
            if (HasPredicateArgument(callExpression, out num) && TryMatchCoalescePattern(callExpression.Arguments[num], out expression2))
            {
                List<Expression> arguments = new List<Expression>(callExpression.Arguments);
                arguments[num] = expression2;
                return Expression.Call(callExpression.Object, callExpression.Method, arguments);
            }
            return callExpression;
        }

        private static MethodCallExpression NormalizeSelectWithTypeCast(MethodCallExpression callExpression)
        {
            Type type;
            if (TryMatchSelectWithConvert(callExpression, out type))
            {
                MethodInfo method = callExpression.Method.DeclaringType.GetMethod("Cast", true, true);
                if (((method != null) && method.IsGenericMethodDefinition) && ReflectionUtil.IsSequenceMethod(method, SequenceMethod.Cast))
                {
                    return Expression.Call(method.MakeGenericMethod(new Type[] { type }), callExpression.Arguments[0]);
                }
            }
            return callExpression;
        }

        private void RecordRewrite(Expression source, Expression rewritten)
        {
            if (source != rewritten)
            {
                this.NormalizerRewrites.Add(rewritten, source);
            }
        }

        private static bool RelationalOperatorPlaceholder<TLeft, TRight>(TLeft left, TRight right)
        {
            return object.ReferenceEquals(left, right);
        }

        private static bool TryCreateRelationalOperator(ExpressionType op, Expression left, Expression right, out BinaryExpression result)
        {
            MethodInfo method = s_relationalOperatorPlaceholderMethod.MakeGenericMethod(new Type[] { left.Type, right.Type });
            switch (op)
            {
                case ExpressionType.Equal:
                    result = Expression.Equal(left, right, false, method);
                    return true;

                case ExpressionType.GreaterThan:
                    result = Expression.GreaterThan(left, right, false, method);
                    return true;

                case ExpressionType.GreaterThanOrEqual:
                    result = Expression.GreaterThanOrEqual(left, right, false, method);
                    return true;

                case ExpressionType.LessThan:
                    result = Expression.LessThan(left, right, false, method);
                    return true;

                case ExpressionType.LessThanOrEqual:
                    result = Expression.LessThanOrEqual(left, right, false, method);
                    return true;

                case ExpressionType.NotEqual:
                    result = Expression.NotEqual(left, right, false, method);
                    return true;
            }
            result = null;
            return false;
        }

        private static bool TryMatchCoalescePattern(Expression expression, out Expression normalized)
        {
            normalized = null;
            bool flag = false;
            if (expression.NodeType == ExpressionType.Quote)
            {
                UnaryExpression expression2 = (UnaryExpression) expression;
                if (TryMatchCoalescePattern(expression2.Operand, out normalized))
                {
                    flag = true;
                    normalized = Expression.Quote(normalized);
                }
                return flag;
            }
            if (expression.NodeType == ExpressionType.Lambda)
            {
                LambdaExpression expression3 = (LambdaExpression) expression;
                if ((expression3.Body.NodeType == ExpressionType.Coalesce) && (expression3.Body.Type == typeof(bool)))
                {
                    BinaryExpression body = (BinaryExpression) expression3.Body;
                    if (body.Right.NodeType == ExpressionType.Constant)
                    {
                        bool flag2 = false;
                        if (flag2.Equals(((ConstantExpression) body.Right).Value))
                        {
                            normalized = Expression.Lambda(expression3.Type, (Expression) Expression.Convert(body.Left, typeof(bool)), (IEnumerable<ParameterExpression>) expression3.Parameters);
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        private static bool TryMatchConvertSingleArgument(Expression expression, out Type convertType)
        {
            convertType = null;
            expression = (expression.NodeType == ExpressionType.Quote) ? ((UnaryExpression) expression).Operand : expression;
            if (expression.NodeType == ExpressionType.Lambda)
            {
                LambdaExpression expression2 = (LambdaExpression) expression;
                if ((expression2.Parameters.Count == 1) && (expression2.Body.NodeType == ExpressionType.Convert))
                {
                    UnaryExpression body = (UnaryExpression) expression2.Body;
                    if (body.Operand == expression2.Parameters[0])
                    {
                        convertType = body.Type;
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool TryMatchSelectWithConvert(MethodCallExpression callExpression, out Type convertType)
        {
            convertType = null;
            return (ReflectionUtil.IsSequenceMethod(callExpression.Method, SequenceMethod.Select) && TryMatchConvertSingleArgument(callExpression.Arguments[1], out convertType));
        }

        private static Expression UnwrapObjectConvert(Expression input)
        {
            if ((input.NodeType == ExpressionType.Constant) && (input.Type == typeof(object)))
            {
                ConstantExpression expression = (ConstantExpression) input;
                if ((expression.Value != null) && (expression.Value.GetType() != typeof(object)))
                {
                    return Expression.Constant(expression.Value, expression.Value.GetType());
                }
            }
            while ((ExpressionType.Convert == input.NodeType) && (typeof(object) == input.Type))
            {
                input = ((UnaryExpression) input).Operand;
            }
            return input;
        }

        internal override Expression VisitBinary(BinaryExpression b)
        {
            Pattern pattern;
            BinaryExpression rewritten = (BinaryExpression) base.VisitBinary(b);
            if (rewritten.NodeType == ExpressionType.Equal)
            {
                Expression left = UnwrapObjectConvert(rewritten.Left);
                Expression right = UnwrapObjectConvert(rewritten.Right);
                if ((left != rewritten.Left) || (right != rewritten.Right))
                {
                    rewritten = CreateRelationalOperator(ExpressionType.Equal, left, right);
                }
            }
            if ((this._patterns.TryGetValue(rewritten.Left, out pattern) && (pattern.Kind == PatternKind.Compare)) && IsConstantZero(rewritten.Right))
            {
                BinaryExpression expression4;
                ComparePattern pattern2 = (ComparePattern) pattern;
                if (TryCreateRelationalOperator(rewritten.NodeType, pattern2.Left, pattern2.Right, out expression4))
                {
                    rewritten = expression4;
                }
            }
            this.RecordRewrite(b, rewritten);
            return rewritten;
        }

        internal override Expression VisitMethodCall(MethodCallExpression call)
        {
            Expression rewritten = this.VisitMethodCallNoRewrite(call);
            this.RecordRewrite(call, rewritten);
            return rewritten;
        }

        internal Expression VisitMethodCallNoRewrite(MethodCallExpression call)
        {
            MethodCallExpression callExpression = (MethodCallExpression) base.VisitMethodCall(call);
            if (callExpression.Method.IsStatic && callExpression.Method.Name.StartsWith("op_", StringComparison.Ordinal))
            {
                if (callExpression.Arguments.Count == 2)
                {
                    switch (callExpression.Method.Name)
                    {
                        case "op_Equality":
                            return Expression.Equal(callExpression.Arguments[0], callExpression.Arguments[1], false, callExpression.Method);

                        case "op_Inequality":
                            return Expression.NotEqual(callExpression.Arguments[0], callExpression.Arguments[1], false, callExpression.Method);

                        case "op_GreaterThan":
                            return Expression.GreaterThan(callExpression.Arguments[0], callExpression.Arguments[1], false, callExpression.Method);

                        case "op_GreaterThanOrEqual":
                            return Expression.GreaterThanOrEqual(callExpression.Arguments[0], callExpression.Arguments[1], false, callExpression.Method);

                        case "op_LessThan":
                            return Expression.LessThan(callExpression.Arguments[0], callExpression.Arguments[1], false, callExpression.Method);

                        case "op_LessThanOrEqual":
                            return Expression.LessThanOrEqual(callExpression.Arguments[0], callExpression.Arguments[1], false, callExpression.Method);

                        case "op_Multiply":
                            return Expression.Multiply(callExpression.Arguments[0], callExpression.Arguments[1], callExpression.Method);

                        case "op_Subtraction":
                            return Expression.Subtract(callExpression.Arguments[0], callExpression.Arguments[1], callExpression.Method);

                        case "op_Addition":
                            return Expression.Add(callExpression.Arguments[0], callExpression.Arguments[1], callExpression.Method);

                        case "op_Division":
                            return Expression.Divide(callExpression.Arguments[0], callExpression.Arguments[1], callExpression.Method);

                        case "op_Modulus":
                            return Expression.Modulo(callExpression.Arguments[0], callExpression.Arguments[1], callExpression.Method);

                        case "op_BitwiseAnd":
                            return Expression.And(callExpression.Arguments[0], callExpression.Arguments[1], callExpression.Method);

                        case "op_BitwiseOr":
                            return Expression.Or(callExpression.Arguments[0], callExpression.Arguments[1], callExpression.Method);

                        case "op_ExclusiveOr":
                            return Expression.ExclusiveOr(callExpression.Arguments[0], callExpression.Arguments[1], callExpression.Method);
                    }
                }
                if (callExpression.Arguments.Count == 1)
                {
                    switch (callExpression.Method.Name)
                    {
                        case "op_UnaryNegation":
                            return Expression.Negate(callExpression.Arguments[0], callExpression.Method);

                        case "op_UnaryPlus":
                            return Expression.UnaryPlus(callExpression.Arguments[0], callExpression.Method);

                        case "op_Explicit":
                        case "op_Implicit":
                            return Expression.Convert(callExpression.Arguments[0], callExpression.Type, callExpression.Method);

                        case "op_OnesComplement":
                        case "op_False":
                            return Expression.Not(callExpression.Arguments[0], callExpression.Method);
                    }
                }
            }
            if ((callExpression.Method.IsStatic && (callExpression.Method.Name == "Equals")) && (callExpression.Arguments.Count > 1))
            {
                return Expression.Equal(callExpression.Arguments[0], callExpression.Arguments[1], false, callExpression.Method);
            }
            if ((!callExpression.Method.IsStatic && (callExpression.Method.Name == "Equals")) && (callExpression.Arguments.Count > 0))
            {
                return CreateRelationalOperator(ExpressionType.Equal, callExpression.Object, callExpression.Arguments[0]);
            }
            if ((callExpression.Method.IsStatic && (callExpression.Method.Name == "CompareString")) && (callExpression.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators"))
            {
                return this.CreateCompareExpression(callExpression.Arguments[0], callExpression.Arguments[1]);
            }
            if ((!callExpression.Method.IsStatic && (callExpression.Method.Name == "CompareTo")) && ((callExpression.Arguments.Count == 1) && (callExpression.Method.ReturnType == typeof(int))))
            {
                return this.CreateCompareExpression(callExpression.Object, callExpression.Arguments[0]);
            }
            if ((callExpression.Method.IsStatic && (callExpression.Method.Name == "Compare")) && ((callExpression.Arguments.Count > 1) && (callExpression.Method.ReturnType == typeof(int))))
            {
                return this.CreateCompareExpression(callExpression.Arguments[0], callExpression.Arguments[1]);
            }
            return NormalizeEnumerableSource(NormalizeSelectWithTypeCast(NormalizePredicateArgument(callExpression)));
        }

        internal override Expression VisitUnary(UnaryExpression u)
        {
            UnaryExpression expression = (UnaryExpression) base.VisitUnary(u);
            Expression rewritten = expression;
            this.RecordRewrite(u, rewritten);
            if ((expression.NodeType != ExpressionType.Convert) && (expression.NodeType != ExpressionType.TypeAs))
            {
                return rewritten;
            }
            if (!expression.Type.IsAssignableFrom(expression.Operand.Type))
            {
                return rewritten;
            }
            if ((PrimitiveType.IsKnownNullableType(expression.Operand.Type) || PrimitiveType.IsKnownNullableType(expression.Type)) && !(expression.Operand.Type == expression.Type))
            {
                return rewritten;
            }
            if (ClientTypeUtil.TypeOrElementTypeIsEntity(expression.Operand.Type) && ProjectionAnalyzer.IsCollectionProducingExpression(expression.Operand))
            {
                return rewritten;
            }
            return expression.Operand;
        }

        internal Dictionary<Expression, Expression> NormalizerRewrites
        {
            get
            {
                return this.normalizerRewrites;
            }
        }

        private sealed class ComparePattern : ExpressionNormalizer.Pattern
        {
            internal readonly Expression Left;
            internal readonly Expression Right;

            internal ComparePattern(Expression left, Expression right)
            {
                this.Left = left;
                this.Right = right;
            }

            internal override ExpressionNormalizer.PatternKind Kind
            {
                get
                {
                    return ExpressionNormalizer.PatternKind.Compare;
                }
            }
        }

        private abstract class Pattern
        {
            protected Pattern()
            {
            }

            internal abstract ExpressionNormalizer.PatternKind Kind { get; }
        }

        private enum PatternKind
        {
            Compare
        }
    }
}

