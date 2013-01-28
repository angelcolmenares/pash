namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class CreateDelegateInstruction : Instruction
    {
        private readonly LightDelegateCreator _creator;

        internal CreateDelegateInstruction(LightDelegateCreator delegateCreator)
        {
            this._creator = delegateCreator;
        }

        public override int Run(InterpretedFrame frame)
        {
            StrongBox<object>[] boxArray;
            if (this.ConsumedStack > 0)
            {
                boxArray = new StrongBox<object>[this.ConsumedStack];
                for (int i = boxArray.Length - 1; i >= 0; i--)
                {
                    boxArray[i] = (StrongBox<object>) frame.Pop();
                }
            }
            else
            {
                boxArray = null;
            }
            Delegate delegate2 = this._creator.CreateDelegate(boxArray);
            frame.Push(delegate2);
            return 1;
        }

        public override int ConsumedStack
        {
            get
            {
                return this._creator.Interpreter.ClosureSize;
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

