namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class TypeAsInstruction<T> : Instruction
    {
        internal TypeAsInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            object obj2 = frame.Pop();
            if (obj2 is T)
            {
                frame.Push(obj2);
            }
            else
            {
                frame.Push(null);
            }
            return 1;
        }

        public override string ToString()
        {
            return ("TypeAs " + typeof(T).Name);
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

