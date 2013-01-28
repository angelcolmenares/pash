namespace System.Management.Automation
{
    using System;

    public interface IModuleAssemblyInitializer
    {
        void OnImport();
    }
}

