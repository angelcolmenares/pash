using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{   
   [XmlType(Namespace = ManagementNamespaces.Namespace)]
   public class MaxElements// : IXmlSerializable
   {
      [XmlText]
      public int Value { get; set; }
      
      public MaxElements()
      {         
      }

      public MaxElements(int value)
      {
         Value = value;
      }

      public XmlSchema GetSchema()
      {
         return null;
      }

      public void ReadXml(XmlReader reader)
      {
         Value = reader.ReadElementContentAsInt();
      }

      public void WriteXml(XmlWriter writer)
      {
         writer.WriteValue(Value);
      }
   }
}