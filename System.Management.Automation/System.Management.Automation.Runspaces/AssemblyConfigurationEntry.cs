namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class AssemblyConfigurationEntry : RunspaceConfigurationEntry
    {
        private string _fileName;

        public AssemblyConfigurationEntry(string name, string fileName) : base(name)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileName.Trim()))
            {
                throw PSTraceSource.NewArgumentNullException("fileName");
            }
            this._fileName = fileName.Trim();
        }

        internal AssemblyConfigurationEntry(string name, string fileName, PSSnapInInfo psSnapinInfo) : base(name, psSnapinInfo)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileName.Trim()))
            {
                throw PSTraceSource.NewArgumentNullException("fileName");
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
    }
}

