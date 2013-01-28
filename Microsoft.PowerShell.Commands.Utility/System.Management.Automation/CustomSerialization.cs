namespace System.Management.Automation
{
    using System;
    using System.Xml;

    internal class CustomSerialization
    {
        private int _depth;
        private bool _notypeinformation;
        private CustomInternalSerializer _serializer;
        private XmlWriter _writer;
        private bool firstCall;
        private static int mshDefaultSerializationDepth = 1;

        internal CustomSerialization(XmlWriter writer, bool notypeinformation) : this(writer, notypeinformation, MshDefaultSerializationDepth)
        {
        }

        internal CustomSerialization(XmlWriter writer, bool notypeinformation, int depth)
        {
            this.firstCall = true;
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentException("writer");
            }
            if (depth < 1)
            {
                throw PSTraceSource.NewArgumentException("writer", "serialization", "DepthOfOneRequired", new object[0]);
            }
            this._depth = depth;
            this._writer = writer;
            this._notypeinformation = notypeinformation;
            this._serializer = null;
        }

        internal void Done()
        {
            if (this.firstCall)
            {
                this.firstCall = false;
                this.Start();
            }
            this._writer.WriteEndElement();
            this._writer.Flush();
        }

        internal void DoneAsStream()
        {
            this._writer.Flush();
        }

        internal void Serialize(object source)
        {
            if (this.firstCall)
            {
                this.firstCall = false;
                this.Start();
            }
            this._serializer = new CustomInternalSerializer(this._writer, this._notypeinformation, true);
            this._serializer.WriteOneObject(source, null, this._depth);
            this._serializer = null;
        }

        internal void SerializeAsStream(object source)
        {
            this._serializer = new CustomInternalSerializer(this._writer, this._notypeinformation, true);
            this._serializer.WriteOneObject(source, null, this._depth);
            this._serializer = null;
        }

        private void Start()
        {
            CustomInternalSerializer.WriteStartElement(this._writer, "Objects");
        }

        internal void Stop()
        {
            CustomInternalSerializer serializer = this._serializer;
            if (serializer != null)
            {
                serializer.Stop();
            }
        }

        public static int MshDefaultSerializationDepth
        {
            get
            {
                return mshDefaultSerializationDepth;
            }
        }
    }
}

