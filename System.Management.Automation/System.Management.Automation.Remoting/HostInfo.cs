namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal.Host;

    internal class HostInfo
    {
        private System.Management.Automation.Remoting.HostDefaultData _hostDefaultData;
        private bool _isHostNull;
        private bool _isHostRawUINull;
        private bool _isHostUINull;
        private bool _useRunspaceHost;

        internal HostInfo(PSHost host)
        {
            CheckHostChain(host, ref this._isHostNull, ref this._isHostUINull, ref this._isHostRawUINull);
            if (!this._isHostUINull && !this._isHostRawUINull)
            {
                this._hostDefaultData = System.Management.Automation.Remoting.HostDefaultData.Create(host.UI.RawUI);
            }
        }

        private static void CheckHostChain(PSHost host, ref bool isHostNull, ref bool isHostUINull, ref bool isHostRawUINull)
        {
            isHostNull = true;
            isHostUINull = true;
            isHostRawUINull = true;
            if (host != null)
            {
                if (host is InternalHost)
                {
                    host = ((InternalHost) host).ExternalHost;
                }
                isHostNull = false;
                if (host.UI != null)
                {
                    isHostUINull = false;
                    if (host.UI.RawUI != null)
                    {
                        isHostRawUINull = false;
                    }
                }
            }
        }

        internal System.Management.Automation.Remoting.HostDefaultData HostDefaultData
        {
            get
            {
                return this._hostDefaultData;
            }
        }

        internal bool IsHostNull
        {
            get
            {
                return this._isHostNull;
            }
        }

        internal bool IsHostRawUINull
        {
            get
            {
                return this._isHostRawUINull;
            }
        }

        internal bool IsHostUINull
        {
            get
            {
                return this._isHostUINull;
            }
        }

        internal bool UseRunspaceHost
        {
            get
            {
                return this._useRunspaceHost;
            }
            set
            {
                this._useRunspaceHost = value;
            }
        }
    }
}

