namespace System.Management.Automation.Language
{
    using System;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Reflection;
    using System.Threading;

    internal class PSUnaryOperationBinder : UnaryOperationBinder
    {
        private static PSUnaryOperationBinder _bnotBinder;
        private static PSUnaryOperationBinder _decrementBinder;
        private static PSUnaryOperationBinder _incrementBinder;
        private static PSUnaryOperationBinder _notBinder;
        private static PSUnaryOperationBinder _unaryMinus;
        private static PSUnaryOperationBinder _unaryPlusBinder;

        private PSUnaryOperationBinder(ExpressionType operation) : base(operation)
        {
        }

        internal DynamicMetaObject BNot(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]);
            }
            if ((target.Value is PSObject) && (PSObject.Base(target.Value) != target.Value))
            {
                return this.DeferForPSObject(new DynamicMetaObject[] { target });
            }
            if (target.Value == null)
            {
                return new DynamicMetaObject(ExpressionCache.Constant(-1).Cast(typeof(object)), target.PSGetTypeRestriction());
            }
            MethodInfo method = target.LimitType.GetMethod("op_OnesComplement", BindingFlags.Public | BindingFlags.Static, null, new Type[] { target.LimitType }, null);
            if (method != null)
            {
                return new DynamicMetaObject(Expression.OnesComplement(target.Expression.Cast(target.LimitType), method).Cast(typeof(object)), target.PSGetTypeRestriction());
            }
            if (target.LimitType.Equals(typeof(string)))
            {
                return new DynamicMetaObject(Expression.Dynamic(this, this.ReturnType, PSBinaryOperationBinder.ConvertStringToNumber(target.Expression, typeof(int))), target.PSGetTypeRestriction());
            }
            Expression expression = null;
            if (!target.LimitType.IsNumeric())
            {
                bool flag;
                Type resultType = typeof(int);
                LanguagePrimitives.ConversionData conversion = LanguagePrimitives.FigureConversion(target.Value, resultType, out flag);
                if (conversion.Rank != ConversionRank.None)
                {
                    expression = PSConvertBinder.InvokeConverter(conversion, target.Expression, resultType, flag, ExpressionCache.InvariantCulture);
                }
                else
                {
                    resultType = typeof(long);
                    conversion = LanguagePrimitives.FigureConversion(target.Value, resultType, out flag);
                    if (conversion.Rank != ConversionRank.None)
                    {
                        expression = PSConvertBinder.InvokeConverter(conversion, target.Expression, resultType, flag, ExpressionCache.InvariantCulture);
                    }
                }
            }
            else
            {
                TypeCode typeCode = LanguagePrimitives.GetTypeCode(target.LimitType);
                if (typeCode < TypeCode.Int32)
                {
                    expression = target.Expression.Cast(typeof(int));
                }
                else if (typeCode <= TypeCode.UInt64)
                {
                    expression = target.Expression.Cast(target.LimitType);
                }
                else
                {
                    Type type2 = (typeCode == TypeCode.Decimal) ? typeof(DecimalOps) : typeof(DoubleOps);
                    Type type = (typeCode == TypeCode.Decimal) ? typeof(decimal) : typeof(double);
                    return new DynamicMetaObject(Expression.Call(type2.GetMethod("BNot", BindingFlags.NonPublic | BindingFlags.Static), target.Expression.Convert(type)), target.PSGetTypeRestriction());
                }
            }
            if ((expression == null) && (errorSuggestion == null))
            {
                return PSConvertBinder.ThrowNoConversion(target, typeof(int), this, -1, new DynamicMetaObject[0]);
            }
            return new DynamicMetaObject(Expression.OnesComplement(expression).Cast(typeof(object)), target.PSGetTypeRestriction());
        }

        public override DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]);
            }
            if ((target.Value is PSObject) && (PSObject.Base(target.Value) != target.Value))
            {
                return this.DeferForPSObject(new DynamicMetaObject[] { target });
            }
            switch (base.Operation)
            {
                case ExpressionType.Negate:
                    return this.UnaryMinus(target, errorSuggestion).WriteToDebugLog(this);

                case ExpressionType.UnaryPlus:
                    return this.UnaryPlus(target, errorSuggestion).WriteToDebugLog(this);

                case ExpressionType.Not:
                    return this.Not(target, errorSuggestion).WriteToDebugLog(this);

                case ExpressionType.Decrement:
                    return this.IncrDecr(target, -1, errorSuggestion).WriteToDebugLog(this);

                case ExpressionType.Increment:
                    return this.IncrDecr(target, 1, errorSuggestion).WriteToDebugLog(this);

                case ExpressionType.OnesComplement:
                    return this.BNot(target, errorSuggestion).WriteToDebugLog(this);
            }
            throw new NotImplementedException();
        }

        internal static PSUnaryOperationBinder Get(ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.Decrement:
                    if (_decrementBinder == null)
                    {
                        Interlocked.CompareExchange<PSUnaryOperationBinder>(ref _decrementBinder, new PSUnaryOperationBinder(operation), null);
                    }
                    return _decrementBinder;

                case ExpressionType.Increment:
                    if (_incrementBinder == null)
                    {
                        Interlocked.CompareExchange<PSUnaryOperationBinder>(ref _incrementBinder, new PSUnaryOperationBinder(operation), null);
                    }
                    return _incrementBinder;

                case ExpressionType.OnesComplement:
                    if (_bnotBinder == null)
                    {
                        Interlocked.CompareExchange<PSUnaryOperationBinder>(ref _bnotBinder, new PSUnaryOperationBinder(operation), null);
                    }
                    return _bnotBinder;

                case ExpressionType.Negate:
                    if (_unaryMinus == null)
                    {
                        Interlocked.CompareExchange<PSUnaryOperationBinder>(ref _unaryMinus, new PSUnaryOperationBinder(operation), null);
                    }
                    return _unaryMinus;

                case ExpressionType.UnaryPlus:
                    if (_unaryPlusBinder == null)
                    {
                        Interlocked.CompareExchange<PSUnaryOperationBinder>(ref _unaryPlusBinder, new PSUnaryOperationBinder(operation), null);
                    }
                    return _unaryPlusBinder;

                case ExpressionType.Not:
                    if (_notBinder == null)
                    {
                        Interlocked.CompareExchange<PSUnaryOperationBinder>(ref _notBinder, new PSUnaryOperationBinder(operation), null);
                    }
                    return _notBinder;
            }
            throw new NotImplementedException("Unimplemented unary operation");
        }

        private DynamicMetaObject IncrDecr(DynamicMetaObject target, int valueToAdd, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]);
            }
            if ((target.Value is PSObject) && (PSObject.Base(target.Value) != target.Value))
            {
                return this.DeferForPSObject(new DynamicMetaObject[] { target });
            }
            if (target.Value == null)
            {
                return new DynamicMetaObject(ExpressionCache.Constant(valueToAdd).Cast(typeof(object)), target.PSGetTypeRestriction());
            }
            if (target.LimitType.IsNumeric())
            {
                DynamicMetaObject arg = new DynamicMetaObject(ExpressionCache.Constant(valueToAdd), BindingRestrictions.Empty, valueToAdd);
                return new DynamicMetaObject(PSBinaryOperationBinder.Get(ExpressionType.Add, true, false).FallbackBinaryOperation(target, arg, errorSuggestion).Expression, target.PSGetTypeRestriction());
            }
            return (errorSuggestion ?? target.ThrowRuntimeError(new DynamicMetaObject[0], BindingRestrictions.Empty, "OperatorRequiresNumber", ParserStrings.OperatorRequiresNumber, new Expression[] { Expression.Constant(((base.Operation == ExpressionType.Increment) ? TokenKind.PlusPlus : TokenKind.MinusMinus).Text()), Expression.Constant(target.LimitType) }));
        }

        internal DynamicMetaObject Not(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]);
            }
            return new DynamicMetaObject(Expression.Not(target.CastOrConvert(typeof(bool))).Cast(typeof(object)), target.PSGetTypeRestriction());
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "PSUnaryOperationBinder {0}", new object[] { base.Operation });
        }

        private DynamicMetaObject UnaryMinus(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]);
            }
            if ((target.Value is PSObject) && (PSObject.Base(target.Value) != target.Value))
            {
                return this.DeferForPSObject(new DynamicMetaObject[] { target });
            }
            if (!target.LimitType.IsNumeric())
            {
                return new DynamicMetaObject(Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Subtract, true, false), typeof(object), ExpressionCache.Constant(0), target.Expression), target.PSGetTypeRestriction());
            }
            Expression expr = target.Expression.Cast(target.LimitType);
            if (target.LimitType.Equals(typeof(byte)) || target.LimitType.Equals(typeof(sbyte)))
            {
                expr = expr.Cast(typeof(int));
            }
            return new DynamicMetaObject(Expression.Negate(expr).Cast(typeof(object)), target.PSGetTypeRestriction());
        }

        private DynamicMetaObject UnaryPlus(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]);
            }
            if ((target.Value is PSObject) && (PSObject.Base(target.Value) != target.Value))
            {
                return this.DeferForPSObject(new DynamicMetaObject[] { target });
            }
            if (!target.LimitType.IsNumeric())
            {
                return new DynamicMetaObject(Expression.Dynamic(PSBinaryOperationBinder.Get(ExpressionType.Add, true, false), typeof(object), ExpressionCache.Constant(0), target.Expression), target.PSGetTypeRestriction());
            }
            Expression expr = target.Expression.Cast(target.LimitType);
            if (target.LimitType.Equals(typeof(byte)) || target.LimitType.Equals(typeof(sbyte)))
            {
                expr = expr.Cast(typeof(int));
            }
            return new DynamicMetaObject(Expression.UnaryPlus(expr).Cast(typeof(object)), target.PSGetTypeRestriction());
        }
    }
}

