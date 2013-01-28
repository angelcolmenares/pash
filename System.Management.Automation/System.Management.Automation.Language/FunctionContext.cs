namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;

    internal class FunctionContext
    {
        internal List<LineBreakpoint> _boundBreakpoints;
        internal BitArray _breakPoints;
        internal int _currentSequencePointIndex;
        internal ExecutionContext _executionContext;
        internal string _functionName;
        internal MutableTuple _localsTuple;
        internal Pipe _outputPipe;
        internal ScriptBlock _scriptBlock;
        internal IScriptExtent[] _sequencePoints;
        internal List<Tuple<Type[], Action<FunctionContext>[], Type[]>> _traps = new List<Tuple<Type[], Action<FunctionContext>[], Type[]>>();

        internal void PopTrapHandlers()
        {
            this._traps.RemoveAt(this._traps.Count - 1);
        }

        internal void PushTrapHandlers(Type[] type, Action<FunctionContext>[] handler, Type[] tupleType)
        {
            this._traps.Add(Tuple.Create<Type[], Action<FunctionContext>[], Type[]>(type, handler, tupleType));
        }

        internal IScriptExtent CurrentPosition
        {
            get
            {
                if (this._sequencePoints == null)
                {
                    return PositionUtilities.EmptyExtent;
                }
                return this._sequencePoints[this._currentSequencePointIndex];
            }
        }
    }
}

