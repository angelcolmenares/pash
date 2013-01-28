namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class AssignLocalInstruction : LocalAccessInstruction, IBoxableInstruction
    {
        internal AssignLocalInstruction(int index) : base(index)
        {
        }

        public Instruction BoxIfIndexMatches(int index)
        {
            if (index != base._index)
            {
                return null;
            }
            return InstructionList.AssignLocalBoxed(index);
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[base._index] = frame.Peek();
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return 1;
            }
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

