namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class LoadObjectInstruction : Instruction
    {
        private readonly object _value;

        internal LoadObjectInstruction(object value)
        {
            this._value = value;
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Data[frame.StackIndex++] = this._value;
            return 1;
        }

        public override string ToString()
        {
            return ("LoadObject(" + (this._value ?? "null") + ")");
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

