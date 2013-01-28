namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class TypeConfigurationEntry : RunspaceConfigurationEntry
    {
        private string _fileName;
        private bool _isRemove;
        private System.Management.Automation.Runspaces.TypeData _typeData;

        public TypeConfigurationEntry(string fileName) : base(fileName)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileName.Trim()))
            {
                throw PSTraceSource.NewArgumentException("fileName");
            }
            this._fileName = fileName.Trim();
        }

        public TypeConfigurationEntry(System.Management.Automation.Runspaces.TypeData typeData, bool isRemove) : base("*")
        {
            if (typeData == null)
            {
                throw PSTraceSource.NewArgumentException("typeData");
            }
            this._typeData = typeData;
            this._isRemove = isRemove;
        }

        public TypeConfigurationEntry(string name, string fileName) : base(name)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileName.Trim()))
            {
                throw PSTraceSource.NewArgumentException("fileName");
            }
            this._fileName = fileName.Trim();
        }

        internal TypeConfigurationEntry(string name, string fileName, PSSnapInInfo psSnapinInfo) : base(name, psSnapinInfo)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileName.Trim()))
            {
                throw PSTraceSource.NewArgumentException("fileName");
            }
            this._fileName = fileName.Trim();
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
    }
}

