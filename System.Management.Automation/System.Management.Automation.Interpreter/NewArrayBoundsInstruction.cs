namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class NewArrayBoundsInstruction : Instruction
    {
        private readonly Type _elementType;
        private readonly int _rank;

        internal NewArrayBoundsInstruction(Type elementType, int rank)
        {
            this._elementType = elementType;
            this._rank = rank;
        }

        public override int Run(InterpretedFrame frame)
        {
            int[] lengths = new int[this._rank];
            for (int i = this._rank - 1; i >= 0; i--)
            {
                lengths[i] = (int) frame.Pop();
            }
            Array array = Array.CreateInstance(this._elementType, lengths);
            frame.Push(array);
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return this._rank;
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

