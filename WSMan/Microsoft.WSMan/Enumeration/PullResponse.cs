using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Enumeration
{
   [MessageContract(IsWrapped = true, WrapperName = "PullResponse", WrapperNamespace = EnumerationActions.Namespace)]
   public class PullResponse
   {
      [MessageBodyMember(Order = 0)]
      [XmlElement(Namespace = EnumerationActions.Namespace)]
      public EnumerationContextKey EnumerationContext { get; set; }

      [MessageBodyMember(Order = 1)]
      [XmlElement(Namespace = EnumerationActions.Namespace)]      
      public EnumerationItemList Items { get; set; }

      [MessageBodyMember(Order = 2)]
      [XmlElement(Namespace = EnumerationActions.Namespace)]
      public EndOfSequence EndOfSequence { get; set; }
   }
}