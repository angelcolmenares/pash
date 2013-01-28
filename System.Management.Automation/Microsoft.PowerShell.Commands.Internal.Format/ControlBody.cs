namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;

    internal abstract class ControlBody : ControlBase
    {
        internal bool? autosize = null;

        protected ControlBody()
        {
        }
    }
}

