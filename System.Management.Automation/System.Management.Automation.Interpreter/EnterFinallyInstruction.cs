using System;

namespace System.Management.Automation.Interpreter
{
    internal sealed class EnterFinallyInstruction : IndexedBranchInstruction
    {
        private readonly static EnterFinallyInstruction[] Cache;

        public override int ConsumedContinuations
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
                return 2;
            }
        }

        static EnterFinallyInstruction()
        {
            EnterFinallyInstruction.Cache = new EnterFinallyInstruction[32];
        }

        private EnterFinallyInstruction(int labelIndex)
            : base(labelIndex)
        {
        }

        internal static EnterFinallyInstruction Create(int labelIndex)
        {
            if (labelIndex >= 32)
            {
                return new EnterFinallyInstruction(labelIndex);
            }
            else
            {
                EnterFinallyInstruction cache = EnterFinallyInstruction.Cache[labelIndex];
                EnterFinallyInstruction enterFinallyInstruction = cache;
                if (cache == null)
                {
                    EnterFinallyInstruction enterFinallyInstruction1 = new EnterFinallyInstruction(labelIndex);
                    EnterFinallyInstruction enterFinallyInstruction2 = enterFinallyInstruction1;
                    EnterFinallyInstruction.Cache[labelIndex] = enterFinallyInstruction1;
                    enterFinallyInstruction = enterFinallyInstruction2;
                }
                return enterFinallyInstruction;
            }
        }

        public override int Run(InterpretedFrame frame)
        {
            if (!frame.IsJumpHappened())
            {
                frame.SetStackDepth(base.GetLabel(frame).StackDepth);
            }
            frame.PushPendingContinuation();
            frame.RemoveContinuation();
            return 1;
        }
    }
}