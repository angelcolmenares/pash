namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class CompiledLoopInstruction : Instruction
    {
        private readonly Func<object[], StrongBox<object>[], InterpretedFrame, int> _compiledLoop;

        public CompiledLoopInstruction(Func<object[], StrongBox<object>[], InterpretedFrame, int> compiledLoop)
        {
            this._compiledLoop = compiledLoop;
        }

        public override int Run(InterpretedFrame frame)
        {
            return this._compiledLoop(frame.Data, frame.Closure, frame);
        }
    }
}

