namespace System.Management.Automation.Language
{
    using System;

    internal class PositionHelper
    {
        private readonly string _filename;
        private int[] _lineStartMap;
        private readonly string _scriptText;

        internal PositionHelper(string filename, string scriptText)
        {
            this._filename = filename;
            this._scriptText = scriptText;
        }

        internal int ColumnFromOffset(int offset)
        {
            return ((offset - this._lineStartMap[this.LineFromOffset(offset) - 1]) + 1);
        }

        internal int LineFromOffset(int offset)
        {
            int num = Array.BinarySearch<int>(this._lineStartMap, offset);
            if (num < 0)
            {
                num = ~num - 1;
            }
            return (num + 1);
        }

        internal string Text(int line)
        {
            int startIndex = this._lineStartMap[line - 1];
            if (line < this._lineStartMap.Length)
            {
                int length = this._lineStartMap[line] - startIndex;
                return this.ScriptText.Substring(startIndex, length);
            }
            return this.ScriptText.Substring(startIndex);
        }

        public string File
        {
            get
            {
                return this._filename;
            }
        }

        internal int[] LineStartMap
        {
            set
            {
                this._lineStartMap = value;
            }
        }

        internal string ScriptText
        {
            get
            {
                return this._scriptText;
            }
        }
    }
}

