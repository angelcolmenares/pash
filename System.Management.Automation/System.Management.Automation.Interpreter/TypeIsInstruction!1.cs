namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class TypeIsInstruction<T> : Instruction
    {
        internal TypeIsInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Push(ScriptingRuntimeHelpers.BooleanToObject(frame.Pop() is T));
            return 1;
        }

        public override string ToString()
        {
            return ("TypeIs " + typeof(T).Name);
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

