using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.WSMan.Transfer;

namespace Microsoft.WSMan.Management
{
   public class ManagementWSTransferFaultHandler : IWSTransferFaultHandler
   {
      public Exception HandleFault(Message faultMessage)
      {     
         //TODO: Check security rish with int.MaxValue
         return new FaultException(MessageFault.CreateFault(faultMessage, int.MaxValue));
      }
   }
}