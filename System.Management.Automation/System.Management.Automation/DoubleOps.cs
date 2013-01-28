namespace System.Management.Automation
{
    using System;

    internal static class DoubleOps
    {
        internal static object Add(double lhs, double rhs)
        {
            return (lhs + rhs);
        }

        internal static object BAnd(double lhs, double rhs)
        {
            ulong num = ConvertToUlong(lhs);
            ulong num2 = ConvertToUlong(rhs);
            if ((lhs >= 0.0) && (rhs >= 0.0))
            {
                return (num & num2);
            }
            return (long) (num & num2);
        }

        internal static object BNot(double val)
        {
            try
            {
                if ((val <= 2147483647.0) && (val >= -2147483648.0))
                {
                    return ~LanguagePrimitives.ConvertTo<int>(val);
                }
                if ((val <= 4294967295) && (val >= 0.0))
                {
                    return ~LanguagePrimitives.ConvertTo<int>(val);
                }
                if ((val <= 9.2233720368547758E+18) && (val >= -9.2233720368547758E+18))
                {
                    return ~LanguagePrimitives.ConvertTo<long>(val);
                }
                if ((val <= 1.8446744073709552E+19) && (val >= 0.0))
                {
                    return ~LanguagePrimitives.ConvertTo<ulong>(val);
                }
            }
            catch (OverflowException)
            {
            }
            LanguagePrimitives.ThrowInvalidCastException(val, typeof(ulong));
            return null;
        }

        internal static object BOr(double lhs, double rhs)
        {
            ulong num = ConvertToUlong(lhs);
            ulong num2 = ConvertToUlong(rhs);
            if ((lhs >= 0.0) && (rhs >= 0.0))
            {
                return (num | num2);
            }
            return (long) (num | num2);
        }

        internal static object BXor(double lhs, double rhs)
        {
            ulong num = ConvertToUlong(lhs);
            ulong num2 = ConvertToUlong(rhs);
            if ((lhs >= 0.0) && (rhs >= 0.0))
            {
                return (num ^ num2);
            }
            return (long) (num ^ num2);
        }

        internal static object CompareEq(double lhs, double rhs)
        {
            if (lhs != rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGe(double lhs, double rhs)
        {
            if (lhs < rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGt(double lhs, double rhs)
        {
            if (lhs <= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLe(double lhs, double rhs)
        {
            if (lhs > rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLt(double lhs, double rhs)
        {
            if (lhs >= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareNe(double lhs, double rhs)
        {
            if (lhs == rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        private static ulong ConvertToUlong(double val)
        {
            if (val < 0.0)
            {
                return 0; //TODO: LanguagePrimitives.ConvertTo<long>(val);
            }
            return LanguagePrimitives.ConvertTo<ulong>(val);
        }

        internal static object Divide(double lhs, double rhs)
        {
            return (lhs / rhs);
        }

        internal static object LeftShift(double val, int count)
        {
            if ((val <= 2147483647.0) && (val >= -2147483648.0))
            {
                return (LanguagePrimitives.ConvertTo<int>((double) val) << (count & 0x1f));
            }
            if ((val <= 4294967295) && (val >= 0.0))
            {
                return (LanguagePrimitives.ConvertTo<int>((double) val) << (count & 0x1f));
            }
            if ((val <= 9.2233720368547758E+18) && (val >= -9.2233720368547758E+18))
            {
                return (LanguagePrimitives.ConvertTo<long>((double) val) << (count & 0x3f));
            }
            if ((val <= 1.8446744073709552E+19) && (val >= 0.0))
            {
                return (LanguagePrimitives.ConvertTo<ulong>((double) val) << (count & 0x3f));
            }
            LanguagePrimitives.ThrowInvalidCastException(val, typeof(ulong));
            return null;
        }

        internal static object Multiply(double lhs, double rhs)
        {
            return (lhs * rhs);
        }

        internal static object Remainder(double lhs, double rhs)
        {
            return (lhs % rhs);
        }

        internal static object RightShift(double val, int count)
        {
            if ((val <= 2147483647.0) && (val >= -2147483648.0))
            {
                return (LanguagePrimitives.ConvertTo<int>((double) val) >> (count & 0x1f));
            }
            if ((val <= 4294967295) && (val >= 0.0))
            {
                return (LanguagePrimitives.ConvertTo<int>((double) val) >> (count & 0x1f));
            }
            if ((val <= 9.2233720368547758E+18) && (val >= -9.2233720368547758E+18))
            {
                return (LanguagePrimitives.ConvertTo<long>((double) val) >> (count & 0x3f));
            }
            if ((val <= 1.8446744073709552E+19) && (val >= 0.0))
            {
                return (LanguagePrimitives.ConvertTo<ulong>((double) val) >> (count & 0x3f));
            }
            LanguagePrimitives.ThrowInvalidCastException(val, typeof(ulong));
            return null;
        }

        internal static object Sub(double lhs, double rhs)
        {
            return (lhs - rhs);
        }
    }
}

