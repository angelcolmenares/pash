namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class FormatConfigurationEntry : RunspaceConfigurationEntry
    {
        private string _fileName;
        private ExtendedTypeDefinition _typeDefinition;

        public FormatConfigurationEntry(ExtendedTypeDefinition typeDefinition) : base("*")
        {
            if (typeDefinition == null)
            {
                throw PSTraceSource.NewArgumentNullException("typeDefinition");
            }
            this._typeDefinition = typeDefinition;
        }

        public FormatConfigurationEntry(string fileName) : base(fileName)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileName.Trim()))
            {
                throw PSTraceSource.NewArgumentException("fileName");
            }
            this._fileName = fileName.Trim();
        }

        public FormatConfigurationEntry(string name, string fileName) : base(name)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileName.Trim()))
            {
                throw PSTraceSource.NewArgumentException("fileName");
            }
            this._fileName = fileName.Trim();
        }

        internal FormatConfigurationEntry(string name, string fileName, PSSnapInInfo psSnapinInfo) : base(name, psSnapinInfo)
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

        public ExtendedTypeDefinition FormatData
        {
            get
            {
                return this._typeDefinition;
            }
        }
    }
}

