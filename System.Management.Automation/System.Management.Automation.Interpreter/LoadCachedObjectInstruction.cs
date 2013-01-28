namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal sealed class LoadCachedObjectInstruction : Instruction
    {
        private readonly int _index;

        internal LoadCachedObjectInstruction(int index)
        {
            this._index = index;
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[frame.StackIndex++] = frame.Interpreter._objects[this._index];
            return 1;
        }

        public override string ToDebugString(int instructionIndex, object cookie, Func<int, int> labelIndexer, IList<object> objects)
        {
            return string.Format(CultureInfo.InvariantCulture, "LoadCached({0}: {1})", new object[] { this._index, objects[(int) this._index] });
        }

        public override string ToString()
        {
            return ("LoadCached(" + this._index + ")");
        }

        public override int ProducedStack
        {
            get
            {
                return 1;
            }
        }
    }
}

