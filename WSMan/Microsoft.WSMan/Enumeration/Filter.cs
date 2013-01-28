using System;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Enumeration
{
   public class Filter : IXmlSerializable
   {
      private const string DialectAttribute = "Dialect";
      private string _dialect;
      private object _value;

      public Filter(string dialect, object value)
      {
         //TODO: Add test if value can be serialized using provided dialect.
         _value = value;
         _dialect = dialect;
      }

      public Filter()
      {
      }

      public object Value
      {
         get { return _value; }
      }

      public string Dialect
      {
         get { return _dialect; }
      }

      public XmlSchema GetSchema()
      {
         return null;
      }

      public void ReadXml(XmlReader reader)
      {
         _dialect = reader.GetAttribute(DialectAttribute);
         Type type = FilterMapExtension.GetDialectType(_dialect);
         if (type == null)
         {
            throw CreateNotSupportedDialectException();
         }
         if (type == typeof(void))
         {
            return;
         }
         if (typeof(IXmlSerializable).IsAssignableFrom(type))
         {
            IXmlSerializable serializable = (IXmlSerializable)Activator.CreateInstance(type);
            serializable.ReadXml(reader);
            return;
         }
         XmlSerializer serializer = new XmlSerializer(type);
         if (!reader.IsEmptyElement)
         {
            reader.ReadStartElement("Filter", reader.NamespaceURI);
            _value = serializer.Deserialize(reader);
            reader.ReadEndElement();
         }
      }

      public void WriteXml(XmlWriter writer)
      {
         writer.WriteAttributeString(DialectAttribute, _dialect);
         IXmlSerializable serializable = _value as IXmlSerializable;
         if (serializable != null)
         {
            serializable.WriteXml(writer);
            return;
         }
         if (_value != null)
         {
            XmlSerializer serializer = new XmlSerializer(_value.GetType());
            serializer.Serialize(writer, _value);
         }
      }

      private static Exception CreateNotSupportedDialectException()
      {
         return new FaultException("The requested filtering dialect is not supported",
                                  FaultCode.CreateSenderFaultCode("FilterDialectRequestedUnavailable", EnumerationActions.Namespace),
                                  "http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault");
      }
   }
}