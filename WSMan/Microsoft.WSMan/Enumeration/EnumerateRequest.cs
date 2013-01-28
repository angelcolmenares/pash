using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml.Serialization;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{
   [MessageContract(IsWrapped = true, WrapperName = "Enumerate", WrapperNamespace = EnumerationActions.Namespace)]   
   public class EnumerateRequest
   {
      [MessageBodyMember(Order = 0)]      
      [XmlElement(Namespace = EnumerationActions.Namespace)]
      public EndpointAddress10 EndTo { get; set; }

      [MessageBodyMember(Order = 1)]      
      [XmlElement(Namespace = ManagementNamespaces.Namespace)]
      public Filter Filter { get; set; }

      [MessageBodyMember(Order = 2)]
      [XmlElement(Namespace = EnumerationActions.Namespace)]
      public Expires Expires { get; set; }      

      [MessageBodyMember(Order = 3)]      
      [XmlElement(Namespace = ManagementNamespaces.Namespace)]
      public EnumerationMode EnumerationMode { get; set; }

      [MessageBodyMember(Order = 4)]
      [XmlElement(Namespace = ManagementNamespaces.Namespace)]
      public OptimizeEnumeration OptimizeEnumeration { get; set; }

      [MessageBodyMember(Order = 5)]
      [XmlElement(Namespace = ManagementNamespaces.Namespace)]
      public MaxElements MaxElements { get; set; }      

   }
}