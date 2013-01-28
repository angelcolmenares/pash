namespace System.Management.Automation.Runspaces
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class RunspaceConfigurationTypeAttribute : Attribute
    {
        private string _runspaceConfigType;

        public RunspaceConfigurationTypeAttribute(string runspaceConfigurationType)
        {
            this._runspaceConfigType = runspaceConfigurationType;
        }

        public string RunspaceConfigurationType
        {
            get
            {
                return this._runspaceConfigType;
            }
        }
    }
}

