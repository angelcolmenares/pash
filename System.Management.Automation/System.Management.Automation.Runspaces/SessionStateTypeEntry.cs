namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class SessionStateTypeEntry : InitialSessionStateEntry
    {
        private string _fileName;
        private bool _isRemove;
        private System.Management.Automation.Runspaces.TypeData _typeData;
        private System.Management.Automation.Runspaces.TypeTable _typeTable;

        public SessionStateTypeEntry(System.Management.Automation.Runspaces.TypeTable typeTable) : base("*")
        {
            if (typeTable == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeTable");
            }
            this._typeTable = typeTable;
        }

        public SessionStateTypeEntry(string fileName) : base(fileName)
        {
            if (string.IsNullOrEmpty(fileName) || (fileName.Trim().Length == 0))
            {
                throw PSTraceSource.NewArgumentException("fileName");
            }
            this._fileName = fileName.Trim();
        }

        public SessionStateTypeEntry(System.Management.Automation.Runspaces.TypeData typeData, bool isRemove) : base("*")
        {
            if (typeData == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeData");
            }
            this._typeData = typeData;
            this._isRemove = isRemove;
        }

        public override InitialSessionStateEntry Clone()
        {
            SessionStateTypeEntry entry;
            if (this._fileName != null)
            {
                entry = new SessionStateTypeEntry(this._fileName);
            }
            else if (this._typeTable != null)
            {
                entry = new SessionStateTypeEntry(this._typeTable);
            }
            else
            {
                entry = new SessionStateTypeEntry(this._typeData, this._isRemove);
            }
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

        public bool IsRemove
        {
            get
            {
                return this._isRemove;
            }
        }

        public System.Management.Automation.Runspaces.TypeData TypeData
        {
            get
            {
                return this._typeData;
            }
        }

        public System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                return this._typeTable;
            }
        }
    }
}

