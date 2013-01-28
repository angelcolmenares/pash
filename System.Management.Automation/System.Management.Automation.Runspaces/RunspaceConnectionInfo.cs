namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Threading;

    public abstract class RunspaceConnectionInfo
    {
        private int cancelTimeout = 0xea60;
        private CultureInfo culture = Thread.CurrentThread.CurrentCulture;
        internal const int defaultCancelTimeout = 0xea60;
        internal const int DefaultIdleTimeout = -1;
        internal const int DefaultOpenTimeout = 0x2bf20;
        internal const int DefaultTimeout = -1;
        private int idleTimeout = -1;
        internal const int InfiniteTimeout = 0;
        private int maxIdleTimeout = 0x7fffffff;
        private int openTimeout = 0x2bf20;
        private int operationTimeout = 0x2bf20;
        private CultureInfo uiCulture = Thread.CurrentThread.CurrentUICulture;

        protected RunspaceConnectionInfo()
        {
        }

        public virtual void SetSessionOptions(PSSessionOption options)
        {
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
            if (options.Culture != null)
            {
                this.Culture = options.Culture;
            }
            if (options.UICulture != null)
            {
                this.UICulture = options.UICulture;
            }
            this.openTimeout = this.TimeSpanToTimeOutMs(options.OpenTimeout);
            this.cancelTimeout = this.TimeSpanToTimeOutMs(options.CancelTimeout);
            this.operationTimeout = this.TimeSpanToTimeOutMs(options.OperationTimeout);
            this.idleTimeout = ((options.IdleTimeout.TotalMilliseconds >= -1.0) && (options.IdleTimeout.TotalMilliseconds < 2147483647.0)) ? ((int) options.IdleTimeout.TotalMilliseconds) : 0x7fffffff;
        }

        internal int TimeSpanToTimeOutMs(TimeSpan t)
        {
            if (((t.TotalMilliseconds <= 2147483647.0) && !(t == TimeSpan.MaxValue)) && (t.TotalMilliseconds >= 0.0))
            {
                return (int) t.TotalMilliseconds;
            }
            return 0x7fffffff;
        }

        public abstract System.Management.Automation.Runspaces.AuthenticationMechanism AuthenticationMechanism { get; set; }

        public int CancelTimeout
        {
            get
            {
                return this.cancelTimeout;
            }
            set
            {
                this.cancelTimeout = value;
            }
        }

        public abstract string CertificateThumbprint { get; set; }

        public abstract string ComputerName { get; set; }

        public abstract PSCredential Credential { get; set; }

        public CultureInfo Culture
        {
            get
            {
                return this.culture;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.culture = value;
            }
        }

        public int IdleTimeout
        {
            get
            {
                return this.idleTimeout;
            }
            set
            {
                this.idleTimeout = value;
            }
        }

        internal int MaxIdleTimeout
        {
            get
            {
                return this.maxIdleTimeout;
            }
            set
            {
                this.maxIdleTimeout = value;
            }
        }

        public int OpenTimeout
        {
            get
            {
                return this.openTimeout;
            }
            set
            {
                this.openTimeout = value;
                if ((this is WSManConnectionInfo) && (this.openTimeout == -1))
                {
                    this.openTimeout = 0x2bf20;
                }
                else if ((this is WSManConnectionInfo) && (this.openTimeout == 0))
                {
                    this.openTimeout = 0x7fffffff;
                }
            }
        }

        public int OperationTimeout
        {
            get
            {
                return this.operationTimeout;
            }
            set
            {
                this.operationTimeout = value;
            }
        }

        public CultureInfo UICulture
        {
            get
            {
                return this.uiCulture;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.uiCulture = value;
            }
        }

		public abstract PSObject ToPSObjectForRemoting();

    }
}

