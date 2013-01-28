using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Enumeration
{
   public class EnumerationItemList : IXmlSerializable
   {
      private readonly List<EnumerationItem> _items;

      public EnumerationItemList()
      {         
         _items = new List<EnumerationItem>();             
      }

      public EnumerationItemList(IEnumerable<EnumerationItem> items)
      {
         _items = new List<EnumerationItem>(items);
      }

      public IEnumerable<EnumerationItem> Items
      {
         get { return _items; }
      }

      public XmlSchema GetSchema()
      {
         return null;
      }

      public void ReadXml(XmlReader reader)
      {
         if (reader.IsEmptyElement)
         {
            reader.ReadStartElement("Items", reader.NamespaceURI);
            return;
         }
         reader.ReadStartElement("Items", reader.NamespaceURI);
         while (reader.NodeType != XmlNodeType.EndElement)
         {
            EnumerationItem item = new EnumerationItem();
            item.ReadXml(reader);
            _items.Add(item);
         }
         reader.ReadEndElement();
      }

      public void WriteXml(XmlWriter writer)
      {
         foreach (EnumerationItem item in Items)
         {
            item.WriteXml(writer);
         }
      }
   }
}