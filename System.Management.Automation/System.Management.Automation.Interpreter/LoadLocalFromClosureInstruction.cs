namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class LoadLocalFromClosureInstruction : LocalAccessInstruction
    {
        internal LoadLocalFromClosureInstruction(int index) : base(index)
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            StrongBox<object> box = frame.Closure[base._index];
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

