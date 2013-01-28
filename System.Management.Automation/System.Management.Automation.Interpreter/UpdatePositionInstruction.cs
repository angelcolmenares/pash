namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Language;

    internal class UpdatePositionInstruction : Instruction
    {
        private readonly bool _checkBreakpoints;
        private readonly int _sequencePoint;

        private UpdatePositionInstruction(bool checkBreakpoints, int sequencePoint)
        {
            this._checkBreakpoints = checkBreakpoints;
            this._sequencePoint = sequencePoint;
        }

        public static Instruction Create(int sequencePoint, bool checkBreakpoints)
        {
            return new UpdatePositionInstruction(checkBreakpoints, sequencePoint);
        }

        public override int Run(InterpretedFrame frame)
        {
            FunctionContext functionContext = frame.FunctionContext;
            ExecutionContext executionContext = frame.ExecutionContext;
            functionContext._currentSequencePointIndex = this._sequencePoint;
            if (this._checkBreakpoints && (executionContext._debuggingMode > 0))
            {
                executionContext.Debugger.OnSequencePointHit(functionContext);
            }
            return 1;
        }
    }
}

