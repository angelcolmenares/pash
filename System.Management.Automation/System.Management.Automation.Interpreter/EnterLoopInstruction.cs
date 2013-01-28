namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Management.Automation.Language;
    using System.Threading;

    internal sealed class EnterLoopInstruction : Instruction
    {
        private Dictionary<ParameterExpression, LocalVariable> _closureVariables;
        private int _compilationThreshold;
        private readonly int _instructionIndex;
        private PowerShellLoopExpression _loop;
        private int _loopEnd;
        private Dictionary<ParameterExpression, LocalVariable> _variables;

        internal EnterLoopInstruction(PowerShellLoopExpression loop, LocalVariables locals, int compilationThreshold, int instructionIndex)
        {
            this._loop = loop;
            this._variables = locals.CopyLocals();
            this._closureVariables = locals.ClosureVariables;
            this._compilationThreshold = compilationThreshold;
            this._instructionIndex = instructionIndex;
        }

        private void Compile(object frameObj)
        {
            if (!this.Compiled)
            {
                lock (this)
                {
                    if (!this.Compiled)
                    {
                        InterpretedFrame frame = (InterpretedFrame) frameObj;
                        LoopCompiler compiler = new LoopCompiler(this._loop, frame.Interpreter.LabelMapping, this._variables, this._closureVariables, this._instructionIndex, this._loopEnd);
                        frame.Interpreter.Instructions.Instructions[this._instructionIndex] = new CompiledLoopInstruction(compiler.CreateDelegate());
                        this._loop = null;
                        this._variables = null;
                        this._closureVariables = null;
                    }
                }
            }
        }

        internal void FinishLoop(int loopEnd)
        {
            this._loopEnd = loopEnd;
        }

        public override int Run(InterpretedFrame frame)
        {
            if (this._compilationThreshold-- == 0)
            {
                if (frame.Interpreter.CompileSynchronously)
                {
                    this.Compile(frame);
                }
                else
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.Compile), frame);
                }
            }
            return 1;
        }

        private bool Compiled
        {
            get
            {
                return (this._loop == null);
            }
        }
    }
}

