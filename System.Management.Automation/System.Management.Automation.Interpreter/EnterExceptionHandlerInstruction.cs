namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class EnterExceptionHandlerInstruction : Instruction
    {
        private readonly bool _hasValue;
        internal static readonly EnterExceptionHandlerInstruction NonVoid = new EnterExceptionHandlerInstruction(true);
        internal static readonly EnterExceptionHandlerInstruction Void = new EnterExceptionHandlerInstruction(false);

        private EnterExceptionHandlerInstruction(bool hasValue)
        {
            this._hasValue = hasValue;
        }

        public override int Run(InterpretedFrame frame)
        {
            return 1;
        }

        public override int ConsumedStack
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

        public override int ProducedStack
        {
            get
            {
                return 1;
            }
        }
    }
}

