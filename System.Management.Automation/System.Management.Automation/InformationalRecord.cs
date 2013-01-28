namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class InformationalRecord
    {
        private System.Management.Automation.InvocationInfo invocationInfo;
        [DataMember]
        private string message;
        private ReadOnlyCollection<int> pipelineIterationInfo;
        private bool serializeExtendedInfo;

        internal InformationalRecord(PSObject serializedObject)
        {
            this.message = (string) SerializationUtilities.GetPropertyValue(serializedObject, "InformationalRecord_Message");
            this.serializeExtendedInfo = (bool) SerializationUtilities.GetPropertyValue(serializedObject, "InformationalRecord_SerializeInvocationInfo");
            if (this.serializeExtendedInfo)
            {
                this.invocationInfo = new System.Management.Automation.InvocationInfo(serializedObject);
                ArrayList psObjectPropertyBaseObject = (ArrayList) SerializationUtilities.GetPsObjectPropertyBaseObject(serializedObject, "InformationalRecord_PipelineIterationInfo");
                this.pipelineIterationInfo = new ReadOnlyCollection<int>((int[]) psObjectPropertyBaseObject.ToArray(Type.GetType("System.Int32")));
            }
            else
            {
                this.invocationInfo = null;
            }
        }

        internal InformationalRecord(string message)
        {
            this.message = message;
            this.invocationInfo = null;
            this.pipelineIterationInfo = null;
            this.serializeExtendedInfo = false;
        }

        internal void SetInvocationInfo(System.Management.Automation.InvocationInfo invocationInfo)
        {
            this.invocationInfo = invocationInfo;
            if (invocationInfo.PipelineIterationInfo != null)
            {
                int[] list = (int[]) invocationInfo.PipelineIterationInfo.Clone();
                this.pipelineIterationInfo = new ReadOnlyCollection<int>(list);
            }
        }

        internal virtual void ToPSObjectForRemoting(PSObject psObject)
        {
            RemotingEncoder.ValueGetterDelegate<object> valueGetter = null;
            RemotingEncoder.AddNoteProperty<string>(psObject, "InformationalRecord_Message", () => this.Message);
            if (!this.SerializeExtendedInfo || (this.invocationInfo == null))
            {
                RemotingEncoder.AddNoteProperty<bool>(psObject, "InformationalRecord_SerializeInvocationInfo", () => false);
            }
            else
            {
                RemotingEncoder.AddNoteProperty<bool>(psObject, "InformationalRecord_SerializeInvocationInfo", () => true);
                this.invocationInfo.ToPSObjectForRemoting(psObject);
                if (valueGetter == null)
                {
                    valueGetter = () => this.PipelineIterationInfo;
                }
                RemotingEncoder.AddNoteProperty<object>(psObject, "InformationalRecord_PipelineIterationInfo", valueGetter);
            }
        }

        public override string ToString()
        {
            return this.Message;
        }

        public System.Management.Automation.InvocationInfo InvocationInfo
        {
            get
            {
                return this.invocationInfo;
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                this.message = value;
            }
        }

        public ReadOnlyCollection<int> PipelineIterationInfo
        {
            get
            {
                return this.pipelineIterationInfo;
            }
        }

        internal bool SerializeExtendedInfo
        {
            get
            {
                return this.serializeExtendedInfo;
            }
            set
            {
                this.serializeExtendedInfo = value;
            }
        }
    }
}

