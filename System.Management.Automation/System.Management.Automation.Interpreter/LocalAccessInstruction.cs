namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;

    internal abstract class LocalAccessInstruction : Instruction
    {
        internal readonly int _index;

        protected LocalAccessInstruction(int index)
        {
            this._index = index;
        }

        public override string ToDebugString(int instructionIndex, object cookie, Func<int, int> labelIndexer, IList<object> objects)
        {
            if (cookie != null)
            {
                return string.Concat(new object[] { this.InstructionName, "(", cookie, ": ", this._index, ")" });
            }
            return string.Concat(new object[] { this.InstructionName, "(", this._index, ")" });
        }
    }
}

