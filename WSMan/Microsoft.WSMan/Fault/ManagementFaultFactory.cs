using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Fault
{
   public class ManagementFaultFactory : FaultFactory
   {
      public ManagementFaultFactory(string reason, string code) 
         : base(reason, code, ManagementNamespaces.Namespace, ManagementNamespaces.FaultAction)
      {
      }      
   }
}