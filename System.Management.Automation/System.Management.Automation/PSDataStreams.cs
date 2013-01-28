namespace System.Management.Automation
{
    using System;

    public sealed class PSDataStreams
    {
        private PowerShell powershell;

        internal PSDataStreams(PowerShell powershell)
        {
            this.powershell = powershell;
        }

        public void ClearStreams()
        {
            this.Error.Clear();
            this.Progress.Clear();
            this.Verbose.Clear();
            this.Debug.Clear();
            this.Warning.Clear();
        }

        public PSDataCollection<DebugRecord> Debug
        {
            get
            {
                return this.powershell.DebugBuffer;
            }
            set
            {
                this.powershell.DebugBuffer = value;
            }
        }

        public PSDataCollection<ErrorRecord> Error
        {
            get
            {
                return this.powershell.ErrorBuffer;
            }
            set
            {
                this.powershell.ErrorBuffer = value;
            }
        }

        public PSDataCollection<ProgressRecord> Progress
        {
            get
            {
                return this.powershell.ProgressBuffer;
            }
            set
            {
                this.powershell.ProgressBuffer = value;
            }
        }

        public PSDataCollection<VerboseRecord> Verbose
        {
            get
            {
                return this.powershell.VerboseBuffer;
            }
            set
            {
                this.powershell.VerboseBuffer = value;
            }
        }

        public PSDataCollection<WarningRecord> Warning
        {
            get
            {
                return this.powershell.WarningBuffer;
            }
            set
            {
                this.powershell.WarningBuffer = value;
            }
        }
    }
}

