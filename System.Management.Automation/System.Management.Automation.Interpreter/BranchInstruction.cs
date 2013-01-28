using System;

namespace System.Management.Automation.Interpreter
{
    internal class BranchInstruction : OffsetInstruction
    {
        private static Instruction[][][] _caches;

        internal readonly bool _hasResult;

        internal readonly bool _hasValue;

        public override Instruction[] Cache
        {
            get
            {
                if (BranchInstruction._caches == null)
                {
                    Instruction[][][] instructionArray = new Instruction[2][][];
                    instructionArray[0] = new Instruction[2][];
                    instructionArray[1] = new Instruction[2][];
                    BranchInstruction._caches = instructionArray;
                }
                Instruction[] instructionArray1 = BranchInstruction._caches[this.ConsumedStack][this.ProducedStack];
                Instruction[] instructionArray2 = instructionArray1;
                if (instructionArray1 == null)
                {
                    Instruction[] instructionArray3 = new Instruction[32];
                    Instruction[] instructionArray4 = instructionArray3;
                    BranchInstruction._caches[this.ConsumedStack][this.ProducedStack] = instructionArray3;
                    instructionArray2 = instructionArray4;
                }
                return instructionArray2;
            }
        }

        public override int ConsumedStack
        {
            get
            {
                if (this._hasValue)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override int ProducedStack
        {
            get
            {
                if (this._hasResult)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        internal BranchInstruction()
            : this(false, false)
        {
        }

        public BranchInstruction(bool hasResult, bool hasValue)
        {
            this._hasResult = hasResult;
            this._hasValue = hasValue;
        }

        public override int Run(InterpretedFrame frame)
        {
            return this._offset;
        }
    }
}