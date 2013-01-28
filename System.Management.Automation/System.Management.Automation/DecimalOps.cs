namespace System.Management.Automation
{
    using System;

    internal static class DecimalOps
    {
        internal static object Add(decimal lhs, decimal rhs)
        {
            object obj2;
            try
            {
                obj2 = lhs + rhs;
            }
            catch (OverflowException exception)
            {
                throw new RuntimeException(exception.Message, exception);
            }
            return obj2;
        }

        internal static object BAnd(decimal lhs, decimal rhs)
        {
            ulong num = ConvertToUlong(lhs);
            ulong num2 = ConvertToUlong(rhs);
            if ((lhs >= 0M) && (rhs >= 0M))
            {
                return (num & num2);
            }
            return (long) (num & num2);
        }

        internal static object BNot(decimal val)
        {
            if ((val <= 2147483647M) && (val >= -2147483648M))
            {
                return ~LanguagePrimitives.ConvertTo<int>(val);
            }
            if ((val <= 4294967295M) && (val >= 0M))
            {
                return ~LanguagePrimitives.ConvertTo<int>(val);
            }
            if ((val <= 9223372036854775807M) && (val >= -9223372036854775808M))
            {
                return ~LanguagePrimitives.ConvertTo<long>(val);
            }
            if ((val <= 18446744073709551615M) && (val >= 0M))
            {
                return ~LanguagePrimitives.ConvertTo<ulong>(val);
            }
            LanguagePrimitives.ThrowInvalidCastException(val, typeof(int));
            return null;
        }

        internal static object BOr(decimal lhs, decimal rhs)
        {
            ulong num = ConvertToUlong(lhs);
            ulong num2 = ConvertToUlong(rhs);
            if ((lhs >= 0M) && (rhs >= 0M))
            {
                return (num | num2);
            }
            return (long) (num | num2);
        }

        internal static object BXor(decimal lhs, decimal rhs)
        {
            ulong num = ConvertToUlong(lhs);
            ulong num2 = ConvertToUlong(rhs);
            if ((lhs >= 0M) && (rhs >= 0M))
            {
                return (num ^ num2);
            }
            return (long) (num ^ num2);
        }

        internal static object CompareEq(decimal lhs, decimal rhs)
        {
            if (!(lhs == rhs))
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareEq1(double lhs, decimal rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareEq), new Func<decimal, decimal, object>(DecimalOps.CompareEq));
        }

        internal static object CompareEq2(decimal lhs, double rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareEq), new Func<decimal, decimal, object>(DecimalOps.CompareEq));
        }

        internal static object CompareGe(decimal lhs, decimal rhs)
        {
            if (lhs < rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGe1(double lhs, decimal rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareGe), new Func<decimal, decimal, object>(DecimalOps.CompareGe));
        }

        internal static object CompareGe2(decimal lhs, double rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareGe), new Func<decimal, decimal, object>(DecimalOps.CompareGe));
        }

        internal static object CompareGt(decimal lhs, decimal rhs)
        {
            if (lhs <= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGt1(double lhs, decimal rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareGt), new Func<decimal, decimal, object>(DecimalOps.CompareGt));
        }

        internal static object CompareGt2(decimal lhs, double rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareGt), new Func<decimal, decimal, object>(DecimalOps.CompareGt));
        }

        internal static object CompareLe(decimal lhs, decimal rhs)
        {
            if (lhs > rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLe1(double lhs, decimal rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareLe), new Func<decimal, decimal, object>(DecimalOps.CompareLe));
        }

        internal static object CompareLe2(decimal lhs, double rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareLe), new Func<decimal, decimal, object>(DecimalOps.CompareLe));
        }

        internal static object CompareLt(decimal lhs, decimal rhs)
        {
            if (lhs >= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLt1(double lhs, decimal rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareLt), new Func<decimal, decimal, object>(DecimalOps.CompareLt));
        }

        internal static object CompareLt2(decimal lhs, double rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareLt), new Func<decimal, decimal, object>(DecimalOps.CompareLt));
        }

        internal static object CompareNe(decimal lhs, decimal rhs)
        {
            if (!(lhs != rhs))
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareNe1(double lhs, decimal rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareNe), new Func<decimal, decimal, object>(DecimalOps.CompareNe));
        }

        internal static object CompareNe2(decimal lhs, double rhs)
        {
            return CompareWithDouble(lhs, rhs, new Func<double, double, object>(DoubleOps.CompareNe), new Func<decimal, decimal, object>(DecimalOps.CompareNe));
        }

        private static object CompareWithDouble(decimal left, double right, Func<double, double, object> doubleComparer, Func<decimal, decimal, object> decimalComparer)
        {
            decimal num;
            try
            {
                num = (decimal) right;
            }
            catch (OverflowException)
            {
                return doubleComparer((double) left, right);
            }
            return decimalComparer(left, num);
        }

        private static object CompareWithDouble(double left, decimal right, Func<double, double, object> doubleComparer, Func<decimal, decimal, object> decimalComparer)
        {
            decimal num;
            try
            {
                num = (decimal) left;
            }
            catch (OverflowException)
            {
                return doubleComparer(left, (double) right);
            }
            return decimalComparer(num, right);
        }

        private static ulong ConvertToUlong(decimal val)
        {
            if (val < 0M)
            {
                return 0; //TODO: //LanguagePrimitives.ConvertTo<long>(val);
            }
            return LanguagePrimitives.ConvertTo<ulong>(val);
        }

        internal static object Divide(decimal lhs, decimal rhs)
        {
            object obj2;
            try
            {
                obj2 = lhs / rhs;
            }
            catch (OverflowException exception)
            {
                throw new RuntimeException(exception.Message, exception);
            }
            catch (DivideByZeroException exception2)
            {
                throw new RuntimeException(exception2.Message, exception2);
            }
            return obj2;
        }

        internal static object LeftShift(decimal val, int count)
        {
            if ((val <= 2147483647M) && (val >= -2147483648M))
            {
                return (LanguagePrimitives.ConvertTo<int>((decimal) val) << (count & 0x1f));
            }
            if ((val <= 4294967295M) && (val >= 0M))
            {
                return (LanguagePrimitives.ConvertTo<int>((decimal) val) << (count & 0x1f));
            }
            if ((val <= 9223372036854775807M) && (val >= -9223372036854775808M))
            {
                return (LanguagePrimitives.ConvertTo<long>((decimal) val) << (count & 0x3f));
            }
            if ((val <= 18446744073709551615M) && (val >= 0M))
            {
                return (LanguagePrimitives.ConvertTo<ulong>((decimal) val) << (count & 0x3f));
            }
            LanguagePrimitives.ThrowInvalidCastException(val, typeof(int));
            return null;
        }

        internal static object Multiply(decimal lhs, decimal rhs)
        {
            object obj2;
            try
            {
                obj2 = lhs * rhs;
            }
            catch (OverflowException exception)
            {
                throw new RuntimeException(exception.Message, exception);
            }
            return obj2;
        }

        internal static object Remainder(decimal lhs, decimal rhs)
        {
            object obj2;
            try
            {
                obj2 = lhs % rhs;
            }
            catch (OverflowException exception)
            {
                throw new RuntimeException(exception.Message, exception);
            }
            catch (DivideByZeroException exception2)
            {
                throw new RuntimeException(exception2.Message, exception2);
            }
            return obj2;
        }

        internal static object RightShift(decimal val, int count)
        {
            if ((val <= 2147483647M) && (val >= -2147483648M))
            {
                return (LanguagePrimitives.ConvertTo<int>((decimal) val) >> (count & 0x1f));
            }
            if ((val <= 4294967295M) && (val >= 0M))
            {
                return (LanguagePrimitives.ConvertTo<int>((decimal) val) >> (count & 0x1f));
            }
            if ((val <= 9223372036854775807M) && (val >= -9223372036854775808M))
            {
                return (LanguagePrimitives.ConvertTo<long>((decimal) val) >> (count & 0x3f));
            }
            if ((val <= 18446744073709551615M) && (val >= 0M))
            {
                return (LanguagePrimitives.ConvertTo<ulong>((decimal) val) >> (count & 0x3f));
            }
            LanguagePrimitives.ThrowInvalidCastException(val, typeof(int));
            return null;
        }

        internal static object Sub(decimal lhs, decimal rhs)
        {
            object obj2;
            try
            {
                obj2 = lhs - rhs;
            }
            catch (OverflowException exception)
            {
                throw new RuntimeException(exception.Message, exception);
            }
            return obj2;
        }
    }
}

