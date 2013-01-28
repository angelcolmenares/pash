namespace System.Management.Automation.Runspaces
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Tracing;
    using System.Threading;

    public static class RunspaceFactory
    {
        static RunspaceFactory()
        {
            if (EtwActivity.GetActivityId() == Guid.Empty)
            {
                EtwActivity.SetActivityId(EtwActivity.CreateActivityId());
            }
        }

        public static Runspace CreateOutOfProcessRunspace(TypeTable typeTable)
        {
            NewProcessConnectionInfo connectionInfo = new NewProcessConnectionInfo(null);
            return CreateRunspace(connectionInfo, null, typeTable);
        }

        public static Runspace CreateOutOfProcessRunspace(TypeTable typeTable, PowerShellProcessInstance processInstance)
        {
            NewProcessConnectionInfo connectionInfo = new NewProcessConnectionInfo(null) {
                Process = processInstance
            };
            return CreateRunspace(connectionInfo, null, typeTable);
        }

        public static Runspace CreateRunspace()
        {
            PSHost host = new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture);
            return CreateRunspace(host);
        }

        public static Runspace CreateRunspace(PSHost host)
        {
            if (host == null)
            {
                throw PSTraceSource.NewArgumentNullException("host");
            }
            return CreateRunspace(host, RunspaceConfiguration.Create());
        }

        public static Runspace CreateRunspace(InitialSessionState initialSessionState)
        {
            if (initialSessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("initialSessionState");
            }
            PSHost host = new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture);
            return CreateRunspace(host, initialSessionState);
        }

        public static Runspace CreateRunspace(RunspaceConfiguration runspaceConfiguration)
        {
            if (runspaceConfiguration == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspaceConfiguration");
            }
            PSHost host = new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture);
            return CreateRunspace(host, runspaceConfiguration);
        }

        public static Runspace CreateRunspace(RunspaceConnectionInfo connectionInfo)
        {
            return CreateRunspace(null, connectionInfo);
        }

        public static Runspace CreateRunspace(PSHost host, InitialSessionState initialSessionState)
        {
            if (host == null)
            {
                throw PSTraceSource.NewArgumentNullException("host");
            }
            if (initialSessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("initialSessionState");
            }
            return new LocalRunspace(host, initialSessionState);
        }

        public static Runspace CreateRunspace(PSHost host, RunspaceConfiguration runspaceConfiguration)
        {
            if (host == null)
            {
                throw PSTraceSource.NewArgumentNullException("host");
            }
            if (runspaceConfiguration == null)
            {
                throw PSTraceSource.NewArgumentNullException("runspaceConfiguration");
            }
            return new LocalRunspace(host, runspaceConfiguration);
        }

        public static Runspace CreateRunspace(PSHost host, RunspaceConnectionInfo connectionInfo)
        {
            return CreateRunspace(connectionInfo, host, null);
        }

        public static Runspace CreateRunspace(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
        {
            return CreateRunspace(connectionInfo, host, typeTable, null);
        }

        public static Runspace CreateRunspace(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable, PSPrimitiveDictionary applicationArguments)
        {
            if (!(connectionInfo is WSManConnectionInfo) && !(connectionInfo is NewProcessConnectionInfo))
            {
                throw new NotSupportedException();
            }
            if (connectionInfo is WSManConnectionInfo)
            {
                RemotingCommandUtil.CheckHostRemotingPrerequisites();
            }
            return new RemoteRunspace(typeTable, connectionInfo, host, applicationArguments, null, -1);
        }

        internal static Runspace CreateRunspaceFromSessionStateNoClone(PSHost host, InitialSessionState initialSessionState)
        {
            if (host == null)
            {
                throw PSTraceSource.NewArgumentNullException("host");
            }
            if (initialSessionState == null)
            {
                throw PSTraceSource.NewArgumentNullException("initialSessionState");
            }
            return new LocalRunspace(host, initialSessionState, true);
        }

        public static RunspacePool CreateRunspacePool()
        {
            return CreateRunspacePool(1, 1, RunspaceConfiguration.Create(), new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
        }

        public static RunspacePool CreateRunspacePool(InitialSessionState initialSessionState)
        {
            return CreateRunspacePool(1, 1, initialSessionState, new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
        }

        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces)
        {
            return CreateRunspacePool(minRunspaces, maxRunspaces, RunspaceConfiguration.Create(), new DefaultHost(Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture));
        }

        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, PSHost host)
        {
            return CreateRunspacePool(minRunspaces, maxRunspaces, RunspaceConfiguration.Create(), host);
        }

        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, RunspaceConnectionInfo connectionInfo)
        {
            return CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, null);
        }

        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, InitialSessionState initialSessionState, PSHost host)
        {
            return new RunspacePool(minRunspaces, maxRunspaces, initialSessionState, host);
        }

        private static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, RunspaceConfiguration runspaceConfiguration, PSHost host)
        {
            return new RunspacePool(minRunspaces, maxRunspaces, runspaceConfiguration, host);
        }

        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, RunspaceConnectionInfo connectionInfo, PSHost host)
        {
            return CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, host, null);
        }

        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
        {
            return CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo, host, typeTable, null);
        }

        public static RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable, PSPrimitiveDictionary applicationArguments)
        {
            if (!(connectionInfo is WSManConnectionInfo) && !(connectionInfo is NewProcessConnectionInfo))
            {
                throw new NotSupportedException();
            }
            if (connectionInfo is WSManConnectionInfo)
            {
                RemotingCommandUtil.CheckHostRemotingPrerequisites();
            }
            return new RunspacePool(minRunspaces, maxRunspaces, typeTable, host, applicationArguments, connectionInfo, null);
        }
    }
}

