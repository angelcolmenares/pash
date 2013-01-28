namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    [Serializable]
    internal class DebugInfo
    {
        private static readonly DebugInfoComparer _debugComparer = new DebugInfoComparer();
        public int EndLine;
        public string FileName;
        public int Index;
        public bool IsClear;
        public int StartLine;

        public static DebugInfo GetMatchingDebugInfo(DebugInfo[] debugInfos, int index)
        {
            DebugInfo info = new DebugInfo {
                Index = index
            };
            int num = Array.BinarySearch<DebugInfo>(debugInfos, info, _debugComparer);
            if (num < 0)
            {
                num = ~num;
                if (num == 0)
                {
                    return null;
                }
                num--;
            }
            return debugInfos[num];
        }

        public override string ToString()
        {
            if (this.IsClear)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}: clear", new object[] { this.Index });
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}: [{1}-{2}] '{3}'", new object[] { this.Index, this.StartLine, this.EndLine, this.FileName });
        }

        private class DebugInfoComparer : IComparer<DebugInfo>
        {
            int IComparer<DebugInfo>.Compare(DebugInfo d1, DebugInfo d2)
            {
                if (d1.Index > d2.Index)
                {
                    return 1;
                }
                if (d1.Index == d2.Index)
                {
                    return 0;
                }
                return -1;
            }
        }
    }
}

