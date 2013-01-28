namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerTypeProxy(typeof(InstructionArray.DebugView))]
    internal struct InstructionArray
    {
        internal readonly int MaxStackDepth;
        internal readonly int MaxContinuationDepth;
        internal readonly Instruction[] Instructions;
        internal readonly object[] Objects;
        internal readonly RuntimeLabel[] Labels;
        internal readonly List<KeyValuePair<int, object>> DebugCookies;
        internal InstructionArray(int maxStackDepth, int maxContinuationDepth, Instruction[] instructions, object[] objects, RuntimeLabel[] labels, List<KeyValuePair<int, object>> debugCookies)
        {
            this.MaxStackDepth = maxStackDepth;
            this.MaxContinuationDepth = maxContinuationDepth;
            this.Instructions = instructions;
            this.DebugCookies = debugCookies;
            this.Objects = objects;
            this.Labels = labels;
        }

        internal int Length
        {
            get
            {
                return this.Instructions.Length;
            }
        }
        internal sealed class DebugView
        {
            private readonly InstructionArray _array;

            public DebugView(InstructionArray array)
            {
                this._array = array;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public InstructionList.DebugView.InstructionView[] A0
            {
                get
                {
                    return InstructionList.DebugView.GetInstructionViews(this._array.Instructions, this._array.Objects, index => this._array.Labels[index].Index, this._array.DebugCookies);
                }
            }
        }
    }
}

