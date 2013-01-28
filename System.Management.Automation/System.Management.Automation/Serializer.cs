namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Runspaces;
    using System.Xml;

    internal class Serializer
    {
        private readonly InternalSerializer _serializer;

        internal Serializer(XmlWriter writer) : this(writer, new SerializationContext())
        {
        }

        internal Serializer(XmlWriter writer, SerializationContext context)
        {
            if (writer == null)
            {
                throw PSTraceSource.NewArgumentException("writer");
            }
            if (context == null)
            {
                throw PSTraceSource.NewArgumentException("context");
            }
            this._serializer = new InternalSerializer(writer, context);
            this._serializer.Start();
        }

        internal Serializer(XmlWriter writer, int depth, bool useDepthFromTypes) : this(writer, new SerializationContext(depth, useDepthFromTypes))
        {
        }

        internal void Done()
        {
            this._serializer.End();
        }

        internal void Serialize(object source)
        {
            this.Serialize(source, null);
        }

        internal void Serialize(object source, string streamName)
        {
            this._serializer.WriteOneTopLevelObject(source, streamName);
        }

        internal void Stop()
        {
            this._serializer.Stop();
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                return this._serializer.TypeTable;
            }
            set
            {
                this._serializer.TypeTable = value;
            }
        }
    }
}

