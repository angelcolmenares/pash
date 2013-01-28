namespace System.Management.Automation
{
    using System;
    using System.Numerics;

    internal static class LongOps
    {
        internal static object Add(long lhs, long rhs)
        {
            decimal num = lhs + rhs;
            if ((num <= 9223372036854775807M) && (num >= -9223372036854775808M))
            {
                return (long) num;
            }
            return (double) num;
        }

        internal static object CompareEq(long lhs, long rhs)
        {
            if (lhs != rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGe(long lhs, long rhs)
        {
            if (lhs < rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGt(long lhs, long rhs)
        {
            if (lhs <= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLe(long lhs, long rhs)
        {
            if (lhs > rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLt(long lhs, long rhs)
        {
            if (lhs >= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareNe(long lhs, long rhs)
        {
            if (lhs == rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object Divide(long lhs, long rhs)
        {
            if (rhs == 0L)
            {
                DivideByZeroException innerException = new DivideByZeroException();
                throw new RuntimeException(innerException.Message, innerException);
            }
            if (((lhs != -9223372036854775808L) || (rhs != -1L)) && ((lhs % rhs) == 0L))
            {
                return (lhs / rhs);
            }
            return (((double) lhs) / ((double) rhs));
        }

        internal static object Multiply(long lhs, long rhs)
        {
            BigInteger integer = lhs;
            BigInteger integer2 = rhs;
            BigInteger integer3 = integer * integer2;
            if ((integer3 <= 0x7fffffffffffffffL) && (integer3 >= -9223372036854775808L))
            {
                return (long) integer3;
            }
            return (double) integer3;
        }

        internal static object Remainder(long lhs, long rhs)
        {
            if (rhs == 0L)
            {
                DivideByZeroException innerException = new DivideByZeroException();
                throw new RuntimeException(innerException.Message, innerException);
            }
            if ((lhs == -9223372036854775808L) && (rhs == -1L))
            {
                return 0L;
            }
            return (lhs % rhs);
        }

        internal static object Sub(long lhs, long rhs)
        {
            decimal num = lhs - rhs;
            if ((num <= 9223372036854775807M) && (num >= -9223372036854775808M))
            {
                return (long) num;
            }
            return (double) num;
        }
    }
}

