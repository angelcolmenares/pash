namespace System.Management.Automation
{
    using System;

    internal class HelpProviderInfo
    {
        internal string AssemblyName = "";
        internal string ClassName = "";
        internal System.Management.Automation.HelpCategory HelpCategory;

        internal HelpProviderInfo(string assemblyName, string className, System.Management.Automation.HelpCategory helpCategory)
        {
            this.AssemblyName = assemblyName;
            this.ClassName = className;
            this.HelpCategory = helpCategory;
        }
    }
}

