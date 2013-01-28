namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Runtime.Serialization;

    [DataContract]
    public class RemotingWarningRecord : WarningRecord
    {
        [DataMember]
        private readonly System.Management.Automation.Remoting.OriginInfo _originInfo;

        internal RemotingWarningRecord(WarningRecord warningRecord, System.Management.Automation.Remoting.OriginInfo originInfo) : base(warningRecord.FullyQualifiedWarningId, warningRecord.Message)
        {
            this._originInfo = originInfo;
        }

        public RemotingWarningRecord(string message, System.Management.Automation.Remoting.OriginInfo originInfo) : base(message)
        {
            this._originInfo = originInfo;
        }

        public System.Management.Automation.Remoting.OriginInfo OriginInfo
        {
            get
            {
                return this._originInfo;
            }
        }
    }
}

