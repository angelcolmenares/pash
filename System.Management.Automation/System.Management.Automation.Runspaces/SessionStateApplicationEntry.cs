namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateApplicationEntry : SessionStateCommandEntry
    {
        private string _path;

        public SessionStateApplicationEntry(string path) : base(path, SessionStateEntryVisibility.Public)
        {
            this._path = path;
            base._commandType = CommandTypes.Application;
        }

        internal SessionStateApplicationEntry(string path, SessionStateEntryVisibility visibility) : base(path, visibility)
        {
            this._path = path;
            base._commandType = CommandTypes.Application;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateApplicationEntry entry = new SessionStateApplicationEntry(this._path, base.Visibility);
            entry.SetModule(base.Module);
            return entry;
        }

        public string Path
        {
            get
            {
                return this._path;
            }
        }
    }
}

