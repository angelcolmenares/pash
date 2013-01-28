namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class ScriptConfigurationEntry : RunspaceConfigurationEntry
    {
        private string _definition;

        public ScriptConfigurationEntry(string name, string definition) : base(name)
        {
            if (string.IsNullOrEmpty(definition) || string.IsNullOrEmpty(definition.Trim()))
            {
                throw PSTraceSource.NewArgumentNullException("definition");
            }
            this._definition = definition.Trim();
        }

        public string Definition
        {
            get
            {
                return this._definition;
            }
        }
    }
}

