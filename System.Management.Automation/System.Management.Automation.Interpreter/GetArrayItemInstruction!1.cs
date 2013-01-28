namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class GetArrayItemInstruction<TElement> : Instruction
    {
        internal GetArrayItemInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            int index = (int) frame.Pop();
            TElement[] localArray = (TElement[]) frame.Pop();
            frame.Push(localArray[index]);
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return 2;
            }
        }

        public override string InstructionName
        {
            get
            {
                return "GetArrayItem";
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

