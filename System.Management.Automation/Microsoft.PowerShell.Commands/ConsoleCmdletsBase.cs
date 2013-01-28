namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    public abstract class ConsoleCmdletsBase : PSCmdlet
    {
        internal const string resBaseName = "ConsoleInfoErrorStrings";

        protected ConsoleCmdletsBase()
        {
        }

        internal void ThrowError(object targetObject, string errorId, Exception innerException, ErrorCategory category)
        {
            base.ThrowTerminatingError(new ErrorRecord(innerException, errorId, category, targetObject));
        }

        internal System.Management.Automation.Runspaces.InitialSessionState InitialSessionState
        {
            get
            {
                return base.Context.InitialSessionState;
            }
        }

        internal RunspaceConfigForSingleShell Runspace
        {
            get
            {
                return (base.Context.RunspaceConfiguration as RunspaceConfigForSingleShell);
            }
        }
    }
}

