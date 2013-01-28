namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class AssignLocalBoxedInstruction : LocalAccessInstruction
    {
        internal AssignLocalBoxedInstruction(int index) : base(index)
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            StrongBox<object> box = (StrongBox<object>) frame.Data[base._index];
            box.Value = frame.Peek();
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

