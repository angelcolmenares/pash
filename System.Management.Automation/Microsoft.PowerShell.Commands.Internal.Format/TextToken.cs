namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal sealed class TextToken : FormatToken
    {
        internal StringResourceReference resource;
        internal string text;
    }
}

