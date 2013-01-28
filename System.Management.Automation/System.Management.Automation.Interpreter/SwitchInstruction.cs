namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;

    internal sealed class SwitchInstruction : Instruction
    {
        private readonly Dictionary<int, int> _cases;

        internal SwitchInstruction(Dictionary<int, int> cases)
        {
            this._cases = cases;
        }

        public override int Run(InterpretedFrame frame)
        {
            int num;
            if (!this._cases.TryGetValue((int) frame.Pop(), out num))
            {
                return 1;
            }
            return num;
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

