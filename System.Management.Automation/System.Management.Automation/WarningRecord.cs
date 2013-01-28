namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class WarningRecord : InformationalRecord
    {
        private string _fullyQualifiedWarningId;

        public WarningRecord(PSObject record) : base(record)
        {
        }

        public WarningRecord(string message) : base(message)
        {
        }

        public WarningRecord(string fullyQualifiedWarningId, PSObject record) : base(record)
        {
            this._fullyQualifiedWarningId = fullyQualifiedWarningId;
        }

        public WarningRecord(string fullyQualifiedWarningId, string message) : base(message)
        {
            this._fullyQualifiedWarningId = fullyQualifiedWarningId;
        }

        public string FullyQualifiedWarningId
        {
            get
            {
                if (this._fullyQualifiedWarningId == null)
                {
                    return string.Empty;
                }
                return this._fullyQualifiedWarningId;
            }
        }
    }
}

