namespace System.Management.Automation.Host
{
    using System;
    using System.Management.Automation.Runspaces;

    public interface IHostSupportsInteractiveSession
    {
        void PopRunspace();
        void PushRunspace(System.Management.Automation.Runspaces.Runspace runspace);

        bool IsRunspacePushed { get; }

        System.Management.Automation.Runspaces.Runspace Runspace { get; }
    }
}

