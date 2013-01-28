namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Runtime.Serialization;

    [DataContract]
    public class RemotingProgressRecord : ProgressRecord
    {
        [DataMember]
        private readonly System.Management.Automation.Remoting.OriginInfo _originInfo;

        public RemotingProgressRecord(ProgressRecord progressRecord, System.Management.Automation.Remoting.OriginInfo originInfo) : base(Validate(progressRecord).ActivityId, Validate(progressRecord).Activity, Validate(progressRecord).StatusDescription)
        {
            this._originInfo = originInfo;
            if (progressRecord != null)
            {
                base.PercentComplete = progressRecord.PercentComplete;
                base.ParentActivityId = progressRecord.ParentActivityId;
                base.RecordType = progressRecord.RecordType;
                base.SecondsRemaining = progressRecord.SecondsRemaining;
                if (!string.IsNullOrEmpty(progressRecord.CurrentOperation))
                {
                    base.CurrentOperation = progressRecord.CurrentOperation;
                }
            }
        }

        private static ProgressRecord Validate(ProgressRecord progressRecord)
        {
            if (progressRecord == null)
            {
                throw new ArgumentNullException("progressRecord");
            }
            return progressRecord;
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

