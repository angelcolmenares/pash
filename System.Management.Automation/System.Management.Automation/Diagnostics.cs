namespace System.Management.Automation
{
    using System;
    using System.Diagnostics;
    using System.Text;

    internal sealed class Diagnostics
    {
        private static bool throwInsteadOfAssert = false;
        private static object throwInsteadOfAssertLock = 1;

        private Diagnostics()
        {
        }

        [Conditional("ASSERTIONS_TRACE"), Conditional("DEBUG")]
        internal static void Assert(bool condition, string whyThisShouldNeverHappen)
        {
            Assert(condition, whyThisShouldNeverHappen, string.Empty);
        }

        [Conditional("DEBUG"), Conditional("ASSERTIONS_TRACE")]
        internal static void Assert(bool condition, string whyThisShouldNeverHappen, string detailMessage)
        {
            if (!condition)
            {
                if (ThrowInsteadOfAssert)
                {
                    AssertException exception = new AssertException("ASSERT: " + whyThisShouldNeverHappen + "  " + detailMessage + " ");
                    throw exception;
                }
                Debug.Fail(whyThisShouldNeverHappen, detailMessage);
            }
        }

        internal static string StackTrace(int framesToSkip)
        {
            StackFrame[] frames = new System.Diagnostics.StackTrace(true).GetFrames();
            StringBuilder builder = new StringBuilder();
            int num = 10;
            num += framesToSkip;
            for (int i = framesToSkip; (i < frames.Length) && (i < num); i++)
            {
                builder.Append(frames[i].ToString());
            }
            return builder.ToString();
        }

        internal static bool ThrowInsteadOfAssert
        {
            get
            {
                lock (throwInsteadOfAssertLock)
                {
                    return throwInsteadOfAssert;
                }
            }
            set
            {
                lock (throwInsteadOfAssertLock)
                {
                    throwInsteadOfAssert = value;
                }
            }
        }
    }
}

