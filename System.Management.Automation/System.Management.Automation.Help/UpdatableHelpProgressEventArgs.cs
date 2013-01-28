namespace System.Management.Automation.Help
{
    using System;

    internal class UpdatableHelpProgressEventArgs : EventArgs
    {
        private string _moduleName;
        private int _progressPercent;
        private string _progressStatus;
        private UpdatableHelpCommandType _type;

        internal UpdatableHelpProgressEventArgs(string moduleName, string status, int percent)
        {
            this._type = UpdatableHelpCommandType.UnknownCommand;
            this._progressStatus = status;
            this._progressPercent = percent;
            this._moduleName = moduleName;
        }

        internal UpdatableHelpProgressEventArgs(string moduleName, UpdatableHelpCommandType type, string status, int percent)
        {
            this._type = type;
            this._progressStatus = status;
            this._progressPercent = percent;
            this._moduleName = moduleName;
        }

        internal UpdatableHelpCommandType CommandType
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
            }
        }

        internal string ModuleName
        {
            get
            {
                return this._moduleName;
            }
        }

        internal int ProgressPercent
        {
            get
            {
                return this._progressPercent;
            }
        }

        internal string ProgressStatus
        {
            get
            {
                return this._progressStatus;
            }
        }
    }
}

