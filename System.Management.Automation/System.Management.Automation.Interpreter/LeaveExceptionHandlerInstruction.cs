using System;

namespace System.Management.Automation.Interpreter
{
    internal sealed class LeaveExceptionHandlerInstruction : IndexedBranchInstruction
    {
        private static LeaveExceptionHandlerInstruction[] Cache;

        private readonly bool _hasValue;

        public override int ConsumedStack
        {
            get
            {
                if (this._hasValue)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public override int ProducedStack
        {
            get
            {
                if (this._hasValue)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        static LeaveExceptionHandlerInstruction()
        {
            LeaveExceptionHandlerInstruction.Cache = new LeaveExceptionHandlerInstruction[64];
        }

        private LeaveExceptionHandlerInstruction(int labelIndex, bool hasValue)
            : base(labelIndex)
        {
            this._hasValue = hasValue;
        }

        internal static LeaveExceptionHandlerInstruction Create(int labelIndex, bool hasValue)
        {
            int num;
            if (labelIndex >= 32)
            {
                return new LeaveExceptionHandlerInstruction(labelIndex, hasValue);
            }
            else
            {
                int num1 = 2 * labelIndex;
                if (hasValue)
                {
                    num = 1;
                }
                else
                {
                    num = 0;
                }
                int num2 = num1 | num;
                LeaveExceptionHandlerInstruction cache = LeaveExceptionHandlerInstruction.Cache[num2];
                LeaveExceptionHandlerInstruction leaveExceptionHandlerInstruction = cache;
                if (cache == null)
                {
                    LeaveExceptionHandlerInstruction leaveExceptionHandlerInstruction1 = new LeaveExceptionHandlerInstruction(labelIndex, hasValue);
                    LeaveExceptionHandlerInstruction leaveExceptionHandlerInstruction2 = leaveExceptionHandlerInstruction1;
                    LeaveExceptionHandlerInstruction.Cache[num2] = leaveExceptionHandlerInstruction1;
                    leaveExceptionHandlerInstruction = leaveExceptionHandlerInstruction2;
                }
                return leaveExceptionHandlerInstruction;
            }
        }

        public override int Run(InterpretedFrame frame)
        {
            Interpreter.AbortThreadIfRequested(frame, this._labelIndex);
            return base.GetLabel(frame).Index - frame.InstructionIndex;
        }
    }
}