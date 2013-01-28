using System.Security;
using System.Threading;

namespace System.Spatial
{
    using System;

    internal class Util
    {
        private static readonly Type AccessViolationType = typeof(AccessViolationException);
        private static readonly Type NullReferenceType = typeof(NullReferenceException);
        private static readonly Type OutOfMemoryType = typeof(OutOfMemoryException);
        private static readonly Type SecurityType = typeof(SecurityException);
        private static readonly Type StackOverflowType = typeof(StackOverflowException);
        private static readonly Type ThreadAbortType = typeof(ThreadAbortException);

        internal static void CheckArgumentNull([ValidatedNotNull] object arg, string errorMessage)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(errorMessage);
            }
        }

        internal static bool IsCatchableExceptionType(Exception e)
        {
            Type c = e.GetType();
            return (((((c != OutOfMemoryType) && (c != StackOverflowType)) && ((c != ThreadAbortType) && (c != AccessViolationType))) && (c != NullReferenceType)) && !SecurityType.IsAssignableFrom(c));
        }

        private sealed class ValidatedNotNullAttribute : Attribute
        {
        }
    }
}

