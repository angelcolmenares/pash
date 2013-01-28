namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class RuntimeVariablesInstruction : Instruction
    {
        private readonly int _count;

        public RuntimeVariablesInstruction(int count)
        {
            this._count = count;
        }

        public override int Run(InterpretedFrame frame)
        {
            IStrongBox[] boxes = new IStrongBox[this._count];
            for (int i = boxes.Length - 1; i >= 0; i--)
            {
                boxes[i] = (IStrongBox) frame.Pop();
            }
            frame.Push(System.Management.Automation.Interpreter.RuntimeVariables.Create(boxes));
            return 1;
        }

        public override string ToString()
        {
            return "GetRuntimeVariables()";
        }

        public override int ConsumedStack
        {
            get
            {
                return this._count;
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

