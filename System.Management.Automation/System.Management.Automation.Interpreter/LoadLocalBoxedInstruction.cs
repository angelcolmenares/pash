namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class LoadLocalBoxedInstruction : LocalAccessInstruction
    {
        internal LoadLocalBoxedInstruction(int index) : base(index)
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            StrongBox<object> box = (StrongBox<object>) frame.Data[base._index];
            frame.Data[frame.StackIndex++] = box.Value;
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

