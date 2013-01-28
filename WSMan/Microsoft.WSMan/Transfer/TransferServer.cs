using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.WSMan.Management;
using Microsoft.WSMan.Configuration;

namespace Microsoft.WSMan.Transfer
{
   [AddressingVersionExtensionServiceBehavior]
   public class TransferServer : IWSTransferContract
   {
      private readonly ITransferRequestHandler _handler;
      private readonly MessageFactory _factory;

      public TransferServer(ITransferRequestHandler handler)
      {
         _handler = handler;
         _factory = new MessageFactory();
      }

      public Message Get(Message getRequest)
      {
         object payload = _handler.HandleGet();
         return _factory.CreateGetResponse(payload);
      }

      public Message Put(Message putRequest)
      {
         object payload = _handler.HandlePut(x => _factory.DeserializeMessageWithPayload(putRequest, x));
         return _factory.CreatePutResponse(payload);
      }

      public Message Create(Message createRequest)
      {
         EndpointAddress address = _handler.HandleCreate(x => _factory.DeserializeMessageWithPayload(createRequest, x));
         return _factory.CreateCreateResponse(address);
      }

      public Message Delete(Message deleteRequest)
      {
         _handler.HandlerDelete();
         return _factory.CreateDeleteResponse();
      }
   }
}