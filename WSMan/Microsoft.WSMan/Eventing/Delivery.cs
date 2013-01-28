using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Eventing
{
   [XmlType(Namespace = EventingActions.Namespace)]
   public class Delivery : IXmlSerializable
   {

      public static Delivery Pull()
      {
         return new Delivery { Mode = DeliveryModePull };
      }

      [XmlAttribute(Namespace = EventingActions.Namespace, DataType = "anyURI", Type = typeof(string) )]
      public string Mode { get; set; }

      [XmlAnyElement]
      public XmlNode[] Any { get; set; }

      [XmlAnyAttribute]
      public XmlAttribute[] AnyAttr { get; set; }

      public const string DeliveryModePull = @"http://schemas.dmtf.org/wbem/wsman/1/wsman/Pull";

		public XmlSchema GetSchema()
		{
			return null;
		}
		
		public void ReadXml(XmlReader reader)
		{
			Mode = reader.ReadString();
			reader.ReadEndElement();
		}
		
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteValue(Mode);
		}
   }   
}