using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.WSMan.Transfer;

namespace Microsoft.WSMan.Management
{
   public interface IManagementRequestHandler
   {
      bool CanHandle(string resourceUri);
      object HandleGet(string fragmentExpression, IEnumerable<Selector> selectors);
      object HandlePut(string fragmentExpression, IEnumerable<Selector> selectors, ExtractBodyDelegate extractBodyCallback);
      EndpointAddress HandleCreate(ExtractBodyDelegate extractBodyCallback);
      void HandlerDelete(IEnumerable<Selector> selectors);
   }
}