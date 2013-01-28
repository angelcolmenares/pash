namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal sealed class TryCatchFinallyHandler
    {
        private readonly ExceptionHandler[] _handlers;
        internal readonly int FinallyEndIndex;
        internal readonly int FinallyStartIndex;
        internal readonly int GotoEndTargetIndex;
        internal readonly int TryEndIndex;
        internal readonly int TryStartIndex;

        internal TryCatchFinallyHandler(int tryStart, int tryEnd, int gotoEndTargetIndex, ExceptionHandler[] handlers) : this(tryStart, tryEnd, gotoEndTargetIndex, 0x7fffffff, 0x7fffffff, handlers)
        {
        }

        internal TryCatchFinallyHandler(int tryStart, int tryEnd, int gotoEndTargetIndex, int finallyStart, int finallyEnd) : this(tryStart, tryEnd, gotoEndTargetIndex, finallyStart, finallyEnd, null)
        {
        }

        internal TryCatchFinallyHandler(int tryStart, int tryEnd, int gotoEndLabelIndex, int finallyStart, int finallyEnd, ExceptionHandler[] handlers)
        {
            this.TryStartIndex = 0x7fffffff;
            this.TryEndIndex = 0x7fffffff;
            this.FinallyStartIndex = 0x7fffffff;
            this.FinallyEndIndex = 0x7fffffff;
            this.GotoEndTargetIndex = 0x7fffffff;
            this.TryStartIndex = tryStart;
            this.TryEndIndex = tryEnd;
            this.FinallyStartIndex = finallyStart;
            this.FinallyEndIndex = finallyEnd;
            this.GotoEndTargetIndex = gotoEndLabelIndex;
            this._handlers = handlers;
            if (this._handlers != null)
            {
                foreach (ExceptionHandler handler in this._handlers)
                {
                    handler.SetParent(this);
                }
            }
        }

        internal int GotoHandler(InterpretedFrame frame, object exception, out ExceptionHandler handler)
        {
            handler = this._handlers.FirstOrDefault<ExceptionHandler>(t => t.Matches(exception.GetType()));
            if (handler == null)
            {
                return 0;
            }
            return frame.Goto(handler.LabelIndex, exception, true);
        }

        internal bool IsCatchBlockExist
        {
            get
            {
                return (this._handlers != null);
            }
        }

        internal bool IsFinallyBlockExist
        {
            get
            {
                return ((this.FinallyStartIndex != 0x7fffffff) && (this.FinallyEndIndex != 0x7fffffff));
            }
        }
    }
}

