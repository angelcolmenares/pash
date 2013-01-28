namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;

    internal sealed class IndentationManager
    {
        private Stack<FrameInfo> _frameInfoStack = new Stack<FrameInfo>();

        internal void Clear()
        {
            this._frameInfoStack.Clear();
        }

        private int ComputeLeftIndentation()
        {
            int num = 0;
            foreach (FrameInfo info in this._frameInfoStack)
            {
                num += info.leftIndentation;
            }
            return num;
        }

        private int ComputeRightIndentation()
        {
            int num = 0;
            foreach (FrameInfo info in this._frameInfoStack)
            {
                num += info.rightIndentation;
            }
            return num;
        }

        private void RemoveStackFrame()
        {
            this._frameInfoStack.Pop();
        }

        internal IDisposable StackFrame(FrameInfo frameInfo)
        {
            IndentationStackFrame frame = new IndentationStackFrame(this);
            this._frameInfoStack.Push(frameInfo);
            return frame;
        }

        internal int FirstLineIndentation
        {
            get
            {
                if (this._frameInfoStack.Count == 0)
                {
                    return 0;
                }
                return this._frameInfoStack.Peek().firstLine;
            }
        }

        internal int LeftIndentation
        {
            get
            {
                return this.ComputeLeftIndentation();
            }
        }

        internal int RightIndentation
        {
            get
            {
                return this.ComputeRightIndentation();
            }
        }

        private sealed class IndentationStackFrame : IDisposable
        {
            private IndentationManager _mgr;

            internal IndentationStackFrame(IndentationManager mgr)
            {
                this._mgr = mgr;
            }

            public void Dispose()
            {
                if (this._mgr != null)
                {
                    this._mgr.RemoveStackFrame();
                }
            }
        }
    }
}

