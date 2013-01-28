namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation.Host;
    using System.Threading;

    internal static class StringUtil
    {
        internal static string Format(string formatSpec, params object[] o)
        {
            return string.Format(Thread.CurrentThread.CurrentCulture, formatSpec, o);
        }

        internal static string Format(string formatSpec, object o)
        {
            return string.Format(Thread.CurrentThread.CurrentCulture, formatSpec, new object[] { o });
        }

        internal static string Format(string formatSpec, object o1, object o2)
        {
            return string.Format(Thread.CurrentThread.CurrentCulture, formatSpec, new object[] { o1, o2 });
        }

        internal static string TruncateToBufferCellWidth(PSHostRawUserInterface rawUI, string toTruncate, int maxWidthInBufferCells)
        {
            int length = Math.Min(toTruncate.Length, maxWidthInBufferCells);
            while (true)
            {
                string source = toTruncate.Substring(0, length);
                if (rawUI.LengthInBufferCells(source) <= maxWidthInBufferCells)
                {
                    return source;
                }
                length--;
            }
        }
    }
}

