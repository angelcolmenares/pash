namespace Microsoft.PowerShell.Commands.Internal.Format
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Management.Automation;

    internal sealed class SubPipelineManager : IDisposable
    {
        private List<CommandEntry> commandEntryList = new List<CommandEntry>();
        private CommandEntry defaultCommandEntry = new CommandEntry();
        private LineOutput lo;

        public void Dispose()
        {
            foreach (CommandEntry entry in this.commandEntryList)
            {
                entry.Dispose();
            }
            this.defaultCommandEntry.Dispose();
        }

        private CommandEntry GetActiveCommandEntry(PSObject so)
        {
            string typeName = PSObjectHelper.PSObjectIsOfExactType(so.InternalTypeNames);
            foreach (CommandEntry entry in this.commandEntryList)
            {
                if (entry.AppliesToType(typeName))
                {
                    return entry;
                }
            }
            return this.defaultCommandEntry;
        }

        internal void Initialize(LineOutput lineOutput, ExecutionContext context)
        {
            this.lo = lineOutput;
            this.InitializeCommandsHardWired(context);
        }

        private void InitializeCommandsHardWired(ExecutionContext context)
        {
            this.RegisterCommandDefault(context, "out-lineoutput", typeof(OutLineOutputCommand));
        }

        internal void Process(PSObject so)
        {
            this.GetActiveCommandEntry(so).command.Process(so);
        }

        private void RegisterCommandDefault(ExecutionContext context, string commandName, Type commandType)
        {
            CommandEntry entry = new CommandEntry();
            entry.command.Initialize(context, commandName, commandType);
            entry.command.AddNamedParameter("LineOutput", this.lo);
            this.defaultCommandEntry = entry;
        }

        internal void ShutDown()
        {
            foreach (CommandEntry entry in this.commandEntryList)
            {
                entry.command.ShutDown();
                entry.command = null;
            }
            this.defaultCommandEntry.command.ShutDown();
            this.defaultCommandEntry.command = null;
        }

        private sealed class CommandEntry : IDisposable
        {
            private StringCollection applicableTypes = new StringCollection();
            internal CommandWrapper command = new CommandWrapper();

            internal bool AppliesToType(string typeName)
            {
                StringEnumerator enumerator = this.applicableTypes.GetEnumerator();
                {
                    while (enumerator.MoveNext())
                    {
                        if (string.Equals(enumerator.Current, typeName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public void Dispose()
            {
                if (this.command != null)
                {
                    this.command.Dispose();
                    this.command = null;
                }
            }
        }
    }
}

