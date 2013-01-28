namespace Microsoft.PowerShell.Commands
{
    using System;

    internal sealed class BackReaderEncodingNotSupportedException : NotSupportedException
    {
        private readonly string _encodingName;

        internal BackReaderEncodingNotSupportedException(string encodingName)
        {
            this._encodingName = encodingName;
        }

        internal BackReaderEncodingNotSupportedException(string message, string encodingName) : base(message)
        {
            this._encodingName = encodingName;
        }

        internal string EncodingName
        {
            get
            {
                return this._encodingName;
            }
        }
    }
}

