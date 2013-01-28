namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class Interpreter
    {
        private readonly Dictionary<ParameterExpression, LocalVariable> _closureVariables;
        internal readonly int _compilationThreshold;
        internal readonly DebugInfo[] _debugInfos;
        private readonly InstructionArray _instructions;
        private readonly HybridReferenceDictionary<LabelTarget, BranchLabel> _labelMapping;
        internal readonly RuntimeLabel[] _labels;
        private readonly int _localCount;
        internal readonly string _name;
        internal readonly object[] _objects;
        [ThreadStatic]
        internal static ThreadAbortException AnyAbortException = null;
        internal static readonly object NoValue = new object();
        internal const int RethrowOnReturn = 0x7fffffff;

        internal Interpreter(string name, LocalVariables locals, HybridReferenceDictionary<LabelTarget, BranchLabel> labelMapping, InstructionArray instructions, DebugInfo[] debugInfos, int compilationThreshold)
        {
            this._name = name;
            this._localCount = locals.LocalCount;
            this._closureVariables = locals.ClosureVariables;
            this._instructions = instructions;
            this._objects = instructions.Objects;
            this._labels = instructions.Labels;
            this._labelMapping = labelMapping;
            this._debugInfos = debugInfos;
            this._compilationThreshold = compilationThreshold;
        }

        internal static void AbortThreadIfRequested(InterpretedFrame frame, int targetLabelIndex)
        {
            ExceptionHandler currentAbortHandler = frame.CurrentAbortHandler;
            int index = frame.Interpreter._labels[targetLabelIndex].Index;
            if (((currentAbortHandler != null) && !currentAbortHandler.IsInsideCatchBlock(index)) && !currentAbortHandler.IsInsideFinallyBlock(index))
            {
                frame.CurrentAbortHandler = null;
                Thread currentThread = Thread.CurrentThread;
                if ((currentThread.ThreadState & ThreadState.AbortRequested) != ThreadState.Running)
                {
                    currentThread.Abort(AnyAbortException.ExceptionState);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Run(InterpretedFrame frame)
        {
            Instruction[] instructions = this._instructions.Instructions;
            int instructionIndex = frame.InstructionIndex;
            while (instructionIndex < instructions.Length)
            {
                instructionIndex += instructions[instructionIndex].Run(frame);
                frame.InstructionIndex = instructionIndex;
            }
        }

        internal int ClosureSize
        {
            get
            {
                if (this._closureVariables == null)
                {
                    return 0;
                }
                return this._closureVariables.Count;
            }
        }

        internal Dictionary<ParameterExpression, LocalVariable> ClosureVariables
        {
            get
            {
                return this._closureVariables;
            }
        }

        internal bool CompileSynchronously
        {
            get
            {
                return (this._compilationThreshold <= 1);
            }
        }

        internal InstructionArray Instructions
        {
            get
            {
                return this._instructions;
            }
        }

        internal HybridReferenceDictionary<LabelTarget, BranchLabel> LabelMapping
        {
            get
            {
                return this._labelMapping;
            }
        }

        internal int LocalCount
        {
            get
            {
                return this._localCount;
            }
        }

        internal int ReturnAndRethrowLabelIndex
        {
            get
            {
                return (this._labels.Length - 1);
            }
        }
    }
}

