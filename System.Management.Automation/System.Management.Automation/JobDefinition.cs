namespace System.Management.Automation
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    [Serializable]
    public class JobDefinition : ISerializable
    {
        private readonly string _command;
        private Guid _instanceId;
        private readonly Type _jobSourceAdapterType;
        private string _jobSourceAdapterTypeName;
        private string _moduleName;
        private string _name;

        protected JobDefinition(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public JobDefinition(Type jobSourceAdapterType, string command, string name)
        {
            this._jobSourceAdapterType = jobSourceAdapterType;
            if (jobSourceAdapterType != null)
            {
                this._jobSourceAdapterTypeName = jobSourceAdapterType.Name;
            }
            this._command = command;
            this._name = name;
            this._instanceId = Guid.NewGuid();
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public virtual void Load(Stream stream)
        {
            throw new NotImplementedException();
        }

        public virtual void Save(Stream stream)
        {
            throw new NotImplementedException();
        }

        public string Command
        {
            get
            {
                return this._command;
            }
        }

        public System.Management.Automation.CommandInfo CommandInfo
        {
            get
            {
                return null;
            }
        }

        public Guid InstanceId
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

        public Type JobSourceAdapterType
        {
            get
            {
                return this._jobSourceAdapterType;
            }
        }

        public string JobSourceAdapterTypeName
        {
            get
            {
                return this._jobSourceAdapterTypeName;
            }
            set
            {
                this._jobSourceAdapterTypeName = value;
            }
        }

        public string ModuleName
        {
            get
            {
                return this._moduleName;
            }
            set
            {
                this._moduleName = value;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }
    }
}

