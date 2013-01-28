namespace System.Management.Automation.Language
{
    using System;
    using System.Globalization;
    using System.Management.Automation;

    public sealed class ScriptExtent : IScriptExtent
    {
        private ScriptPosition _endPosition;
        private ScriptPosition _startPosition;

        private ScriptExtent()
        {
        }

        public ScriptExtent(ScriptPosition startPosition, ScriptPosition endPosition)
        {
            this._startPosition = startPosition;
            this._endPosition = endPosition;
        }

        internal static ScriptExtent FromPSObjectForRemoting(PSObject serializedScriptExtent)
        {
            ScriptExtent extent = new ScriptExtent();
            extent.PopulateFromSerializedInfo(serializedScriptExtent);
            return extent;
        }

        private void PopulateFromSerializedInfo(PSObject serializedScriptExtent)
        {
            string propertyValue = RemotingDecoder.GetPropertyValue<string>(serializedScriptExtent, "ScriptExtent_File");
            int scriptLineNumber = RemotingDecoder.GetPropertyValue<int>(serializedScriptExtent, "ScriptExtent_StartLineNumber");
            int offsetInLine = RemotingDecoder.GetPropertyValue<int>(serializedScriptExtent, "ScriptExtent_StartColumnNumber");
            int num3 = RemotingDecoder.GetPropertyValue<int>(serializedScriptExtent, "ScriptExtent_EndLineNumber");
            int num4 = RemotingDecoder.GetPropertyValue<int>(serializedScriptExtent, "ScriptExtent_EndColumnNumber");
            ScriptPosition position = new ScriptPosition(propertyValue, scriptLineNumber, offsetInLine, null);
            ScriptPosition position2 = new ScriptPosition(propertyValue, num3, num4, null);
            this._startPosition = position;
            this._endPosition = position2;
        }

        internal void ToPSObjectForRemoting(PSObject dest)
        {
            RemotingEncoder.AddNoteProperty<string>(dest, "ScriptExtent_File", () => this.File);
            RemotingEncoder.AddNoteProperty<int>(dest, "ScriptExtent_StartLineNumber", () => this.StartLineNumber);
            RemotingEncoder.AddNoteProperty<int>(dest, "ScriptExtent_StartColumnNumber", () => this.StartColumnNumber);
            RemotingEncoder.AddNoteProperty<int>(dest, "ScriptExtent_EndLineNumber", () => this.EndLineNumber);
            RemotingEncoder.AddNoteProperty<int>(dest, "ScriptExtent_EndColumnNumber", () => this.EndColumnNumber);
        }

        public int EndColumnNumber
        {
            get
            {
                return this._endPosition.ColumnNumber;
            }
        }

        public int EndLineNumber
        {
            get
            {
                return this._endPosition.LineNumber;
            }
        }

        public int EndOffset
        {
            get
            {
                return 0;
            }
        }

        public IScriptPosition EndScriptPosition
        {
            get
            {
                return this._endPosition;
            }
        }

        public string File
        {
            get
            {
                return this._startPosition.File;
            }
        }

        public int StartColumnNumber
        {
            get
            {
                return this._startPosition.ColumnNumber;
            }
        }

        public int StartLineNumber
        {
            get
            {
                return this._startPosition.LineNumber;
            }
        }

        public int StartOffset
        {
            get
            {
                return 0;
            }
        }

        public IScriptPosition StartScriptPosition
        {
            get
            {
                return this._startPosition;
            }
        }

        public string Text
        {
            get
            {
                if (this.EndColumnNumber <= 0)
                {
                    return string.Empty;
                }
                if (this.StartLineNumber == this.EndLineNumber)
                {
                    return this._startPosition.Line.Substring(this._startPosition.ColumnNumber - 1, this._endPosition.ColumnNumber - this._startPosition.ColumnNumber);
                }
                return string.Format(CultureInfo.InvariantCulture, "{0}...{1}", new object[] { this._startPosition.Line.Substring(this._startPosition.ColumnNumber), this._endPosition.Line.Substring(0, this._endPosition.ColumnNumber) });
            }
        }
    }
}

