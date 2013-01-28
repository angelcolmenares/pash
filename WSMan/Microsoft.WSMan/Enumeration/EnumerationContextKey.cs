using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Enumeration
{
   [XmlType("EnumerationContext", Namespace = EnumerationActions.Namespace)]
   public class EnumerationContextKey
   {
      [XmlText]
      public string Text { get; set; }

      public EnumerationContextKey()
      {         
      }

      public EnumerationContextKey(string value)
      {
         Text = value;
      }

      public static EnumerationContextKey Unique()
      {
         return new EnumerationContextKey(Guid.NewGuid().ToString());
      }

      public override int GetHashCode()
      {
         return Text.GetHashCode();
      }

      public override bool Equals(object obj)
      {
         EnumerationContextKey other = obj as EnumerationContextKey;
         return other != null && Text.Equals(other.Text);
      }
   }
}