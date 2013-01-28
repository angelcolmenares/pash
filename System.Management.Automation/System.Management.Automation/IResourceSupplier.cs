namespace System.Management.Automation
{
    using System;

    public interface IResourceSupplier
    {
        string GetResourceString(string baseName, string resourceId);
    }
}

