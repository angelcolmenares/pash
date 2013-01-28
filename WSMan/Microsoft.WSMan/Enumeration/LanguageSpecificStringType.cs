using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Enumeration
{
   
   [Serializable]
   
   
   [XmlType(Namespace = EnumerationActions.Namespace)]
   public class LanguageSpecificString
   {
      private XmlAttribute[] anyAttrField;
      private string langField;

      private string valueField;


      [XmlAttribute(Form = XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
      public string lang
      {
         get { return langField; }
         set { langField = value; }
      }


      [XmlAnyAttribute]
      public XmlAttribute[] AnyAttr
      {
         get { return anyAttrField; }
         set { anyAttrField = value; }
      }


      [XmlText]
      public string Value
      {
         get { return valueField; }
         set { valueField = value; }
      }
   }
}