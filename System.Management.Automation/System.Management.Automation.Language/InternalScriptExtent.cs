namespace System.Management.Automation.Language
{
    using System;

    internal sealed class InternalScriptExtent : IScriptExtent
    {
        private readonly int _endOffset;
        private readonly System.Management.Automation.Language.PositionHelper _positionHelper;
        private readonly int _startOffset;

        internal InternalScriptExtent(System.Management.Automation.Language.PositionHelper _positionHelper, int startOffset, int endOffset)
        {
            this._positionHelper = _positionHelper;
            this._startOffset = startOffset;
            this._endOffset = endOffset;
        }

        public override string ToString()
        {
            return this.Text;
        }

        public int EndColumnNumber
        {
            get
            {
                return this._positionHelper.ColumnFromOffset(this._endOffset);
            }
        }

        public int EndLineNumber
        {
            get
            {
                return this._positionHelper.LineFromOffset(this._endOffset);
            }
        }

        public int EndOffset
        {
            get
            {
                return this._endOffset;
            }
        }

        public IScriptPosition EndScriptPosition
        {
            get
            {
                return new InternalScriptPosition(this._positionHelper, this._endOffset);
            }
        }

        public string File
        {
            get
            {
                return this._positionHelper.File;
            }
        }

        internal System.Management.Automation.Language.PositionHelper PositionHelper
        {
            get
            {
                return this._positionHelper;
            }
        }

        public int StartColumnNumber
        {
            get
            {
                return this._positionHelper.ColumnFromOffset(this._startOffset);
            }
        }

        public int StartLineNumber
        {
            get
            {
                return this._positionHelper.LineFromOffset(this._startOffset);
            }
        }

        public int StartOffset
        {
            get
            {
                return this._startOffset;
            }
        }

        public IScriptPosition StartScriptPosition
        {
            get
            {
                return new InternalScriptPosition(this._positionHelper, this._startOffset);
            }
        }

        public string Text
        {
            get
            {
                if (this._startOffset > this._positionHelper.ScriptText.Length)
                {
                    return "";
                }
                return this._positionHelper.ScriptText.Substring(this._startOffset, this._endOffset - this._startOffset);
            }
        }
    }
}

