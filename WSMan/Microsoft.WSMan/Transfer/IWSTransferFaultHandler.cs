using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel.Channels;

namespace Microsoft.WSMan.Transfer
{
   public interface IWSTransferFaultHandler
   {
      Exception HandleFault(Message faultMessage);
   }
}