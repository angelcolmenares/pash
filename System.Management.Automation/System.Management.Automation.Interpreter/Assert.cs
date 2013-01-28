namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal static class Assert
    {
        [Conditional("DEBUG")]
        public static void NotEmpty(string str)
        {
        }

        [Conditional("DEBUG")]
        public static void NotNull(object var)
        {
        }

        [Conditional("DEBUG")]
        public static void NotNull(object var1, object var2)
        {
        }

        [Conditional("DEBUG")]
        public static void NotNull(object var1, object var2, object var3)
        {
        }

        [Conditional("DEBUG")]
        public static void NotNullItems<T>(IEnumerable<T> items) where T: class
        {
            using (IEnumerator<T> enumerator = items.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                }
            }
        }

        internal static Exception Unreachable
        {
            get
            {
                return new InvalidOperationException("Code supposed to be unreachable");
            }
        }
    }
}

