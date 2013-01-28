namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class LoadFieldInstruction : Instruction
    {
        private readonly FieldInfo _field;

        public LoadFieldInstruction(FieldInfo field)
        {
            this._field = field;
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Push(this._field.GetValue(frame.Pop()));
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

