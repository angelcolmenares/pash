using System;
using System.Collections.Generic;

namespace System.Management.Automation.Interpreter
{
    internal abstract class OffsetInstruction : Instruction
    {
        internal const int Unknown = -2147483648;

        internal const int CacheSize = 32;

        protected int _offset;

        public abstract Instruction[] Cache
        {
            get;
        }

        public int Offset
        {
            get
            {
                return this._offset;
            }
        }

        protected OffsetInstruction()
        {
            this._offset = -2147483648;
        }

        public Instruction Fixup(int offset)
        {
            this._offset = offset;
            Instruction[] cache = this.Cache;
            if (cache == null || offset < 0 || offset >= (int)cache.Length)
            {
                return this;
            }
            else
            {
                Instruction instruction = cache[offset];
                Instruction instruction1 = instruction;
                if (instruction == null)
                {
                    OffsetInstruction offsetInstruction = this;
                    Instruction instruction2 = offsetInstruction;
                    cache[offset] = offsetInstruction;
                    instruction1 = instruction2;
                }
                return instruction1;
            }
        }

        public override string ToDebugString(int instructionIndex, object cookie, Func<int, int> labelIndexer, IList<object> objects)
        {
            string str;
            string str1 = this.ToString();
            if (this._offset != -2147483648)
            {
                str = string.Concat(" -> ", instructionIndex + this._offset);
            }
            else
            {
                str = "";
            }
            return string.Concat(str1, str);
        }

        public override string ToString()
        {
            string str;
            string instructionName = this.InstructionName;
            if (this._offset == -2147483648)
            {
                str = "(?)";
            }
            else
            {
                str = string.Concat("(", this._offset, ")");
            }
            return string.Concat(instructionName, str);
        }
    }
}