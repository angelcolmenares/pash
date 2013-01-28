using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WSMan.Enumeration;

namespace Microsoft.WSMan.Eventing
{
   [MessageContract(IsWrapped = true, WrapperName = "SubscribeResponse", WrapperNamespace = EventingActions.Namespace)]
   public class SubscribeResponse
   {
      [MessageBodyMember(Order = 0)]
      [XmlElement(Namespace = EventingActions.Namespace)]
      public SubscriptionManager SubscriptionManager { get; set; }      

      [MessageBodyMember(Order = 1)]
      [XmlElement(Namespace = EventingActions.Namespace)]
      public Expires Expires { get; set; }

      [MessageBodyMember(Order = 2)]
      [XmlElement(Namespace = Enumeration.EnumerationActions.Namespace)]
      public EnumerationContextKey EnumerationContext { get; set; }

      [XmlAnyElement]
      public XmlElement[] Any { get; set; }

      [XmlAnyAttribute]
      public XmlAttribute[] AnyAttr { get; set; }
   }
}