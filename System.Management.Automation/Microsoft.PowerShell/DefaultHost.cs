namespace Microsoft.PowerShell
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Host;

    internal class DefaultHost : PSHost
    {
        private CultureInfo currentCulture;
        private CultureInfo currentUICulture;
        private Guid id = Guid.NewGuid();
        private System.Version ver = PSVersionInfo.PSVersion;
        private int _exitCode = 0;

        internal DefaultHost(CultureInfo currentCulture, CultureInfo currentUICulture)
        {
            this.currentCulture = currentCulture;
            this.currentUICulture = currentUICulture;
        }

        public override void EnterNestedPrompt()
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        public override void ExitNestedPrompt()
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        public override void NotifyBeginApplication()
        {
        }

        public override void NotifyEndApplication()
        {
        }

        public override void SetShouldExit(int exitCode)
        {
            _exitCode = exitCode;
        }

        /// <summary>
        /// 
        /// </summary>
        public override int ExitCode
        {
            get
            {
                return _exitCode;
            }
        }

        public override CultureInfo CurrentCulture
        {
            get
            {
                return this.currentCulture;
            }
        }

        public override CultureInfo CurrentUICulture
        {
            get
            {
                return this.currentUICulture;
            }
        }

        public override Guid InstanceId
        {
            get
            {
                return this.id;
            }
        }

        public override string Name
        {
            get
            {
                return "Default Host";
            }
        }

        public override PSHostUserInterface UI
        {
            get
            {
                return null;
            }
        }

        public override System.Version Version
        {
            get
            {
                return this.ver;
            }
        }
    }
}

