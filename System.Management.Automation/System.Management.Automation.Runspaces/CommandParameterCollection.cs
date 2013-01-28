namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;

    public sealed class CommandParameterCollection : Collection<CommandParameter>
    {
        public void Add(string name)
        {
            base.Add(new CommandParameter(name));
        }

        public void Add(string name, object value)
        {
            base.Add(new CommandParameter(name, value));
        }
    }
}

