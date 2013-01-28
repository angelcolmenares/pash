namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class PopInstruction : Instruction
    {
        internal static readonly PopInstruction Instance = new PopInstruction();

        private PopInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Pop();
            return 1;
        }

        public override string ToString()
        {
            return "Pop()";
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

