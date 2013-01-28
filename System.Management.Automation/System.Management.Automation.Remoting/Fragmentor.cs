namespace System.Management.Automation.Remoting
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Text;
    using System.Xml;

    internal class Fragmentor
    {
        private System.Management.Automation.DeserializationContext _deserializationContext;
        private int _fragmentSize;
        private SerializationContext _serializationContext;
        private System.Management.Automation.Runspaces.TypeTable _typeTable;
        private const int SerializationDepthForRemoting = 1;

        internal Fragmentor(int fragmentSize, PSRemotingCryptoHelper cryptoHelper)
        {
            this._fragmentSize = fragmentSize;
			this._serializationContext = new SerializationContext(SerializationDepthForRemoting, SerializationOptions.RemotingOptions, cryptoHelper);
            this._deserializationContext = new System.Management.Automation.DeserializationContext(DeserializationOptions.RemotingOptions, cryptoHelper);
        }

        internal PSObject DeserializeToPSObject(Stream serializedDataStream)
        {
            object obj2 = null;
            using (XmlReader reader = XmlReader.Create(serializedDataStream, InternalDeserializer.XmlReaderSettingsForCliXml))
            {
                Deserializer deserializer = new Deserializer(reader, this._deserializationContext) {
                    TypeTable = this._typeTable
                };
                obj2 = deserializer.Deserialize();
                deserializer.Done();
            }
            if (obj2 == null)
            {
                throw new PSRemotingDataStructureException(RemotingErrorIdStrings.DeserializedObjectIsNull);
            }
            return PSObject.AsPSObject(obj2);
        }

        internal void Fragment<T>(RemoteDataObject<T> obj, SerializedDataStream dataToBeSent)
        {
            dataToBeSent.Enter();
            try
            {
                obj.Serialize(dataToBeSent, this);
            }
            finally
            {
                dataToBeSent.Exit();
            }
        }

        internal void SerializeToBytes(object obj, Stream streamToWriteTo)
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                CheckCharacters = false,
                Indent = false,
                CloseOutput = false,
                Encoding = Encoding.UTF8,
                NewLineHandling = NewLineHandling.None,
                OmitXmlDeclaration = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };
            using (XmlWriter writer = XmlWriter.Create(streamToWriteTo, settings))
            {
                Serializer serializer = new Serializer(writer, this._serializationContext) {
                    TypeTable = this._typeTable
                };
                serializer.Serialize(obj);
                serializer.Done();
                writer.Flush();
            }
			/*
			var falseStream = new MemoryStream();
			using (XmlWriter writer = XmlWriter.Create(falseStream, settings))
			{
				Serializer serializer = new Serializer(writer, this._serializationContext) {
					TypeTable = this._typeTable
				};
				serializer.Serialize(obj);
				serializer.Done();
				writer.Flush();
			}

			var newString = new StreamReader(falseStream, Encoding.UTF8).ReadToEnd();
			*/


        }

        internal System.Management.Automation.DeserializationContext DeserializationContext
        {
            get
            {
                return this._deserializationContext;
            }
        }

        internal int FragmentSize
        {
            get
            {
                return this._fragmentSize;
            }
            set
            {
                this._fragmentSize = value;
            }
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                return this._typeTable;
            }
            set
            {
                this._typeTable = value;
            }
        }
    }
}

