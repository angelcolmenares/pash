namespace System.Management.Automation.Language
{
    using System;

    internal sealed class InternalScriptPosition : IScriptPosition
    {
        private readonly int _offset;
        private readonly PositionHelper _positionHelper;

        internal InternalScriptPosition(PositionHelper _positionHelper, int offset)
        {
            this._positionHelper = _positionHelper;
            this._offset = offset;
        }

        internal InternalScriptPosition CloneWithNewOffset(int offset)
        {
            return new InternalScriptPosition(this._positionHelper, offset);
        }

        public string GetFullScript()
        {
            return this._positionHelper.ScriptText;
        }

        public int ColumnNumber
        {
            get
            {
                return this._positionHelper.ColumnFromOffset(this._offset);
            }
        }

        public string File
        {
            get
            {
                return this._positionHelper.File;
            }
        }

        public string Line
        {
            get
            {
                return this._positionHelper.Text(this.LineNumber);
            }
        }

        public int LineNumber
        {
            get
            {
                return this._positionHelper.LineFromOffset(this._offset);
            }
        }

        public int Offset
        {
            get
            {
                return this._offset;
            }
        }
    }
}

