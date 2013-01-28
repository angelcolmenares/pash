namespace System.Management.Automation
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ScriptBlockSerializationHelper : ISerializable, IObjectReference
    {
        private readonly string scriptText;

        private ScriptBlockSerializationHelper(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.scriptText = info.GetValue("ScriptText", typeof(string)) as string;
            if (this.scriptText == null)
            {
                throw PSTraceSource.NewArgumentNullException("info");
            }
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException();
        }

        public object GetRealObject(StreamingContext context)
        {
            return ScriptBlock.Create(this.scriptText);
        }
    }
}

