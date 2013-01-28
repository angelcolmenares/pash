namespace System.Management.Automation.Runspaces
{
    using System;

    public sealed class SessionStateAssemblyEntry : InitialSessionStateEntry
    {
        private string _fileName;

        public SessionStateAssemblyEntry(string name) : base(name)
        {
        }

        public SessionStateAssemblyEntry(string name, string fileName) : base(name)
        {
            this._fileName = fileName;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateAssemblyEntry entry = new SessionStateAssemblyEntry(base.Name, this._fileName);
            entry.SetPSSnapIn(base.PSSnapIn);
            entry.SetModule(base.Module);
            return entry;
        }

        public string FileName
        {
            get
            {
                return this._fileName;
            }
        }
    }
}

