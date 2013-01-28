namespace Microsoft.PowerShell.Commands
{
    using System;

    internal class EventWriteException : Exception
    {
        internal EventWriteException(string msg) : base(msg)
        {
        }

        internal EventWriteException(string msg, Exception innerException) : base(msg, innerException)
        {
        }
    }
}

