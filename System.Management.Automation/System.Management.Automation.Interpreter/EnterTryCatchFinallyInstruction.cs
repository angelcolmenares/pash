namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Threading;

    internal sealed class EnterTryCatchFinallyInstruction : IndexedBranchInstruction
    {
        private readonly bool _hasFinally;
        private TryCatchFinallyHandler _tryHandler;

        private EnterTryCatchFinallyInstruction(int targetIndex, bool hasFinally) : base(targetIndex)
        {
            this._hasFinally = hasFinally;
        }

        internal static EnterTryCatchFinallyInstruction CreateTryCatch()
        {
            return new EnterTryCatchFinallyInstruction(0x7fffffff, false);
        }

        internal static EnterTryCatchFinallyInstruction CreateTryFinally(int labelIndex)
        {
            return new EnterTryCatchFinallyInstruction(labelIndex, true);
        }

        public override int Run(InterpretedFrame frame)
        {
            if (this._hasFinally)
            {
                frame.PushContinuation(base._labelIndex);
            }
            int instructionIndex = frame.InstructionIndex;
            frame.InstructionIndex++;
            Instruction[] instructions = frame.Interpreter.Instructions.Instructions;
            try
            {
                int index = frame.InstructionIndex;
                while ((index >= this._tryHandler.TryStartIndex) && (index < this._tryHandler.TryEndIndex))
                {
                    index += instructions[index].Run(frame);
                    frame.InstructionIndex = index;
                }
                if (index == this._tryHandler.GotoEndTargetIndex)
                {
                    frame.InstructionIndex += instructions[index].Run(frame);
                }
            }
            catch (RethrowException)
            {
                throw;
            }
            catch (Exception exception)
            {
                ExceptionHandler handler;
                frame.SaveTraceToException(exception);
                if (!this._tryHandler.IsCatchBlockExist)
                {
                    throw;
                }
                frame.InstructionIndex += this._tryHandler.GotoHandler(frame, exception, out handler);
                if (handler == null)
                {
                    throw;
                }
                ThreadAbortException exception2 = exception as ThreadAbortException;
                if (exception2 != null)
                {
                    System.Management.Automation.Interpreter.Interpreter.AnyAbortException = exception2;
                    frame.CurrentAbortHandler = handler;
                }
                bool flag = false;
                try
                {
                    int num3 = frame.InstructionIndex;
                    while ((num3 >= handler.HandlerStartIndex) && (num3 < handler.HandlerEndIndex))
                    {
                        num3 += instructions[num3].Run(frame);
                        frame.InstructionIndex = num3;
                    }
                    if (num3 == this._tryHandler.GotoEndTargetIndex)
                    {
                        frame.InstructionIndex += instructions[num3].Run(frame);
                    }
                }
                catch (RethrowException)
                {
                    flag = true;
                }
                if (flag)
                {
                    throw;
                }
            }
            finally
            {
                if (this._tryHandler.IsFinallyBlockExist)
                {
                    int num4 = frame.InstructionIndex = this._tryHandler.FinallyStartIndex;
                    while ((num4 >= this._tryHandler.FinallyStartIndex) && (num4 < this._tryHandler.FinallyEndIndex))
                    {
                        num4 += instructions[num4].Run(frame);
                        frame.InstructionIndex = num4;
                    }
                }
            }
            return (frame.InstructionIndex - instructionIndex);
        }

        internal void SetTryHandler(TryCatchFinallyHandler tryHandler)
        {
            this._tryHandler = tryHandler;
        }

        public override string ToString()
        {
            if (!this._hasFinally)
            {
                return "EnterTryCatch";
            }
            return ("EnterTryFinally[" + base._labelIndex + "]");
        }

        public override string InstructionName
        {
            get
            {
                if (!this._hasFinally)
                {
                    return "EnterTryCatch";
                }
                return "EnterTryFinally";
            }
        }

        public override int ProducedContinuations
        {
            get
            {
                if (!this._hasFinally)
                {
                    return 0;
                }
                return 1;
            }
        }
    }
}

