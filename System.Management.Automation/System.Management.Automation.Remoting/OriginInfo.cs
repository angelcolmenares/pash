namespace System.Management.Automation.Remoting
{
    using System;
    using System.Runtime.Serialization;

    [Serializable, DataContract]
    public class OriginInfo
    {
        [DataMember]
        private string _computerName;
        [DataMember]
        private Guid _instanceId;
        [DataMember]
        private Guid _runspaceID;

        public OriginInfo(string computerName, Guid runspaceID) : this(computerName, runspaceID, Guid.Empty)
        {
        }

        public OriginInfo(string computerName, Guid runspaceID, Guid instanceID)
        {
            this._computerName = computerName;
            this._runspaceID = runspaceID;
            this._instanceId = instanceID;
        }

        public override string ToString()
        {
            return this.PSComputerName;
        }

        public Guid InstanceID
        {
            get
            {
                return this._instanceId;
            }
            set
            {
                this._instanceId = value;
            }
        }

        public string PSComputerName
        {
            get
            {
                return this._computerName;
            }
        }

        public Guid RunspaceID
        {
            get
            {
                return this._runspaceID;
            }
        }
    }
}

