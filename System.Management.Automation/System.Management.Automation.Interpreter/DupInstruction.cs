namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class DupInstruction : Instruction
    {
        internal static readonly DupInstruction Instance = new DupInstruction();

        private DupInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[frame.StackIndex++] = frame.Peek();
            return 1;
        }

        public override string ToString()
        {
            return "Dup()";
        }

        public override int ConsumedStack
        {
            get
            {
                return 0;
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

