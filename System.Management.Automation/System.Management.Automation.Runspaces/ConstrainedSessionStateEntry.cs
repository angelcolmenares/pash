namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public abstract class ConstrainedSessionStateEntry : InitialSessionStateEntry
    {
        private SessionStateEntryVisibility _visibility;

        protected ConstrainedSessionStateEntry(string name, SessionStateEntryVisibility visibility) : base(name)
        {
            this._visibility = visibility;
        }

        public SessionStateEntryVisibility Visibility
        {
            get
            {
                return this._visibility;
            }
            set
            {
                this._visibility = value;
            }
        }
    }
}

