namespace Microsoft.Data.OData
{
    using System;
	using System.Threading;

    internal static class ExceptionUtils
    {
        private static readonly Type OutOfMemoryType = typeof(OutOfMemoryException);
        private static readonly Type StackOverflowType = typeof(StackOverflowException);
        private static readonly Type ThreadAbortType = typeof(ThreadAbortException);

        internal static void CheckArgumentNotNull<T>([ValidatedNotNull] T value, string parameterName) where T: class
        {
            if (value == null)
            {
                throw Error.ArgumentNull(parameterName);
            }
        }

        internal static void CheckArgumentStringNotNullOrEmpty([ValidatedNotNull] string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(parameterName, Strings.ExceptionUtils_ArgumentStringNullOrEmpty);
            }
        }

        internal static void CheckIntegerNotNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, Strings.ExceptionUtils_CheckIntegerNotNegative(value));
            }
        }

        internal static void CheckIntegerPositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, Strings.ExceptionUtils_CheckIntegerPositive(value));
            }
        }

        internal static void CheckLongPositive(long value, string parameterName)
        {
            if (value <= 0L)
            {
                throw new ArgumentOutOfRangeException(parameterName, Strings.ExceptionUtils_CheckLongPositive(value));
            }
        }

        internal static bool IsCatchableExceptionType(Exception e)
        {
            Type type = e.GetType();
            return (((type != ThreadAbortType) && (type != StackOverflowType)) && (type != OutOfMemoryType));
        }

        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}

