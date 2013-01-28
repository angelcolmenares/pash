namespace System.Management.Automation.Language
{
    using System;

    public sealed class ScriptPosition : IScriptPosition
    {
        private readonly string _line;
        private readonly int _offsetInLine;
        private readonly int _scriptLineNumber;
        private readonly string _scriptName;

        public ScriptPosition(string scriptName, int scriptLineNumber, int offsetInLine, string line)
        {
            this._scriptName = scriptName;
            this._scriptLineNumber = scriptLineNumber;
            this._offsetInLine = offsetInLine;
            if (string.IsNullOrEmpty(line))
            {
                this._line = string.Empty;
            }
            else
            {
                this._line = line;
            }
        }

        public string GetFullScript()
        {
            return null;
        }

        public int ColumnNumber
        {
            get
            {
                return this._offsetInLine;
            }
        }

        public string File
        {
            get
            {
                return this._scriptName;
            }
        }

        public string Line
        {
            get
            {
                return this._line;
            }
        }

        public int LineNumber
        {
            get
            {
                return this._scriptLineNumber;
            }
        }

        public int Offset
        {
            get
            {
                return 0;
            }
        }
    }
}

