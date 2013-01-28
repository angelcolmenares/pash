namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal class TooManyErrorsException : TypeInfoDataBaseLoaderException
    {
        internal int errorCount;
    }
}

