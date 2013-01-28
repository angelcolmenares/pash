namespace System.Management.Automation
{
    using System;

    internal static class UIntOps
    {
        internal static object Add(uint lhs, uint rhs)
        {
            ulong num = lhs + rhs;
            if (num <= 0xffffffffL)
            {
                return (int) num;
            }
            return (double) num;
        }

        internal static object CompareEq(uint lhs, uint rhs)
        {
            if (lhs != rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGe(uint lhs, uint rhs)
        {
            if (lhs < rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGt(uint lhs, uint rhs)
        {
            if (lhs <= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLe(uint lhs, uint rhs)
        {
            if (lhs > rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLt(uint lhs, uint rhs)
        {
            if (lhs >= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareNe(uint lhs, uint rhs)
        {
            if (lhs == rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object Divide(uint lhs, uint rhs)
        {
            if (rhs == 0)
            {
                DivideByZeroException innerException = new DivideByZeroException();
                throw new RuntimeException(innerException.Message, innerException);
            }
            if ((lhs % rhs) == 0)
            {
                return (lhs / rhs);
            }
            return (((double) lhs) / ((double) rhs));
        }

        internal static object Multiply(uint lhs, uint rhs)
        {
            ulong num = lhs * rhs;
            if (num <= 0xffffffffL)
            {
                return (int) num;
            }
            return (double) num;
        }

        internal static object Remainder(uint lhs, uint rhs)
        {
            if (rhs == 0)
            {
                DivideByZeroException innerException = new DivideByZeroException();
                throw new RuntimeException(innerException.Message, innerException);
            }
            return (lhs % rhs);
        }

        internal static object Sub(uint lhs, uint rhs)
        {
            long num = lhs - rhs;
            if (num >= 0L)
            {
                return (int) num;
            }
            return (double) num;
        }
    }
}

