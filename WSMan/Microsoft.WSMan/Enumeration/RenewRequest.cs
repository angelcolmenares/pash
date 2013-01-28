using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Enumeration
{
   [Serializable]
   [XmlType(AnonymousType = true, Namespace = EnumerationActions.Namespace)]
   [XmlRoot("Renew", Namespace = EnumerationActions.Namespace, IsNullable = false)]
   public class RenewRequest
   {
      private XmlAttribute[] anyAttrField;
      private XmlElement[] anyField;
      private EnumerationContextKey enumerationContextField;

      private string expiresField;


      public EnumerationContextKey EnumerationContext
      {
         get { return enumerationContextField; }
         set { enumerationContextField = value; }
      }


      public string Expires
      {
         get { return expiresField; }
         set { expiresField = value; }
      }


      [XmlAnyElement]
      public XmlElement[] Any
      {
         get { return anyField; }
         set { anyField = value; }
      }


      [XmlAnyAttribute]
      public XmlAttribute[] AnyAttr
      {
         get { return anyAttrField; }
         set { anyAttrField = value; }
      }
   }
}