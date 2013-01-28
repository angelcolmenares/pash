namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class RemotingErrorRecord : ErrorRecord
    {
        private System.Management.Automation.Remoting.OriginInfo _originInfo;

        public RemotingErrorRecord(ErrorRecord errorRecord, System.Management.Automation.Remoting.OriginInfo originInfo) : this(errorRecord, originInfo, null)
        {
        }

        protected RemotingErrorRecord(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._originInfo = (System.Management.Automation.Remoting.OriginInfo) info.GetValue("RemoteErrorRecord_OriginInfo", typeof(System.Management.Automation.Remoting.OriginInfo));
        }

        private RemotingErrorRecord(ErrorRecord errorRecord, System.Management.Automation.Remoting.OriginInfo originInfo, Exception replaceParentContainsErrorRecordException) : base(errorRecord, replaceParentContainsErrorRecordException)
        {
            if (errorRecord != null)
            {
                base.SetInvocationInfo(errorRecord.InvocationInfo);
            }
            this._originInfo = originInfo;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("RemoteErrorRecord_OriginInfo", this._originInfo);
        }

        internal override ErrorRecord WrapException(Exception replaceParentContainsErrorRecordException)
        {
            return new RemotingErrorRecord(this, this.OriginInfo, replaceParentContainsErrorRecordException);
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

