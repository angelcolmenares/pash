namespace System.Management.Automation
{
    using System;

    internal class LogContext
    {
        private string _commandLine = "";
        private string _commandName = "";
        private string _commandPath = "";
        private string _commandType = "";
        private string _engineVersion = "";
        private System.Management.Automation.ExecutionContext _executionContext;
        private string _hostId = "";
        private string _hostName = "";
        private string _hostVersion = "";
        private string _pipelineId = "";
        private string _runspaceId = "";
        private string _scriptName = "";
        private string _sequenceNumber = "";
        private string _severity = "";
        private string _shellId;
        private string _time = "";
        private string _user = "";

        internal string CommandLine
        {
            get
            {
                return this._commandLine;
            }
            set
            {
                this._commandLine = value;
            }
        }

        internal string CommandName
        {
            get
            {
                return this._commandName;
            }
            set
            {
                this._commandName = value;
            }
        }

        internal string CommandPath
        {
            get
            {
                return this._commandPath;
            }
            set
            {
                this._commandPath = value;
            }
        }

        internal string CommandType
        {
            get
            {
                return this._commandType;
            }
            set
            {
                this._commandType = value;
            }
        }

        internal string EngineVersion
        {
            get
            {
                return this._engineVersion;
            }
            set
            {
                this._engineVersion = value;
            }
        }

        internal System.Management.Automation.ExecutionContext ExecutionContext
        {
            get
            {
                return this._executionContext;
            }
            set
            {
                this._executionContext = value;
            }
        }

        internal string HostId
        {
            get
            {
                return this._hostId;
            }
            set
            {
                this._hostId = value;
            }
        }

        internal string HostName
        {
            get
            {
                return this._hostName;
            }
            set
            {
                this._hostName = value;
            }
        }

        internal string HostVersion
        {
            get
            {
                return this._hostVersion;
            }
            set
            {
                this._hostVersion = value;
            }
        }

        internal string PipelineId
        {
            get
            {
                return this._pipelineId;
            }
            set
            {
                this._pipelineId = value;
            }
        }

        internal string RunspaceId
        {
            get
            {
                return this._runspaceId;
            }
            set
            {
                this._runspaceId = value;
            }
        }

        internal string ScriptName
        {
            get
            {
                return this._scriptName;
            }
            set
            {
                this._scriptName = value;
            }
        }

        internal string SequenceNumber
        {
            get
            {
                return this._sequenceNumber;
            }
            set
            {
                this._sequenceNumber = value;
            }
        }

        internal string Severity
        {
            get
            {
                return this._severity;
            }
            set
            {
                this._severity = value;
            }
        }

        internal string ShellId
        {
            get
            {
                return this._shellId;
            }
            set
            {
                this._shellId = value;
            }
        }

        internal string Time
        {
            get
            {
                return this._time;
            }
            set
            {
                this._time = value;
            }
        }

        internal string User
        {
            get
            {
                return this._user;
            }
            set
            {
                this._user = value;
            }
        }
    }
}

