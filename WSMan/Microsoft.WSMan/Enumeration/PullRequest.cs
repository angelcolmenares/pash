using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml.Serialization;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{   
   [MessageContract(IsWrapped = true, WrapperName = "Pull", WrapperNamespace = EnumerationActions.Namespace)]   
   public class PullRequest
   {
	  public PullRequest()
	  {

	  }

      [MessageBodyMember(Order = 0)]
      [XmlElement(Namespace = EnumerationActions.Namespace)]
      public EnumerationContextKey EnumerationContext { get; set; }

      [MessageBodyMember(Order = 1)]
      [XmlElement(Namespace = EnumerationActions.Namespace)]
      public MaxTime MaxTime { get; set; }

      [MessageBodyMember(Order = 2)]
      [XmlElement(Namespace = ManagementNamespaces.Namespace)]
      public MaxElements MaxElements { get; set; }      
   }
}