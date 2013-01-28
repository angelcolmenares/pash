namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateFunctionEntry : SessionStateCommandEntry
    {
        private string _definition;
        private string _helpFile;
        private ScopedItemOptions _options;
        private System.Management.Automation.ScriptBlock _scriptBlock;

        public SessionStateFunctionEntry(string name, string definition) : this(name, definition, ScopedItemOptions.None, null)
        {
        }

        public SessionStateFunctionEntry(string name, string definition, string helpFile) : this(name, definition, ScopedItemOptions.None, helpFile)
        {
        }

        public SessionStateFunctionEntry(string name, string definition, ScopedItemOptions options, string helpFile) : base(name, SessionStateEntryVisibility.Public)
        {
            this._definition = definition;
            base._commandType = CommandTypes.Function;
            this._options = options;
            this._scriptBlock = System.Management.Automation.ScriptBlock.Create(this._definition);
            this._helpFile = helpFile;
        }

        internal SessionStateFunctionEntry(string name, string definition, ScopedItemOptions options, SessionStateEntryVisibility visibility, System.Management.Automation.ScriptBlock scriptBlock, string helpFile) : base(name, visibility)
        {
            this._definition = definition;
            this._options = options;
            this._scriptBlock = scriptBlock;
            this._helpFile = helpFile;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateFunctionEntry entry = new SessionStateFunctionEntry(base.Name, this._definition, this._options, base.Visibility, this._scriptBlock, this._helpFile);
            entry.SetModule(base.Module);
            return entry;
        }

        internal void SetHelpFile(string help)
        {
            this._helpFile = help;
        }

        public string Definition
        {
            get
            {
                return this._definition;
            }
        }

        public string HelpFile
        {
            get
            {
                return this._helpFile;
            }
        }

        public ScopedItemOptions Options
        {
            get
            {
                return this._options;
            }
        }

        internal System.Management.Automation.ScriptBlock ScriptBlock
        {
            get
            {
                return this._scriptBlock;
            }
            set
            {
                this._scriptBlock = value;
            }
        }
    }
}

