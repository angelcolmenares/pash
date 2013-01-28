namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Runspaces.Internal;
    using System.Runtime.InteropServices;
    using System.Threading;

    public sealed class RunspacePool : IDisposable
    {
        private RunspacePoolInternal internalPool;
        private bool isRemote;
        private object syncObject;

        internal event EventHandler<PSEventArgs> ForwardEvent
        {
            add
            {
                lock (this.syncObject)
                {
                    bool flag = this.InternalForwardEvent == null;
                    this.InternalForwardEvent += value;
                    if (flag)
                    {
                        this.internalPool.ForwardEvent += new EventHandler<PSEventArgs>(this.OnInternalPoolForwardEvent);
                    }
                }
            }
            remove
            {
                lock (this.syncObject)
                {
                    this.InternalForwardEvent -= value;
                    if (this.InternalForwardEvent == null)
                    {
                        this.internalPool.ForwardEvent -= new EventHandler<PSEventArgs>(this.OnInternalPoolForwardEvent);
                    }
                }
            }
        }

        private event EventHandler<PSEventArgs> InternalForwardEvent;

        private event EventHandler<RunspaceCreatedEventArgs> InternalRunspaceCreated;

        private event EventHandler<RunspacePoolStateChangedEventArgs> InternalStateChanged;

        internal event EventHandler<RunspaceCreatedEventArgs> RunspaceCreated
        {
            add
            {
                lock (this.syncObject)
                {
                    bool flag = null == this.InternalRunspaceCreated;
                    this.InternalRunspaceCreated += value;
                    if (flag)
                    {
                        this.internalPool.RunspaceCreated += new EventHandler<RunspaceCreatedEventArgs>(this.OnRunspaceCreated);
                    }
                }
            }
            remove
            {
                lock (this.syncObject)
                {
                    this.InternalRunspaceCreated -= value;
                    if (this.InternalRunspaceCreated == null)
                    {
                        this.internalPool.RunspaceCreated -= new EventHandler<RunspaceCreatedEventArgs>(this.OnRunspaceCreated);
                    }
                }
            }
        }

        public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged
        {
            add
            {
                lock (this.syncObject)
                {
                    bool flag = null == this.InternalStateChanged;
                    this.InternalStateChanged += value;
                    if (flag)
                    {
                        this.internalPool.StateChanged += new EventHandler<RunspacePoolStateChangedEventArgs>(this.OnStateChanged);
                    }
                }
            }
            remove
            {
                lock (this.syncObject)
                {
                    this.InternalStateChanged -= value;
                    if (this.InternalStateChanged == null)
                    {
                        this.internalPool.StateChanged -= new EventHandler<RunspacePoolStateChangedEventArgs>(this.OnStateChanged);
                    }
                }
            }
        }

        internal RunspacePool(int minRunspaces, int maxRunspaces, System.Management.Automation.Runspaces.InitialSessionState initialSessionState, PSHost host)
        {
            this.syncObject = new object();
            this.internalPool = new RunspacePoolInternal(minRunspaces, maxRunspaces, initialSessionState, host);
        }

        internal RunspacePool(int minRunspaces, int maxRunspaces, RunspaceConfiguration runspaceConfiguration, PSHost host)
        {
            this.syncObject = new object();
            this.internalPool = new RunspacePoolInternal(minRunspaces, maxRunspaces, runspaceConfiguration, host);
        }

        internal RunspacePool(bool isDisconnected, Guid instanceId, string name, ConnectCommandInfo[] connectCommands, RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
        {
            this.syncObject = new object();
            if (!(connectionInfo is WSManConnectionInfo))
            {
                throw new NotSupportedException();
            }
            this.internalPool = new System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal(instanceId, name, isDisconnected, connectCommands, connectionInfo, host, typeTable);
            this.isRemote = true;
        }

        internal RunspacePool(int minRunspaces, int maxRunspaces, TypeTable typeTable, PSHost host, PSPrimitiveDictionary applicationArguments, RunspaceConnectionInfo connectionInfo, string name = null)
        {
            this.syncObject = new object();
            if (!(connectionInfo is WSManConnectionInfo) && !(connectionInfo is NewProcessConnectionInfo))
            {
                throw new NotSupportedException();
            }
            this.internalPool = new System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal(minRunspaces, maxRunspaces, typeTable, host, applicationArguments, connectionInfo, name);
            this.isRemote = true;
        }

        internal void AssertPoolIsOpen()
        {
            this.internalPool.AssertPoolIsOpen();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.internalPool.BeginClose(callback, state);
        }

        public IAsyncResult BeginConnect(AsyncCallback callback, object state)
        {
            return this.internalPool.BeginConnect(callback, state);
        }

        public IAsyncResult BeginDisconnect(AsyncCallback callback, object state)
        {
            return this.internalPool.BeginDisconnect(callback, state);
        }

        internal IAsyncResult BeginGetRunspace(AsyncCallback callback, object state)
        {
            return this.internalPool.BeginGetRunspace(callback, state);
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.internalPool.BeginOpen(callback, state);
        }

        internal void CancelGetRunspace(IAsyncResult asyncResult)
        {
            this.internalPool.CancelGetRunspace(asyncResult);
        }

        public void Close()
        {
            this.internalPool.Close();
        }

        public void Connect()
        {
            this.internalPool.Connect();
        }

        public Collection<PowerShell> CreateDisconnectedPowerShells()
        {
            return this.internalPool.CreateDisconnectedPowerShells(this);
        }

        public void Disconnect()
        {
            this.internalPool.Disconnect();
        }

        public void Dispose()
        {
            this.internalPool.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void EndClose(IAsyncResult asyncResult)
        {
            this.internalPool.EndClose(asyncResult);
        }

        public void EndConnect(IAsyncResult asyncResult)
        {
            this.internalPool.EndConnect(asyncResult);
        }

        public void EndDisconnect(IAsyncResult asyncResult)
        {
            this.internalPool.EndDisconnect(asyncResult);
        }

        internal Runspace EndGetRunspace(IAsyncResult asyncResult)
        {
            return this.internalPool.EndGetRunspace(asyncResult);
        }

        public void EndOpen(IAsyncResult asyncResult)
        {
            this.internalPool.EndOpen(asyncResult);
        }

        public PSPrimitiveDictionary GetApplicationPrivateData()
        {
            return this.internalPool.GetApplicationPrivateData();
        }

        public int GetAvailableRunspaces()
        {
            return this.internalPool.GetAvailableRunspaces();
        }

        public RunspacePoolCapability GetCapabilities()
        {
            return this.internalPool.GetCapabilities();
        }

        public int GetMaxRunspaces()
        {
            return this.internalPool.GetMaxRunspaces();
        }

        public int GetMinRunspaces()
        {
            return this.internalPool.GetMinRunspaces();
        }

        public static RunspacePool[] GetRunspacePools(RunspaceConnectionInfo connectionInfo)
        {
            return GetRunspacePools(connectionInfo, null, null);
        }

        public static RunspacePool[] GetRunspacePools(RunspaceConnectionInfo connectionInfo, PSHost host)
        {
            return GetRunspacePools(connectionInfo, host, null);
        }

        public static RunspacePool[] GetRunspacePools(RunspaceConnectionInfo connectionInfo, PSHost host, TypeTable typeTable)
        {
            return System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal.GetRemoteRunspacePools(connectionInfo, host, typeTable);
        }

        private void OnEventForwarded(PSEventArgs e)
        {
            EventHandler<PSEventArgs> internalForwardEvent = this.InternalForwardEvent;
            if (internalForwardEvent != null)
            {
                internalForwardEvent(this, e);
            }
        }

        private void OnInternalPoolForwardEvent(object sender, PSEventArgs e)
        {
            this.OnEventForwarded(e);
        }

        private void OnRunspaceCreated(object source, RunspaceCreatedEventArgs args)
        {
            this.InternalRunspaceCreated.SafeInvoke<RunspaceCreatedEventArgs>(this, args);
        }

        private void OnStateChanged(object source, RunspacePoolStateChangedEventArgs args)
        {
            if (this.ConnectionInfo is NewProcessConnectionInfo)
            {
                NewProcessConnectionInfo connectionInfo = this.ConnectionInfo as NewProcessConnectionInfo;
                if ((connectionInfo.Process != null) && ((args.RunspacePoolStateInfo.State == RunspacePoolState.Opened) || (args.RunspacePoolStateInfo.State == RunspacePoolState.Broken)))
                {
                    connectionInfo.Process.RunspacePool = this;
                }
            }
            this.InternalStateChanged.SafeInvoke<RunspacePoolStateChangedEventArgs>(this, args);
        }

        public void Open()
        {
            this.internalPool.Open();
        }

        internal void ReleaseRunspace(Runspace runspace)
        {
            this.internalPool.ReleaseRunspace(runspace);
        }

        public bool SetMaxRunspaces(int maxRunspaces)
        {
            return this.internalPool.SetMaxRunspaces(maxRunspaces);
        }

        public bool SetMinRunspaces(int minRunspaces)
        {
            return this.internalPool.SetMinRunspaces(minRunspaces);
        }

        public System.Threading.ApartmentState ApartmentState
        {
            get
            {
                return this.internalPool.ApartmentState;
            }
            set
            {
                if (this.RunspacePoolStateInfo.State != RunspacePoolState.BeforeOpen)
                {
                    throw new InvalidRunspacePoolStateException(RunspacePoolStrings.ChangePropertyAfterOpen);
                }
                this.internalPool.ApartmentState = value;
            }
        }

        public TimeSpan CleanupInterval
        {
            get
            {
                return this.internalPool.CleanupInterval;
            }
            set
            {
                this.internalPool.CleanupInterval = value;
            }
        }

        public RunspaceConnectionInfo ConnectionInfo
        {
            get
            {
                return this.internalPool.ConnectionInfo;
            }
        }

        public System.Management.Automation.Runspaces.InitialSessionState InitialSessionState
        {
            get
            {
                return this.internalPool.InitialSessionState;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this.internalPool.InstanceId;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return this.internalPool.IsDisposed;
            }
        }

        internal bool IsRemote
        {
            get
            {
                return this.isRemote;
            }
        }

        internal System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal RemoteRunspacePoolInternal
        {
            get
            {
                if (this.internalPool is System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal)
                {
                    return (System.Management.Automation.Runspaces.Internal.RemoteRunspacePoolInternal) this.internalPool;
                }
                return null;
            }
        }

        public System.Management.Automation.Runspaces.RunspacePoolAvailability RunspacePoolAvailability
        {
            get
            {
                return this.internalPool.RunspacePoolAvailability;
            }
        }

        public System.Management.Automation.RunspacePoolStateInfo RunspacePoolStateInfo
        {
            get
            {
                return this.internalPool.RunspacePoolStateInfo;
            }
        }

        public PSThreadOptions ThreadOptions
        {
            get
            {
                return this.internalPool.ThreadOptions;
            }
            set
            {
                if (this.RunspacePoolStateInfo.State != RunspacePoolState.BeforeOpen)
                {
                    throw new InvalidRunspacePoolStateException(RunspacePoolStrings.ChangePropertyAfterOpen);
                }
                this.internalPool.ThreadOptions = value;
            }
        }
    }
}

