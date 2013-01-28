namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;

    internal abstract class IndexedBranchInstruction : Instruction
    {
        internal readonly int _labelIndex;
        protected const int CacheSize = 0x20;

        public IndexedBranchInstruction(int labelIndex)
        {
            this._labelIndex = labelIndex;
        }

        public RuntimeLabel GetLabel(InterpretedFrame frame)
        {
            return frame.Interpreter._labels[this._labelIndex];
        }

        public override string ToDebugString(int instructionIndex, object cookie, Func<int, int> labelIndexer, IList<object> objects)
        {
            int num = labelIndexer(this._labelIndex);
            return (this.ToString() + ((num != -2147483648) ? (" -> " + num) : ""));
        }

        public override string ToString()
        {
            return string.Concat(new object[] { this.InstructionName, "[", this._labelIndex, "]" });
        }
    }
}

