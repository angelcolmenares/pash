namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateWorkflowEntry : SessionStateCommandEntry
    {
        private string _definition;
        private string _helpFile;
        private ScopedItemOptions _options;
        private System.Management.Automation.WorkflowInfo _workflow;

        public SessionStateWorkflowEntry(string name, string definition) : this(name, definition, ScopedItemOptions.None, null)
        {
        }

        public SessionStateWorkflowEntry(string name, string definition, string helpFile) : this(name, definition, ScopedItemOptions.None, helpFile)
        {
        }

        public SessionStateWorkflowEntry(string name, string definition, ScopedItemOptions options, string helpFile) : base(name, SessionStateEntryVisibility.Public)
        {
            this._definition = definition;
            base._commandType = CommandTypes.Workflow;
            this._options = options;
            this._helpFile = helpFile;
        }

        internal SessionStateWorkflowEntry(string name, string definition, ScopedItemOptions options, SessionStateEntryVisibility visibility, System.Management.Automation.WorkflowInfo workflow, string helpFile) : base(name, visibility)
        {
            this._definition = definition;
            this._options = options;
            this._workflow = workflow;
            this._helpFile = helpFile;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateWorkflowEntry entry = new SessionStateWorkflowEntry(base.Name, this._definition, this._options, base.Visibility, this._workflow, this._helpFile);
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

        internal System.Management.Automation.WorkflowInfo WorkflowInfo
        {
            get
            {
                return this._workflow;
            }
            set
            {
                this._workflow = value;
            }
        }
    }
}

