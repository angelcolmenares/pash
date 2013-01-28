namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Management.Automation;

    internal sealed class TerminatingErrorContext
    {
        private PSCmdlet _command;

        internal TerminatingErrorContext(PSCmdlet command)
        {
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            this._command = command;
        }

        internal void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            this._command.ThrowTerminatingError(errorRecord);
        }
    }
}

