namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class StringFormatError : FormattingError
    {
        internal Exception exception;
        internal string formatString;
    }
}

