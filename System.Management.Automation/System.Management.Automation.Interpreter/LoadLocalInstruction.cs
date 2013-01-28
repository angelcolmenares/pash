namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class LoadLocalInstruction : LocalAccessInstruction, IBoxableInstruction
    {
        internal LoadLocalInstruction(int index) : base(index)
        {
        }

        public Instruction BoxIfIndexMatches(int index)
        {
            if (index != base._index)
            {
                return null;
            }
            return InstructionList.LoadLocalBoxed(index);
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[frame.StackIndex++] = frame.Data[base._index];
            return 1;
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

