namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;

    internal class StopUpstreamCommandsException : FlowControlException
    {
        public StopUpstreamCommandsException(InternalCommand requestingCommand)
        {
            this.RequestingCommandProcessor = requestingCommand.Context.CurrentCommandProcessor;
        }

        public CommandProcessorBase RequestingCommandProcessor { get; private set; }
    }
}

