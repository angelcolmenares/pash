namespace System.Management.Automation
{
    using System;
    using System.Numerics;

    internal static class ULongOps
    {
        internal static object Add(ulong lhs, ulong rhs)
        {
            decimal num = lhs + rhs;
            if (num <= 18446744073709551615M)
            {
                return (ulong) num;
            }
            return (double) num;
        }

        internal static object CompareEq(ulong lhs, ulong rhs)
        {
            if (lhs != rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGe(ulong lhs, ulong rhs)
        {
            if (lhs < rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGt(ulong lhs, ulong rhs)
        {
            if (lhs <= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLe(ulong lhs, ulong rhs)
        {
            if (lhs > rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLt(ulong lhs, ulong rhs)
        {
            if (lhs >= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareNe(ulong lhs, ulong rhs)
        {
            if (lhs == rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object Divide(ulong lhs, ulong rhs)
        {
            if (rhs == 0L)
            {
                DivideByZeroException innerException = new DivideByZeroException();
                throw new RuntimeException(innerException.Message, innerException);
            }
            if ((lhs % rhs) == 0L)
            {
                return (lhs / rhs);
            }
            return (((double) lhs) / ((double) rhs));
        }

        internal static object Multiply(ulong lhs, ulong rhs)
        {
            BigInteger integer = lhs;
            BigInteger integer2 = rhs;
            BigInteger integer3 = integer * integer2;
            if (integer3 <= -1L)
            {
                return (ulong) integer3;
            }
            return (double) integer3;
        }

        internal static object Remainder(ulong lhs, ulong rhs)
        {
            if (rhs == 0L)
            {
                DivideByZeroException innerException = new DivideByZeroException();
                throw new RuntimeException(innerException.Message, innerException);
            }
            return (lhs % rhs);
        }

        internal static object Sub(ulong lhs, ulong rhs)
        {
            decimal num = lhs - rhs;
            if (num >= 0M)
            {
                return (ulong) num;
            }
            return (double) num;
        }
    }
}

