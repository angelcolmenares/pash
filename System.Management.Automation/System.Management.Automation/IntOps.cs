namespace System.Management.Automation
{
    using System;

    internal static class IntOps
    {
        internal static object Add(int lhs, int rhs)
        {
            long num = lhs + rhs;
            if ((num <= 0x7fffffffL) && (num >= -2147483648L))
            {
                return (int) num;
            }
            return (double) num;
        }

        internal static object CompareEq(int lhs, int rhs)
        {
            if (lhs != rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGe(int lhs, int rhs)
        {
            if (lhs < rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareGt(int lhs, int rhs)
        {
            if (lhs <= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLe(int lhs, int rhs)
        {
            if (lhs > rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareLt(int lhs, int rhs)
        {
            if (lhs >= rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object CompareNe(int lhs, int rhs)
        {
            if (lhs == rhs)
            {
                return Boxed.False;
            }
            return Boxed.True;
        }

        internal static object Divide(int lhs, int rhs)
        {
            if (rhs == 0)
            {
                DivideByZeroException innerException = new DivideByZeroException();
                throw new RuntimeException(innerException.Message, innerException);
            }
            if (((lhs != -2147483648) || (rhs != -1)) && ((lhs % rhs) == 0))
            {
                return (lhs / rhs);
            }
            return (((double) lhs) / ((double) rhs));
        }

        internal static object Multiply(int lhs, int rhs)
        {
            long num = lhs * rhs;
            if ((num <= 0x7fffffffL) && (num >= -2147483648L))
            {
                return (int) num;
            }
            return (double) num;
        }

        internal static object[] Range(int lower, int upper)
        {
            if (lower == upper)
            {
                return new object[] { lower };
            }
            object[] objArray = new object[Math.Abs((int) (upper - lower)) + 1];
            if (lower > upper)
            {
                for (int j = 0; j < objArray.Length; j++)
                {
                    objArray[j] = lower--;
                }
                return objArray;
            }
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray[i] = lower++;
            }
            return objArray;
        }

        internal static object Remainder(int lhs, int rhs)
        {
            if (rhs == 0)
            {
                DivideByZeroException innerException = new DivideByZeroException();
                throw new RuntimeException(innerException.Message, innerException);
            }
            if ((lhs == -2147483648) && (rhs == -1))
            {
                return 0;
            }
            return (lhs % rhs);
        }

        internal static object Sub(int lhs, int rhs)
        {
            long num = lhs - rhs;
            if ((num <= 0x7fffffffL) && (num >= -2147483648L))
            {
                return (int) num;
            }
            return (double) num;
        }
    }
}

