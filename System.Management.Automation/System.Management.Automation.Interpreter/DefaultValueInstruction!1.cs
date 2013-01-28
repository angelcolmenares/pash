namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class DefaultValueInstruction<T> : Instruction
    {
        internal DefaultValueInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Push(default(T));
            return 1;
        }

        public override string ToString()
        {
            return ("New " + typeof(T));
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

