namespace System.Management.Automation.Language
{
    using System;

    public class ParseError
    {
        private readonly string _errorId;
        private readonly IScriptExtent _extent;
        private readonly bool _incompleteInput;
        private readonly string _message;

        public ParseError(IScriptExtent extent, string errorId, string message) : this(extent, errorId, message, false)
        {
        }

        internal ParseError(IScriptExtent extent, string errorId, string message, bool incompleteInput)
        {
            this._extent = extent;
            this._errorId = errorId;
            this._message = message;
            this._incompleteInput = incompleteInput;
        }

        public override string ToString()
        {
            return (PositionUtilities.VerboseMessage(this._extent) + "\n" + this._message);
        }

        public string ErrorId
        {
            get
            {
                return this._errorId;
            }
        }

        public IScriptExtent Extent
        {
            get
            {
                return this._extent;
            }
        }

        public bool IncompleteInput
        {
            get
            {
                return this._incompleteInput;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }
    }
}

