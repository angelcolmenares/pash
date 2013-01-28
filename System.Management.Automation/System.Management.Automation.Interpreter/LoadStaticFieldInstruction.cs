namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class LoadStaticFieldInstruction : Instruction
    {
        private readonly FieldInfo _field;

        public LoadStaticFieldInstruction(FieldInfo field)
        {
            this._field = field;
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Push(this._field.GetValue(null));
            return 1;
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

