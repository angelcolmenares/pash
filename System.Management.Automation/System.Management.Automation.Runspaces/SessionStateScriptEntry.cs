namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateScriptEntry : SessionStateCommandEntry
    {
        private string _path;

        public SessionStateScriptEntry(string path) : base(path, SessionStateEntryVisibility.Public)
        {
            this._path = path;
            base._commandType = CommandTypes.ExternalScript;
        }

        internal SessionStateScriptEntry(string path, SessionStateEntryVisibility visibility) : base(path, visibility)
        {
            this._path = path;
            base._commandType = CommandTypes.ExternalScript;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateScriptEntry entry = new SessionStateScriptEntry(this._path, base.Visibility);
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

