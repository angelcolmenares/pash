namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Text;

    public class WSManConfigurationOption : PSTransportOption
    {
        private int? _idleTimeoutSec = null;
        private int? _maxIdleTimeoutSec = null;
        private int? _processIdleTimeoutSec = null;
        internal const string AttribIdleTimeout = "IdleTimeoutms";
        private const string AttribMaxConcurrentCommandsPerSession = "MaxConcurrentCommandsPerShell";
        private const string AttribMaxConcurrentUsers = "MaxConcurrentUsers";
        internal const string AttribMaxIdleTimeout = "MaxIdleTimeoutms";
        private const string AttribMaxMemoryPerSessionMB = "MaxMemoryPerShellMB";
        private const string AttribMaxProcessesPerSession = "MaxProcessesPerShell";
        private const string AttribMaxSessions = "MaxShells";
        private const string AttribMaxSessionsPerUser = "MaxShellsPerUser";
        internal const string AttribOutputBufferingMode = "OutputBufferingMode";
        private const string AttribProcessIdleTimeout = "ProcessIdleTimeoutSec";
        internal static readonly int? DefaultIdleTimeout = 0x1c20;
        internal static readonly int? DefaultMaxConcurrentCommandsPerSession = 0x3e8;
        internal static readonly int? DefaultMaxConcurrentUsers = 5;
        internal static readonly int? DefaultMaxIdleTimeout = 0xa8c0;
        internal static readonly int? DefaultMaxMemoryPerSessionMB = 0x400;
        internal static readonly int? DefaultMaxProcessesPerSession = 15;
        internal static readonly int? DefaultMaxSessions = 0x19;
        internal static readonly int? DefaultMaxSessionsPerUser = 0x19;
        internal static System.Management.Automation.Runspaces.OutputBufferingMode? DefaultOutputBufferingMode = (System.Management.Automation.Runspaces.OutputBufferingMode)2;
        internal static readonly int? DefaultProcessIdleTimeout_ForPSRemoting = 0;
        internal static readonly int? DefaultProcessIdleTimeout_ForWorkflow = 300;
        private int? maxConcurrentCommandsPerSession = null;
        private int? maxConcurrentUsers = null;
        private int? maxMemoryPerSessionMB = null;
        private int? maxProcessesPerSession = null;
        private int? maxSessions = null;
        private int? maxSessionsPerUser = null;
        private System.Management.Automation.Runspaces.OutputBufferingMode? outputBufferingMode = null;
        private const string QuotasToken = "<Quotas {0} />";
        private const string Token = " {0}='{1}'";

        internal WSManConfigurationOption()
        {
        }

        internal override Hashtable ConstructOptionsAsHashtable()
        {
            Hashtable hashtable = new Hashtable();
            if (this.outputBufferingMode.HasValue)
            {
                hashtable["OutputBufferingMode"] = this.outputBufferingMode.ToString();
            }
            if (this._processIdleTimeoutSec.HasValue)
            {
                hashtable["ProcessIdleTimeoutSec"] = this._processIdleTimeoutSec;
            }
            return hashtable;
        }

        internal override string ConstructOptionsAsXmlAttributes()
        {
            StringBuilder builder = new StringBuilder();
            if (this.outputBufferingMode.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", new object[] { "OutputBufferingMode", this.outputBufferingMode.ToString() }));
            }
            if (this._processIdleTimeoutSec.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", new object[] { "ProcessIdleTimeoutSec", this._processIdleTimeoutSec }));
            }
            return builder.ToString();
        }

        internal override string ConstructQuotas()
        {
            StringBuilder builder = new StringBuilder();
            if (this._idleTimeoutSec.HasValue)
            {
                object[] args = new object[] { "IdleTimeoutms", 0x3e8 * this._idleTimeoutSec };
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", args));
            }
            if (this.maxConcurrentUsers.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", new object[] { "MaxConcurrentUsers", this.maxConcurrentUsers }));
            }
            if (this.maxProcessesPerSession.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", new object[] { "MaxProcessesPerShell", this.maxProcessesPerSession }));
            }
            if (this.maxMemoryPerSessionMB.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", new object[] { "MaxMemoryPerShellMB", this.maxMemoryPerSessionMB }));
            }
            if (this.maxSessionsPerUser.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", new object[] { "MaxShellsPerUser", this.maxSessionsPerUser }));
            }
            if (this.maxConcurrentCommandsPerSession.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", new object[] { "MaxConcurrentCommandsPerShell", this.maxConcurrentCommandsPerSession }));
            }
            if (this.maxSessions.HasValue)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", new object[] { "MaxShells", this.maxSessions }));
            }
            if (this._maxIdleTimeoutSec.HasValue)
            {
                object[] objArray8 = new object[] { "MaxIdleTimeoutms", 0x3e8 * this._maxIdleTimeoutSec };
                builder.Append(string.Format(CultureInfo.InvariantCulture, " {0}='{1}'", objArray8));
            }
            if (builder.Length <= 0)
            {
                return string.Empty;
            }
            return string.Format(CultureInfo.InvariantCulture, "<Quotas {0} />", new object[] { builder.ToString() });
        }

        internal override Hashtable ConstructQuotasAsHashtable()
        {
            Hashtable hashtable = new Hashtable();
            if (this._idleTimeoutSec.HasValue)
            {
                hashtable["IdleTimeoutms"] = (0x3e8 * this._idleTimeoutSec.Value).ToString(CultureInfo.InvariantCulture);
            }
            if (this.maxConcurrentUsers.HasValue)
            {
                hashtable["MaxConcurrentUsers"] = this.maxConcurrentUsers.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (this.maxProcessesPerSession.HasValue)
            {
                hashtable["MaxProcessesPerShell"] = this.maxProcessesPerSession.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (this.maxMemoryPerSessionMB.HasValue)
            {
                hashtable["MaxMemoryPerShellMB"] = this.maxMemoryPerSessionMB.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (this.maxSessionsPerUser.HasValue)
            {
                hashtable["MaxShellsPerUser"] = this.maxSessionsPerUser.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (this.maxConcurrentCommandsPerSession.HasValue)
            {
                hashtable["MaxConcurrentCommandsPerShell"] = this.maxConcurrentCommandsPerSession.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (this.maxSessions.HasValue)
            {
                hashtable["MaxShells"] = this.maxSessions.Value.ToString(CultureInfo.InvariantCulture);
            }
            if (this._maxIdleTimeoutSec.HasValue)
            {
                hashtable["MaxIdleTimeoutms"] = (0x3e8 * this._maxIdleTimeoutSec.Value).ToString(CultureInfo.InvariantCulture);
            }
            return hashtable;
        }

        protected internal override void LoadFromDefaults(PSSessionType sessionType, bool keepAssigned)
        {
            if (!keepAssigned || !this.outputBufferingMode.HasValue)
            {
                this.outputBufferingMode = DefaultOutputBufferingMode;
            }
            if (!keepAssigned || !this._processIdleTimeoutSec.HasValue)
            {
                this._processIdleTimeoutSec = (sessionType == PSSessionType.Workflow) ? DefaultProcessIdleTimeout_ForWorkflow : DefaultProcessIdleTimeout_ForPSRemoting;
            }
            if (!keepAssigned || !this._maxIdleTimeoutSec.HasValue)
            {
                this._maxIdleTimeoutSec = DefaultMaxIdleTimeout;
            }
            if (!keepAssigned || !this._idleTimeoutSec.HasValue)
            {
                this._idleTimeoutSec = DefaultIdleTimeout;
            }
            if (!keepAssigned || !this.maxConcurrentUsers.HasValue)
            {
                this.maxConcurrentUsers = DefaultMaxConcurrentUsers;
            }
            if (!keepAssigned || !this.maxProcessesPerSession.HasValue)
            {
                this.maxProcessesPerSession = DefaultMaxProcessesPerSession;
            }
            if (!keepAssigned || !this.maxMemoryPerSessionMB.HasValue)
            {
                this.maxMemoryPerSessionMB = DefaultMaxMemoryPerSessionMB;
            }
            if (!keepAssigned || !this.maxSessions.HasValue)
            {
                this.maxSessions = DefaultMaxSessions;
            }
            if (!keepAssigned || !this.maxSessionsPerUser.HasValue)
            {
                this.maxSessionsPerUser = DefaultMaxSessionsPerUser;
            }
            if (!keepAssigned || !this.maxConcurrentCommandsPerSession.HasValue)
            {
                this.maxConcurrentCommandsPerSession = DefaultMaxConcurrentCommandsPerSession;
            }
        }

        public int? IdleTimeoutSec
        {
            get
            {
                return this._idleTimeoutSec;
            }
            internal set
            {
                this._idleTimeoutSec = value;
            }
        }

        public int? MaxConcurrentCommandsPerSession
        {
            get
            {
                return this.maxConcurrentCommandsPerSession;
            }
            internal set
            {
                this.maxConcurrentCommandsPerSession = value;
            }
        }

        public int? MaxConcurrentUsers
        {
            get
            {
                return this.maxConcurrentUsers;
            }
            internal set
            {
                this.maxConcurrentUsers = value;
            }
        }

        public int? MaxIdleTimeoutSec
        {
            get
            {
                return this._maxIdleTimeoutSec;
            }
            internal set
            {
                this._maxIdleTimeoutSec = value;
            }
        }

        public int? MaxMemoryPerSessionMB
        {
            get
            {
                return this.maxMemoryPerSessionMB;
            }
            internal set
            {
                this.maxMemoryPerSessionMB = value;
            }
        }

        public int? MaxProcessesPerSession
        {
            get
            {
                return this.maxProcessesPerSession;
            }
            internal set
            {
                this.maxProcessesPerSession = value;
            }
        }

        public int? MaxSessions
        {
            get
            {
                return this.maxSessions;
            }
            internal set
            {
                this.maxSessions = value;
            }
        }

        public int? MaxSessionsPerUser
        {
            get
            {
                return this.maxSessionsPerUser;
            }
            internal set
            {
                this.maxSessionsPerUser = value;
            }
        }

        public System.Management.Automation.Runspaces.OutputBufferingMode? OutputBufferingMode
        {
            get
            {
                return this.outputBufferingMode;
            }
            internal set
            {
                this.outputBufferingMode = value;
            }
        }

        public int? ProcessIdleTimeoutSec
        {
            get
            {
                return this._processIdleTimeoutSec;
            }
            internal set
            {
                this._processIdleTimeoutSec = value;
            }
        }
    }
}

