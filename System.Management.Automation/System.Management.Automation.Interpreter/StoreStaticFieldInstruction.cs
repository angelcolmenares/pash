namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class StoreStaticFieldInstruction : Instruction
    {
        private readonly FieldInfo _field;

        public StoreStaticFieldInstruction(FieldInfo field)
        {
            this._field = field;
        }

        public override int Run(InterpretedFrame frame)
        {
            object obj2 = frame.Pop();
            this._field.SetValue(null, obj2);
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
                return 0;
            }
        }
    }
}

