namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;

    public sealed class CommandCollection : Collection<Command>
    {
        internal CommandCollection()
        {
        }

        public void Add(string command)
        {
            if (string.Equals(command, "out-default", StringComparison.OrdinalIgnoreCase))
            {
                this.Add(command, true);
            }
            else
            {
                base.Add(new Command(command));
            }
        }

        internal void Add(string command, bool mergeUnclaimedPreviousCommandError)
        {
            base.Add(new Command(command, false, false, mergeUnclaimedPreviousCommandError));
        }

        public void AddScript(string scriptContents)
        {
            base.Add(new Command(scriptContents, true));
        }

        public void AddScript(string scriptContents, bool useLocalScope)
        {
            base.Add(new Command(scriptContents, true, useLocalScope));
        }

        internal string GetCommandStringForHistory()
        {
            Command command = base[0];
            return command.CommandText;
        }
    }
}

