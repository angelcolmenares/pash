using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WSMan.Enumeration;

namespace Microsoft.WSMan.Eventing
{
   [MessageContract(IsWrapped = true, WrapperName = "RenewResponse", WrapperNamespace = EventingActions.Namespace)]
   public class RenewResponse
   {
      [MessageBodyMember(Order = 0)]
      [XmlElement]
      public Expires Expires { get; set; }

      [MessageBodyMember(Order = 1)]
      [XmlAnyElement]
      public XmlElement[] Any { get; set; }

      [MessageBodyMember(Order = 2)]
      [XmlAnyAttribute]
      public XmlAttribute[] AnyAttr { get; set; }
   }
}