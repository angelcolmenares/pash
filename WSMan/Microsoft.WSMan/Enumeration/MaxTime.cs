using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Enumeration
{
   public class MaxTime : IXmlSerializable
   {
      private TimeSpan _value;

      public TimeSpan Value
      {
         get { return _value; }
      }

      public MaxTime()
      {         
      }

      public MaxTime(TimeSpan value)
      {
         _value = value;
      }

      public XmlSchema GetSchema()
      {
         return null;
      }

      public void ReadXml(XmlReader reader)
      {
         _value = XmlConvert.ToTimeSpan(reader.ReadString());
         reader.ReadEndElement();
      }

      public void WriteXml(XmlWriter writer)
      {
         writer.WriteValue(XmlConvert.ToString(_value)); 
      }
   }
}