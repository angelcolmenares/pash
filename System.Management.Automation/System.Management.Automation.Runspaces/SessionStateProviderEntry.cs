namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateProviderEntry : ConstrainedSessionStateEntry
    {
        private string _helpFileName;
        private Type _implementingType;

        public SessionStateProviderEntry(string name, Type implementingType, string helpFileName) : base(name, SessionStateEntryVisibility.Public)
        {
            this._implementingType = implementingType;
            this._helpFileName = helpFileName;
        }

        internal SessionStateProviderEntry(string name, Type implementingType, string helpFileName, SessionStateEntryVisibility visibility) : base(name, visibility)
        {
            this._implementingType = implementingType;
            this._helpFileName = helpFileName;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateProviderEntry entry = new SessionStateProviderEntry(base.Name, this._implementingType, this._helpFileName, base.Visibility);
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

