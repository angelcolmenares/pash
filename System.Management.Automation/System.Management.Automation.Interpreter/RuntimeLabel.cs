namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RuntimeLabel
    {
        public readonly int Index;
        public readonly int StackDepth;
        public readonly int ContinuationStackDepth;
        public RuntimeLabel(int index, int continuationStackDepth, int stackDepth)
        {
            this.Index = index;
            this.ContinuationStackDepth = continuationStackDepth;
            this.StackDepth = stackDepth;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "->{0} C({1}) S({2})", new object[] { this.Index, this.ContinuationStackDepth, this.StackDepth });
        }
    }
}

