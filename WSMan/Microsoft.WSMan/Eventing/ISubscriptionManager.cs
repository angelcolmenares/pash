using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.WSMan.Enumeration;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Eventing
{
   public interface ISubscriptionManager
   {
      Subsciption Subscribe(Filter filter, IEnumerable<Selector> selectors, Expires expires, EndpointAddressBuilder susbcriptionManagerEndpointAddress);
      void Unsubscribe(Subsciption subsciption);
   }
}