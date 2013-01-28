namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Runspaces;

    internal abstract class ExecutionCmdletHelper : IThrottleOperation
    {
        protected Exception internalException;
        protected System.Management.Automation.Runspaces.Pipeline pipeline;

        protected ExecutionCmdletHelper()
        {
        }

        internal Exception InternalException
        {
            get
            {
                return this.internalException;
            }
        }

        internal System.Management.Automation.Runspaces.Pipeline Pipeline
        {
            get
            {
                return this.pipeline;
            }
        }
    }
}

