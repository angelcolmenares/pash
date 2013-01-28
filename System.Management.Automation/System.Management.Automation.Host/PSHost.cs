namespace System.Management.Automation.Host
{
    using System;
    using System.Globalization;
    using System.Management.Automation;

    public abstract class PSHost
    {
        internal static bool IsStdOutputRedirected;
        internal const int MaximumNestedPromptLevel = 0x80;
        private bool shouldSetThreadUILanguageToZero;

        protected PSHost()
        {

        }

        public virtual int ExitCode
        {
            get { return 0; }
        }

        public virtual bool ShouldExit
        {
            get { return ExitCode == Int32.MaxValue; }
        }

        public abstract void EnterNestedPrompt();
        public abstract void ExitNestedPrompt();
        public abstract void NotifyBeginApplication();
        public abstract void NotifyEndApplication();
        public abstract void SetShouldExit(int exitCode);

        public abstract CultureInfo CurrentCulture { get; }

        public abstract CultureInfo CurrentUICulture { get; }

        public abstract Guid InstanceId { get; }

        public abstract string Name { get; }

        public virtual PSObject PrivateData
        {
            get
            {
                return null;
            }
        }

        internal bool ShouldSetThreadUILanguageToZero
        {
            get
            {
                return this.shouldSetThreadUILanguageToZero;
            }
            set
            {
                this.shouldSetThreadUILanguageToZero = value;
            }
        }

        public abstract PSHostUserInterface UI { get; }

        public abstract System.Version Version { get; }
    }
}

