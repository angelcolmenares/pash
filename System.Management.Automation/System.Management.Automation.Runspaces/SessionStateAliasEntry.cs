namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateAliasEntry : SessionStateCommandEntry
    {
        private string _definition;
        private string _description;
        private ScopedItemOptions _options;

        public SessionStateAliasEntry(string name, string definition) : base(name, SessionStateEntryVisibility.Public)
        {
            this._description = string.Empty;
            this._definition = definition;
            base._commandType = CommandTypes.Alias;
        }

        public SessionStateAliasEntry(string name, string definition, string description) : base(name, SessionStateEntryVisibility.Public)
        {
            this._description = string.Empty;
            this._definition = definition;
            base._commandType = CommandTypes.Alias;
            this._description = description;
        }

        public SessionStateAliasEntry(string name, string definition, string description, ScopedItemOptions options) : base(name, SessionStateEntryVisibility.Public)
        {
            this._description = string.Empty;
            this._definition = definition;
            base._commandType = CommandTypes.Alias;
            this._description = description;
            this._options = options;
        }

        internal SessionStateAliasEntry(string name, string definition, string description, ScopedItemOptions options, SessionStateEntryVisibility visibility) : base(name, visibility)
        {
            this._description = string.Empty;
            this._definition = definition;
            base._commandType = CommandTypes.Alias;
            this._description = description;
            this._options = options;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateAliasEntry entry = new SessionStateAliasEntry(base.Name, this._definition, this._description, this._options, base.Visibility);
            entry.SetModule(base.Module);
            return entry;
        }

        public string Definition
        {
            get
            {
                return this._definition;
            }
        }

        public string Description
        {
            get
            {
                return this._description;
            }
        }

        public ScopedItemOptions Options
        {
            get
            {
                return this._options;
            }
        }
    }
}

