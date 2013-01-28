namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class ThrowInstruction : Instruction
    {
        private readonly bool _hasResult;
        private readonly bool _rethrow;
        internal static readonly ThrowInstruction Rethrow = new ThrowInstruction(true, true);
        internal static readonly ThrowInstruction Throw = new ThrowInstruction(true, false);
        internal static readonly ThrowInstruction VoidRethrow = new ThrowInstruction(false, true);
        internal static readonly ThrowInstruction VoidThrow = new ThrowInstruction(false, false);

        private ThrowInstruction(bool hasResult, bool isRethrow)
        {
            this._hasResult = hasResult;
            this._rethrow = isRethrow;
        }

        public override int Run(InterpretedFrame frame)
        {
            Exception exception = (Exception) frame.Pop();
            if (this._rethrow)
            {
                throw new RethrowException();
            }
            throw exception;
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
                if (!this._hasResult)
                {
                    return 0;
                }
                return 1;
            }
        }
    }
}

