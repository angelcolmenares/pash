namespace System.Management.Automation.Provider
{
    using System;

    public interface ICmdletProviderSupportsHelp
    {
        string GetHelpMaml(string helpItemName, string path);
    }
}

