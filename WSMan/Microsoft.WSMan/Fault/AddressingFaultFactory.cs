using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.WSMan.Fault
{
   public class AddressingFaultFactory : FaultFactory
   {
      public AddressingFaultFactory(string reason, string code) 
         : base(reason, code, null, null)
      {
      }

      protected override bool CheckCodeNamespace(FaultCode code)
      {
         return code.Namespace == AddressingVersion.WSAddressing10.GetNamespace() ||
                code.Namespace == AddressingVersion.WSAddressingAugust2004.GetNamespace();
      } 
   }
}