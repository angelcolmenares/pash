namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public abstract class SessionStateCommandEntry : ConstrainedSessionStateEntry
    {
        internal CommandTypes _commandType;
        internal bool _isImported;

        protected SessionStateCommandEntry(string name) : base(name, SessionStateEntryVisibility.Public)
        {
            this._isImported = true;
        }

        protected internal SessionStateCommandEntry(string name, SessionStateEntryVisibility visibility) : base(name, visibility)
        {
            this._isImported = true;
        }

        public CommandTypes CommandType
        {
            get
            {
                return this._commandType;
            }
        }
    }
}

