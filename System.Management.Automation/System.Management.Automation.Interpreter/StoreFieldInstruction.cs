namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Reflection;

    internal sealed class StoreFieldInstruction : Instruction
    {
        private readonly FieldInfo _field;

        public StoreFieldInstruction(FieldInfo field)
        {
            this._field = field;
        }

        public override int Run(InterpretedFrame frame)
        {
            object obj2 = frame.Pop();
            object obj3 = frame.Pop();
            this._field.SetValue(obj3, obj2);
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return 2;
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

