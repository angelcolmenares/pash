using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Fault
{
   public class EnumerationFaultFactory : FaultFactory
   {
      public EnumerationFaultFactory(string reason, string code)
         : base(reason, code, Enumeration.EnumerationActions.Namespace, Enumeration.EnumerationActions.FaultAction)
      {
      }
   }
}