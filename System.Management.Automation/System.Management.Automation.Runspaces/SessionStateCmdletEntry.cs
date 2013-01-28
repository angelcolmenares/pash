namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateCmdletEntry : SessionStateCommandEntry
    {
        private string _helpFileName;
        private Type _implementingType;

        public SessionStateCmdletEntry(string name, Type implementingType, string helpFileName) : base(name, SessionStateEntryVisibility.Public)
        {
            this._implementingType = implementingType;
            this._helpFileName = helpFileName;
            base._commandType = CommandTypes.Cmdlet;
        }

        internal SessionStateCmdletEntry(string name, Type implementingType, string helpFileName, SessionStateEntryVisibility visibility) : base(name, visibility)
        {
            this._implementingType = implementingType;
            this._helpFileName = helpFileName;
            base._commandType = CommandTypes.Cmdlet;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateCmdletEntry entry = new SessionStateCmdletEntry(base.Name, this._implementingType, this._helpFileName, base.Visibility);
            entry.SetPSSnapIn(base.PSSnapIn);
            entry.SetModule(base.Module);
            return entry;
        }

        public string HelpFileName
        {
            get
            {
                return this._helpFileName;
            }
        }

        public Type ImplementingType
        {
            get
            {
                return this._implementingType;
            }
        }
    }
}

