namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections;

    internal class InnerFormatShapeCommandBase : ImplementationCommandBase
    {
        protected Stack contextManager = new Stack();

        internal InnerFormatShapeCommandBase()
        {
            this.contextManager.Push(new FormattingContext(FormattingContext.State.none));
        }

        protected class FormattingContext
        {
            internal State state;

            internal FormattingContext(State s)
            {
                this.state = s;
            }

            internal enum State
            {
                none,
                document,
                group
            }
        }
    }
}

