namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;

    internal sealed class CommandWrapper : IDisposable
    {
        private string commandName;
        private List<CommandParameterInternal> commandParameterList = new List<CommandParameterInternal>();
        private Type commandType;
        private ExecutionContext context;
        private PipelineProcessor pp;

        internal void AddNamedParameter(string parameterName, object parameterValue)
        {
            this.commandParameterList.Add(CommandParameterInternal.CreateParameterWithArgument(PositionUtilities.EmptyExtent, parameterName, null, PositionUtilities.EmptyExtent, parameterValue, false));
        }

        private void DelayedInternalInitialize()
        {
            this.pp = new PipelineProcessor();
            CmdletInfo cmdletInfo = new CmdletInfo(this.commandName, this.commandType, null, null, this.context);
            CommandProcessor commandProcessor = new CommandProcessor(cmdletInfo, this.context);
            foreach (CommandParameterInternal internal2 in this.commandParameterList)
            {
                commandProcessor.AddParameter(internal2);
            }
            this.pp.Add(commandProcessor);
        }

        public void Dispose()
        {
            if (this.pp != null)
            {
                this.pp.Dispose();
                this.pp = null;
            }
        }

        internal void Initialize(ExecutionContext execContext, string nameOfCommand, Type typeOfCommand)
        {
            this.context = execContext;
            this.commandName = nameOfCommand;
            this.commandType = typeOfCommand;
        }

        internal Array Process(object o)
        {
            if (this.pp == null)
            {
                this.DelayedInternalInitialize();
            }
            return this.pp.Step(o);
        }

        internal Array ShutDown()
        {
            if (this.pp == null)
            {
                return new object[0];
            }
            PipelineProcessor pp = this.pp;
            this.pp = null;
            return pp.Execute();
        }
    }
}

