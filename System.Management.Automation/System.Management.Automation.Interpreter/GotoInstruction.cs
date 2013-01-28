using System;

namespace System.Management.Automation.Interpreter
{
    internal sealed class GotoInstruction : IndexedBranchInstruction
    {
        private const int Variants = 4;

        private readonly static GotoInstruction[] Cache;

        private readonly bool _hasResult;

        private readonly bool _hasValue;

        public override int ConsumedContinuations
        {
            get
            {
                return 0;
            }
        }

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

        public override int ProducedContinuations
        {
            get
            {
                return 0;
            }
        }

        public override int ProducedStack
        {
            get
            {
                if (this._hasResult)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        static GotoInstruction()
        {
            GotoInstruction.Cache = new GotoInstruction[128];
        }

        private GotoInstruction(int targetIndex, bool hasResult, bool hasValue)
            : base(targetIndex)
        {
            this._hasResult = hasResult;
            this._hasValue = hasValue;
        }

        internal static GotoInstruction Create(int labelIndex, bool hasResult, bool hasValue)
        {
            int num;
            int num1;
            if (labelIndex >= 32)
            {
                return new GotoInstruction(labelIndex, hasResult, hasValue);
            }
            else
            {
                int num2 = 4 * labelIndex;
                if (hasResult)
                {
                    num = 2;
                }
                else
                {
                    num = 0;
                }
                int num3 = num2 | num;
                if (hasValue)
                {
                    num1 = 1;
                }
                else
                {
                    num1 = 0;
                }
                int num4 = num3 | num1;
                GotoInstruction cache = GotoInstruction.Cache[num4];
                GotoInstruction gotoInstruction = cache;
                if (cache == null)
                {
                    GotoInstruction gotoInstruction1 = new GotoInstruction(labelIndex, hasResult, hasValue);
                    GotoInstruction gotoInstruction2 = gotoInstruction1;
                    GotoInstruction.Cache[num4] = gotoInstruction1;
                    gotoInstruction = gotoInstruction2;
                }
                return gotoInstruction;
            }
        }

        public override int Run(InterpretedFrame frame)
        {
            object noValue;
            Interpreter.AbortThreadIfRequested(frame, this._labelIndex);
            InterpretedFrame interpretedFrame = frame;
            int num = this._labelIndex;
            if (this._hasValue)
            {
                noValue = frame.Pop();
            }
            else
            {
                noValue = Interpreter.NoValue;
            }
            return interpretedFrame.Goto(num, noValue, false);
        }
    }
}