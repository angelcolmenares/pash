namespace System.Management.Automation
{
    using System;

    internal class CommandFactory
    {
        private ExecutionContext context;

        internal CommandFactory()
        {
        }

        internal CommandFactory(ExecutionContext context)
        {
            this.Context = context;
        }

        private CommandProcessorBase _CreateCommand(string commandName, CommandOrigin commandOrigin, bool? useLocalScope)
        {
            if (this.context == null)
            {
                throw PSTraceSource.NewInvalidOperationException("DiscoveryExceptions", "ExecutionContextNotSet", new object[0]);
            }
            CommandDiscovery commandDiscovery = this.context.CommandDiscovery;
            if (commandDiscovery == null)
            {
                throw PSTraceSource.NewInvalidOperationException("DiscoveryExceptions", "CommandDiscoveryMissing", new object[0]);
            }
            return commandDiscovery.LookupCommandProcessor(commandName, commandOrigin, useLocalScope);
        }

        internal CommandProcessorBase CreateCommand(string commandName, CommandOrigin commandOrigin)
        {
            return this._CreateCommand(commandName, commandOrigin, false);
        }

        internal CommandProcessorBase CreateCommand(string commandName, CommandOrigin commandOrigin, bool? useLocalScope)
        {
            return this._CreateCommand(commandName, commandOrigin, useLocalScope);
        }

        internal CommandProcessorBase CreateCommand(string commandName, ExecutionContext executionContext, CommandOrigin commandOrigin)
        {
            this.Context = executionContext;
            return this._CreateCommand(commandName, commandOrigin, false);
        }

        internal ExecutionContext Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
            }
        }
    }
}

