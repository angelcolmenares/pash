using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.WSMan.Enumeration;

namespace Microsoft.WSMan.Eventing
{
   [MessageContract(IsWrapped = true, WrapperName = "Subscribe", WrapperNamespace = EventingActions.Namespace)]   
   public class SubscribeRequest
   {
      [MessageBodyMember(Order = 0)]
	  [XmlElement(Namespace = EventingActions.Namespace)]
      public EndpointReference EndTo { get; set; }
      
		[MessageBodyMember(Order = 1)]
	  [XmlElement(Namespace = EventingActions.Namespace, Type = typeof(Expires))]
      public Expires Expires { get; set; }

      	[MessageBodyMember(Order = 2)]
		[XmlElement(Namespace = EventingActions.Namespace, Type  = typeof(Filter))]
      	public Filter Filter { get; set; }

		
		[MessageBodyMember(Order = 3)]
		[XmlElement(Namespace = EventingActions.Namespace, Type = typeof(Delivery))]
		public Delivery Delivery { get; set; }

      [XmlAnyElement]
      public XmlElement[] Any { get; set; }

	  //[MessageBodyMember(Order = 5)]
      [XmlAnyAttribute]
      public XmlAttribute[] AnyAttr { get; set; }
   }
}