namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class FormattingError
    {
        internal object sourceObject;

        protected FormattingError()
        {
        }
    }
}

