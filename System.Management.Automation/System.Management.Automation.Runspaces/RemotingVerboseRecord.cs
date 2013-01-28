namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Remoting;
    using System.Runtime.Serialization;

    [DataContract]
    public class RemotingVerboseRecord : VerboseRecord
    {
        [DataMember]
        private readonly System.Management.Automation.Remoting.OriginInfo _originInfo;

        public RemotingVerboseRecord(string message, System.Management.Automation.Remoting.OriginInfo originInfo) : base(message)
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

