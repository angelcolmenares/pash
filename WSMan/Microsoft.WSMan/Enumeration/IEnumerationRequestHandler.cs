using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{
   public interface IEnumerationContext
   {
      string Context { get; }
      Filter Filter { get; }
      IEnumerable<Selector> Selectors { get; }
   }

   public interface IEnumerationRequestHandler
   {
      IEnumerable<object> Enumerate(IEnumerationContext context);
      int EstimateRemainingItemsCount(IEnumerationContext context);
   }
}
