namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    public class CommandLookupEventArgs : EventArgs
    {
        private string commandName;
        private System.Management.Automation.CommandOrigin commandOrigin;
        private ExecutionContext context;
        private ScriptBlock scriptBlock;

        internal CommandLookupEventArgs(string commandName, System.Management.Automation.CommandOrigin commandOrigin, ExecutionContext context)
        {
            this.commandName = commandName;
            this.commandOrigin = commandOrigin;
            this.context = context;
        }

        public CommandInfo Command { get; set; }

        public string CommandName
        {
            get
            {
                return this.commandName;
            }
        }

        public System.Management.Automation.CommandOrigin CommandOrigin
        {
            get
            {
                return this.commandOrigin;
            }
        }

        public ScriptBlock CommandScriptBlock
        {
            get
            {
                return this.scriptBlock;
            }
            set
            {
                this.scriptBlock = value;
                if (this.scriptBlock != null)
                {
                    string name = "LookupHandlerReplacementFor<<" + this.commandName + ">>";
                    this.Command = new FunctionInfo(name, this.scriptBlock, this.context);
                    this.StopSearch = true;
                }
                else
                {
                    this.Command = null;
                    this.StopSearch = false;
                }
            }
        }

        public bool StopSearch { get; set; }
    }
}

