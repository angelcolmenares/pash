namespace System.Management.Automation.Interpreter
{
    using System;

    internal sealed class LeaveFinallyInstruction : Instruction
    {
        internal static readonly Instruction Instance = new LeaveFinallyInstruction();

        private LeaveFinallyInstruction()
        {
        }

        public override int Run(InterpretedFrame frame)
        {
            frame.PopPendingContinuation();
            if (!frame.IsJumpHappened())
            {
                return 1;
            }
            return frame.YieldToPendingContinuation();
        }

        public override int ConsumedStack
        {
            get
            {
                return 2;
            }
        }
    }
}

