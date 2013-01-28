namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class StoreLocalInstruction : LocalAccessInstruction, IBoxableInstruction
    {
        internal StoreLocalInstruction(int index) : base(index)
        {
        }

        public Instruction BoxIfIndexMatches(int index)
        {
            if (index != base._index)
            {
                return null;
            }
            return InstructionList.StoreLocalBoxed(index);
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[base._index] = frame.Data[--frame.StackIndex];
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return 1;
            }
        }
    }
}

