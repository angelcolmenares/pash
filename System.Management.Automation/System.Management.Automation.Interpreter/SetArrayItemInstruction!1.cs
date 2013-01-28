namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class SetArrayItemInstruction<TElement> : Instruction
    {
        internal SetArrayItemInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            TElement local = (TElement) frame.Pop();
            int index = (int) frame.Pop();
            TElement[] localArray = (TElement[]) frame.Pop();
            localArray[index] = local;
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return 3;
            }
        }

        public override string InstructionName
        {
            get
            {
                return "SetArrayItem";
            }
        }

        public override int ProducedStack
        {
            get
            {
                return 0;
            }
        }
    }
}

