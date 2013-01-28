namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;

    public sealed class PSCommand
    {
        private CommandCollection commands;
        private Command currentCommand;
        private PowerShell owner;
        private static string resBaseName = "PSCommandStrings";

        public PSCommand()
        {
            this.Initialize(null, false, null);
        }

        internal PSCommand(PSCommand commandToClone)
        {
            this.commands = new CommandCollection();
            foreach (Command command in commandToClone.Commands)
            {
                Command item = command.Clone();
                this.commands.Add(item);
                this.currentCommand = item;
            }
        }

        internal PSCommand(Command command)
        {
            this.currentCommand = command;
            this.commands = new CommandCollection();
            this.commands.Add(this.currentCommand);
        }

        public PSCommand AddArgument(object value)
        {
            if (this.currentCommand == null)
            {
                throw PSTraceSource.NewInvalidOperationException(resBaseName, "ParameterRequiresCommand", new object[] { "PSCommand" });
            }
            if (this.owner != null)
            {
                this.owner.AssertChangesAreAccepted();
            }
            this.currentCommand.Parameters.Add(null, value);
            return this;
        }

        public PSCommand AddCommand(Command command)
        {
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("command");
            }
            if (this.owner != null)
            {
                this.owner.AssertChangesAreAccepted();
            }
            this.currentCommand = command;
            this.commands.Add(this.currentCommand);
            return this;
        }

        public PSCommand AddCommand(string command)
        {
            if (command == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            if (this.owner != null)
            {
                this.owner.AssertChangesAreAccepted();
            }
            this.currentCommand = new Command(command, false);
            this.commands.Add(this.currentCommand);
            return this;
        }

        public PSCommand AddCommand(string cmdlet, bool useLocalScope)
        {
            if (cmdlet == null)
            {
                throw PSTraceSource.NewArgumentNullException("cmdlet");
            }
            if (this.owner != null)
            {
                this.owner.AssertChangesAreAccepted();
            }
            this.currentCommand = new Command(cmdlet, false, useLocalScope);
            this.commands.Add(this.currentCommand);
            return this;
        }

        public PSCommand AddParameter(string parameterName)
        {
            if (this.currentCommand == null)
            {
                throw PSTraceSource.NewInvalidOperationException(resBaseName, "ParameterRequiresCommand", new object[] { "PSCommand" });
            }
            if (this.owner != null)
            {
                this.owner.AssertChangesAreAccepted();
            }
            this.currentCommand.Parameters.Add(parameterName, true);
            return this;
        }

        public PSCommand AddParameter(string parameterName, object value)
        {
            if (this.currentCommand == null)
            {
                throw PSTraceSource.NewInvalidOperationException(resBaseName, "ParameterRequiresCommand", new object[] { "PSCommand" });
            }
            if (this.owner != null)
            {
                this.owner.AssertChangesAreAccepted();
            }
            this.currentCommand.Parameters.Add(parameterName, value);
            return this;
        }

        public PSCommand AddScript(string script)
        {
            if (script == null)
            {
                throw PSTraceSource.NewArgumentNullException("script");
            }
            if (this.owner != null)
            {
                this.owner.AssertChangesAreAccepted();
            }
            this.currentCommand = new Command(script, true);
            this.commands.Add(this.currentCommand);
            return this;
        }

        public PSCommand AddScript(string script, bool useLocalScope)
        {
            if (script == null)
            {
                throw PSTraceSource.NewArgumentNullException("script");
            }
            if (this.owner != null)
            {
                this.owner.AssertChangesAreAccepted();
            }
            this.currentCommand = new Command(script, true, useLocalScope);
            this.commands.Add(this.currentCommand);
            return this;
        }

        public PSCommand AddStatement()
        {
            if (this.commands.Count != 0)
            {
                this.commands[this.commands.Count - 1].IsEndOfStatement = true;
            }
            return this;
        }

        public void Clear()
        {
            this.commands.Clear();
            this.currentCommand = null;
        }

        public PSCommand Clone()
        {
            return new PSCommand(this);
        }

        private void Initialize(string command, bool isScript, bool? useLocalScope)
        {
            this.commands = new CommandCollection();
            if (command != null)
            {
                this.currentCommand = new Command(command, isScript, useLocalScope);
                this.commands.Add(this.currentCommand);
            }
        }

        public CommandCollection Commands
        {
            get
            {
                return this.commands;
            }
        }

        internal PowerShell Owner
        {
            get
            {
                return this.owner;
            }
            set
            {
                this.owner = value;
            }
        }
    }
}

