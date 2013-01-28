namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Xml;

    public class PSSerializer
    {
        private static int mshDefaultSerializationDepth = 50;

        internal PSSerializer()
        {
        }

        public static object Deserialize(string source)
        {
            object[] objArray = DeserializeAsList(source);
            if (objArray.Length == 0)
            {
                return null;
            }
            if (objArray.Length == 1)
            {
                return objArray[0];
            }
            return objArray;
        }

        public static object[] DeserializeAsList(string source)
        {
            ArrayList list = new ArrayList();
            TextReader input = new StringReader(source);
            Deserializer deserializer = new Deserializer(XmlReader.Create(input, InternalDeserializer.XmlReaderSettingsForCliXml));
            while (!deserializer.Done())
            {
                object obj2 = deserializer.Deserialize();
                list.Add(obj2);
            }
            return list.ToArray();
        }

        public static string Serialize(object source)
        {
            return Serialize(source, mshDefaultSerializationDepth);
        }

        public static string Serialize(object source, int depth)
        {
            StringBuilder output = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings {
                CloseOutput = true,
                Encoding = Encoding.Unicode,
                Indent = true,
                OmitXmlDeclaration = true
            };
            Serializer serializer = new Serializer(XmlWriter.Create(output, settings), depth, true);
            serializer.Serialize(source);
            serializer.Done();
            serializer = null;
            return output.ToString();
        }
    }
}

