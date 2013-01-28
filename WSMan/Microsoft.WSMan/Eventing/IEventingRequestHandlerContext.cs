using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WSMan.Enumeration;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Eventing
{
   public interface IEventingRequestHandlerContext
   {
      Filter Filter { get; }
      IEnumerable<Selector> Selectors { get; }
      void Push(object @event);
   }
}