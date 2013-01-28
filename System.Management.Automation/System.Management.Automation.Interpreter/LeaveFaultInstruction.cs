namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class LeaveFaultInstruction : Instruction
    {
        private readonly bool _hasValue;
        internal static readonly Instruction NonVoid = new LeaveFaultInstruction(true);
        internal static readonly Instruction Void = new LeaveFaultInstruction(false);

        private LeaveFaultInstruction(bool hasValue)
        {
            this._hasValue = hasValue;
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.Pop();
            throw new RethrowException();
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
                if (!this._hasValue)
                {
                    return 0;
                }
                return 1;
            }
        }
    }
}

