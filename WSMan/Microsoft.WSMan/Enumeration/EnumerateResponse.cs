using System;

using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml.Serialization;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{   
   [MessageContract(IsWrapped = true, WrapperName = "EnumerateResponse", WrapperNamespace = EnumerationActions.Namespace)]
   public class EnumerateResponse
   {
      [MessageBodyMember(Order = 0)]
      [XmlElement(Namespace = EnumerationActions.Namespace)]
      public Expires Expires { get; set; }

      [MessageBodyMember(Order = 1)]
      [XmlElement(Namespace = EnumerationActions.Namespace)]
      public EnumerationContextKey EnumerationContext { get; set; }

      [MessageBodyMember(Order = 2)]
	  [XmlElement(Namespace = ManagementNamespaces.Namespace)]      
      public EnumerationItemList Items { get; set; }

      [MessageBodyMember(Order = 3)]
	  [XmlElement(Namespace = ManagementNamespaces.Namespace)]
      public EndOfSequence EndOfSequence { get; set; }
   }
}