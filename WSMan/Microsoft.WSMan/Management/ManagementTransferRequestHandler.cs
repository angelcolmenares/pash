using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.WSMan.Transfer;

namespace Microsoft.WSMan.Management
{
   public class ManagementTransferRequestHandler : ITransferRequestHandler
   {            
      public object HandleGet()
      {
         FragmentTransferHeader fragmentTransferHeader =
            OperationContextProxy.Current.FindHeader<FragmentTransferHeader>();

         OperationContextProxy.Current.AddHeader(fragmentTransferHeader);         

         return GetHandler().HandleGet(fragmentTransferHeader.Expression, GetSelectors());
      }     

      public object HandlePut(ExtractBodyDelegate extractBodyCallback)
      {
         FragmentTransferHeader fragmentTransferHeader =
            OperationContextProxy.Current.FindHeader<FragmentTransferHeader>();

         OperationContextProxy.Current.AddHeader(fragmentTransferHeader);         

         return GetHandler().HandlePut(fragmentTransferHeader.Expression, GetSelectors(), extractBodyCallback);
      }

      public EndpointAddress HandleCreate(ExtractBodyDelegate extractBodyCallback)
      {
         return GetHandler().HandleCreate(extractBodyCallback);
      }

      public void HandlerDelete()
      {         
         GetHandler().HandlerDelete(GetSelectors());
      }

      public void Bind(Uri resourceUri, IManagementRequestHandler handler)
      {
         _handlers[resourceUri.ToString()] = handler;
      }

      private IManagementRequestHandler GetHandler()
      {
         ResourceUriHeader resourceUriHeader = OperationContextProxy.Current.FindHeader<ResourceUriHeader>();
         
         //TODO: Fault
         return _handlers[resourceUriHeader.ResourceUri];
      }


      private static List<Selector> GetSelectors()
      {
         SelectorSetHeader selectorSetHeader = OperationContextProxy.Current.FindHeader<SelectorSetHeader>();         

         List<Selector> selectors = selectorSetHeader != null 
            ? selectorSetHeader.Selectors 
            : new List<Selector>();
         return selectors;
      }

      private readonly Dictionary<string, IManagementRequestHandler> _handlers = new Dictionary<string, IManagementRequestHandler>();
   }
}