namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management.Automation.Runspaces;
    using System.Runtime.Serialization;

    [Serializable]
    public class JobInvocationInfo : ISerializable
    {
        private string _command;
        private JobDefinition _definition;
        private readonly Guid _instanceId;
        private string _name;
        private List<CommandParameterCollection> _parameters;

        protected JobInvocationInfo()
        {
            this._name = string.Empty;
            this._instanceId = Guid.NewGuid();
        }

        public JobInvocationInfo(JobDefinition definition, Dictionary<string, object> parameters)
        {
            this._name = string.Empty;
            this._instanceId = Guid.NewGuid();
            this._definition = definition;
            CommandParameterCollection item = ConvertDictionaryToParameterCollection(parameters);
            if (item != null)
            {
                this.Parameters.Add(item);
            }
        }

        public JobInvocationInfo(JobDefinition definition, IEnumerable<Dictionary<string, object>> parameterCollectionList)
        {
            this._name = string.Empty;
            this._instanceId = Guid.NewGuid();
            this._definition = definition;
            if (parameterCollectionList != null)
            {
                foreach (Dictionary<string, object> dictionary in parameterCollectionList)
                {
                    if (dictionary != null)
                    {
                        CommandParameterCollection item = ConvertDictionaryToParameterCollection(dictionary);
                        if (item != null)
                        {
                            this.Parameters.Add(item);
                        }
                    }
                }
            }
        }

        public JobInvocationInfo(JobDefinition definition, IEnumerable<CommandParameterCollection> parameters)
        {
            this._name = string.Empty;
            this._instanceId = Guid.NewGuid();
            this._definition = definition;
            if (parameters != null)
            {
                foreach (CommandParameterCollection parameters2 in parameters)
                {
                    this.Parameters.Add(parameters2);
                }
            }
        }

        public JobInvocationInfo(JobDefinition definition, CommandParameterCollection parameters)
        {
            this._name = string.Empty;
            this._instanceId = Guid.NewGuid();
            this._definition = definition;
            this.Parameters.Add(parameters ?? new CommandParameterCollection());
        }

        protected JobInvocationInfo(SerializationInfo info, StreamingContext context)
        {
            this._name = string.Empty;
            this._instanceId = Guid.NewGuid();
            throw new NotImplementedException();
        }

        private static CommandParameterCollection ConvertDictionaryToParameterCollection(IEnumerable<KeyValuePair<string, object>> parameters)
        {
            if (parameters == null)
            {
                return null;
            }
            CommandParameterCollection parameters2 = new CommandParameterCollection();
            foreach (CommandParameter parameter in from param in parameters select new CommandParameter(param.Key, param.Value))
            {
                parameters2.Add(parameter);
            }
            return parameters2;
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
                return (this._command ?? this._definition.Command);
            }
            set
            {
                this._command = value;
            }
        }

        public JobDefinition Definition
        {
            get
            {
                return this._definition;
            }
            set
            {
                this._definition = value;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this._instanceId;
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
                if (value == null)
                {
                    throw new PSArgumentNullException("value");
                }
                this._name = value;
            }
        }

        public List<CommandParameterCollection> Parameters
        {
            get
            {
                return (this._parameters ?? (this._parameters = new List<CommandParameterCollection>()));
            }
        }
    }
}

