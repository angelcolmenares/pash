using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Management
{
   public class XmlFragment<T> : IXmlSerializable
   {      
      private readonly XmlSerializer _xs = new XmlSerializer(typeof(T), 
			new XmlRootAttribute("XmlFragment"){Namespace = ManagementNamespaces.Namespace});

      private T _value;

      public XmlFragment(T value)
      {
         _value = value;
      }

      public XmlFragment()
      {         
      }

      public T Value
      {
         get { return _value; }
      }

      public XmlSchema GetSchema()
      {
         return null;
      }

      public void ReadXml(XmlReader reader)
      {
         _value = (T) _xs.Deserialize(reader);
      }

      public void WriteXml(XmlWriter writer)
      {
         _xs.Serialize(writer, _value);
      }
   }
}