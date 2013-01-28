namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class NewArrayInstruction<TElement> : Instruction
    {
        internal NewArrayInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            int num = (int) frame.Pop();
            frame.Push(new TElement[num]);
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

