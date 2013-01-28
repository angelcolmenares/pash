namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Management.Automation;
    using System.Management.Automation.Language;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal sealed class InterpretedFrame
    {
        private int _continuationIndex;
        private int[] _continuations;
        private static MethodInfo _Goto;
        internal InterpretedFrame _parent;
        private int _pendingContinuation;
        private object _pendingValue;
        private static MethodInfo _VoidGoto;
        public readonly StrongBox<object>[] Closure;
        public ExceptionHandler CurrentAbortHandler;
        public static readonly System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame> CurrentFrame = new System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>();
        public readonly object[] Data;
        public int InstructionIndex;
        internal readonly System.Management.Automation.Interpreter.Interpreter Interpreter;
        public int StackIndex;

        internal InterpretedFrame(System.Management.Automation.Interpreter.Interpreter interpreter, StrongBox<object>[] closure)
        {
            this.Interpreter = interpreter;
            this.StackIndex = interpreter.LocalCount;
            this.Data = new object[this.StackIndex + interpreter.Instructions.MaxStackDepth];
            int maxContinuationDepth = interpreter.Instructions.MaxContinuationDepth;
            if (maxContinuationDepth > 0)
            {
                this._continuations = new int[maxContinuationDepth];
            }
            this.Closure = closure;
            this._pendingContinuation = -1;
            this._pendingValue = System.Management.Automation.Interpreter.Interpreter.NoValue;
        }

        public void Dup()
        {
            int stackIndex = this.StackIndex;
            this.Data[stackIndex] = this.Data[stackIndex - 1];
            this.StackIndex = stackIndex + 1;
        }

        internal System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo Enter()
        {
            var storageInfo = CurrentFrame.GetStorageInfo();
            this._parent = storageInfo.Value;
            storageInfo.Value = this;
            return storageInfo;
        }

        public DebugInfo GetDebugInfo(int instructionIndex)
        {
            return DebugInfo.GetMatchingDebugInfo(this.Interpreter._debugInfos, instructionIndex);
        }

        public static InterpretedFrameInfo[] GetExceptionStackTrace(Exception exception)
        {
            return (exception.Data[typeof(InterpretedFrameInfo)] as InterpretedFrameInfo[]);
        }

        public IEnumerable<InterpretedFrameInfo> GetStackTraceDebugInfo()
        {
            InterpretedFrame parent = this;
            do
            {
                yield return new InterpretedFrameInfo(parent.Name, parent.GetDebugInfo(parent.InstructionIndex));
                parent = parent.Parent;
            }
            while (parent != null);
        }

        public int Goto(int labelIndex, object value, bool gotoExceptionHandler)
        {
            RuntimeLabel label = this.Interpreter._labels[labelIndex];
            if (this._continuationIndex == label.ContinuationStackDepth)
            {
                this.SetStackDepth(label.StackDepth);
                if (value != System.Management.Automation.Interpreter.Interpreter.NoValue)
                {
                    this.Data[this.StackIndex - 1] = value;
                }
                return (label.Index - this.InstructionIndex);
            }
            this._pendingContinuation = labelIndex;
            this._pendingValue = value;
            return this.YieldToCurrentContinuation();
        }

        public static IEnumerable<StackFrame> GroupStackFrames(IEnumerable<StackFrame> stackTrace)
        {
            bool iteratorVariable0 = false;
            foreach (StackFrame iteratorVariable1 in stackTrace)
            {
                if (IsInterpretedFrame(iteratorVariable1.GetMethod()))
                {
                    if (iteratorVariable0)
                    {
                        continue;
                    }
                    iteratorVariable0 = true;
                }
                else
                {
                    iteratorVariable0 = false;
                }
                yield return iteratorVariable1;
            }
        }

        public static bool IsInterpretedFrame(MethodBase method)
        {
            return ((method.DeclaringType == typeof(System.Management.Automation.Interpreter.Interpreter)) && (method.Name == "Run"));
        }

        internal bool IsJumpHappened()
        {
            return (this._pendingContinuation >= 0);
        }

        internal void Leave(System.Management.Automation.Interpreter.ThreadLocal<InterpretedFrame>.StorageInfo currentFrame)
        {
            currentFrame.Value = this._parent;
        }

        public object Peek()
        {
            return this.Data[this.StackIndex - 1];
        }

        public object Pop()
        {
            return this.Data[--this.StackIndex];
        }

        internal void PopPendingContinuation()
        {
            this._pendingValue = this.Pop();
            this._pendingContinuation = (int) this.Pop();
        }

        public void Push(bool value)
        {
            this.Data[this.StackIndex++] = value ? ScriptingRuntimeHelpers.True : ScriptingRuntimeHelpers.False;
        }

        public void Push(int value)
        {
            this.Data[this.StackIndex++] = ScriptingRuntimeHelpers.Int32ToObject(value);
        }

        public void Push(object value)
        {
            this.Data[this.StackIndex++] = value;
        }

        public void PushContinuation(int continuation)
        {
            this._continuations[this._continuationIndex++] = continuation;
        }

        internal void PushPendingContinuation()
        {
            this.Push(this._pendingContinuation);
            this.Push(this._pendingValue);
            this._pendingContinuation = -1;
            this._pendingValue = System.Management.Automation.Interpreter.Interpreter.NoValue;
        }

        public void RemoveContinuation()
        {
            this._continuationIndex--;
        }

        internal void SaveTraceToException(Exception exception)
        {
            if (exception.Data[typeof(InterpretedFrameInfo)] == null)
            {
                exception.Data[typeof(InterpretedFrameInfo)] = new List<InterpretedFrameInfo>(this.GetStackTraceDebugInfo()).ToArray();
            }
        }

        internal void SetStackDepth(int depth)
        {
            this.StackIndex = this.Interpreter.LocalCount + depth;
        }

        public int VoidGoto(int labelIndex)
        {
            return this.Goto(labelIndex, System.Management.Automation.Interpreter.Interpreter.NoValue, false);
        }

        public int YieldToCurrentContinuation()
        {
            RuntimeLabel label = this.Interpreter._labels[this._continuations[this._continuationIndex - 1]];
            this.SetStackDepth(label.StackDepth);
            return (label.Index - this.InstructionIndex);
        }

        public int YieldToPendingContinuation()
        {
            RuntimeLabel label = this.Interpreter._labels[this._pendingContinuation];
            if (label.ContinuationStackDepth < this._continuationIndex)
            {
                RuntimeLabel label2 = this.Interpreter._labels[this._continuations[this._continuationIndex - 1]];
                this.SetStackDepth(label2.StackDepth);
                return (label2.Index - this.InstructionIndex);
            }
            this.SetStackDepth(label.StackDepth);
            if (this._pendingValue != System.Management.Automation.Interpreter.Interpreter.NoValue)
            {
                this.Data[this.StackIndex - 1] = this._pendingValue;
            }
            this._pendingContinuation = -1;
            this._pendingValue = System.Management.Automation.Interpreter.Interpreter.NoValue;
            return (label.Index - this.InstructionIndex);
        }

        public System.Management.Automation.ExecutionContext ExecutionContext
        {
            get
            {
                return (System.Management.Automation.ExecutionContext) this.Data[1];
            }
        }

        public System.Management.Automation.Language.FunctionContext FunctionContext
        {
            get
            {
                return (System.Management.Automation.Language.FunctionContext) this.Data[0];
            }
        }

        internal static MethodInfo GotoMethod
        {
            get
            {
                return (_Goto ?? (_Goto = typeof(InterpretedFrame).GetMethod("Goto")));
            }
        }

        public string Name
        {
            get
            {
                return this.Interpreter._name;
            }
        }

        public InterpretedFrame Parent
        {
            get
            {
                return this._parent;
            }
        }

        internal static MethodInfo VoidGotoMethod
        {
            get
            {
                return (_VoidGoto ?? (_VoidGoto = typeof(InterpretedFrame).GetMethod("VoidGoto")));
            }
        }

        
    }
}

