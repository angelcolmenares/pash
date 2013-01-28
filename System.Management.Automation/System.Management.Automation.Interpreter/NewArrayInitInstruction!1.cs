namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class NewArrayInitInstruction<TElement> : Instruction
    {
        private readonly int _elementCount;

        internal NewArrayInitInstruction(int elementCount)
        {
            this._elementCount = elementCount;
        }

        public override int Run(InterpretedFrame frame)
        {
            TElement[] localArray = new TElement[this._elementCount];
            for (int i = this._elementCount - 1; i >= 0; i--)
            {
                localArray[i] = (TElement) frame.Pop();
            }
            frame.Push(localArray);
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return this._elementCount;
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

