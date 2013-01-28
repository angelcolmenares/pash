using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.WSMan.Transfer
{
   public delegate void HeaderCreatorDelegate(MessageHeaders headers);

   public delegate void AddressHeaderCreatorDelegate(Collection<AddressHeader> addressHeaders);

   public class TransferClient
   {
      private readonly Uri _endpointUri;
      private readonly IChannelFactory<IWSTransferContract> _proxyFactory;
      private readonly MessageFactory _factory;
      private readonly AddressingVersion _addressingVersion;
      private readonly IWSTransferFaultHandler _faultHandler;

      public TransferClient(Uri endpointUri, 
         IChannelFactory<IWSTransferContract> proxyFactory, 
         MessageVersion version, IWSTransferFaultHandler faultHandler)
      {
         _endpointUri = endpointUri;
         _faultHandler = faultHandler;
         _proxyFactory = proxyFactory;
         _factory = new MessageFactory(version);
         _addressingVersion = version.Addressing;
      }

      public T Get<T>(AddressHeaderCreatorDelegate addressHeaderCreatorDelegate, HeaderCreatorDelegate headerCreatorCallback)
      {
         using (ClientContext<IWSTransferContract> ctx = new ClientContext<IWSTransferContract>(_endpointUri, _addressingVersion, _proxyFactory, addressHeaderCreatorDelegate))
         {
            headerCreatorCallback(OperationContext.Current.OutgoingMessageHeaders);
			var message = _factory.CreateGetRequest();
			Message response = ctx.Channel.Get(message);
            if (response.IsFault)
            {
               throw _faultHandler.HandleFault(response);
            }
            return (T)_factory.DeserializeMessageWithPayload(response, typeof(T));
         }
      }

      public T Put<T>(AddressHeaderCreatorDelegate addressHeaderCreatorDelegate, HeaderCreatorDelegate headerCreatorCallback, object payload)
      {
         using (ClientContext<IWSTransferContract> ctx = new ClientContext<IWSTransferContract>(_endpointUri, _addressingVersion, _proxyFactory, addressHeaderCreatorDelegate))
         {
            headerCreatorCallback(OperationContext.Current.OutgoingMessageHeaders);
            Message response = ctx.Channel.Put(_factory.CreatePutRequest(payload));
            if (response.IsFault)
            {
               throw _faultHandler.HandleFault(response);
            }
            return (T)_factory.DeserializeMessageWithPayload(response, typeof(T));
         }
      }

      public EndpointAddress Create(AddressHeaderCreatorDelegate addressHeaderCreatorDelegate, HeaderCreatorDelegate headerCreatorCallback, object payload)
      {
         using (ClientContext<IWSTransferContract> ctx = new ClientContext<IWSTransferContract>(_endpointUri, _addressingVersion, _proxyFactory, addressHeaderCreatorDelegate))
         {
            headerCreatorCallback(OperationContext.Current.OutgoingMessageHeaders);
            Message response = ctx.Channel.Create(_factory.CreateCreateRequest(payload));
            if (response.IsFault)
            {
               throw _faultHandler.HandleFault(response);
            }
            return _factory.DeserializeCreateResponse(response);
         }
      }

      public void Delete(AddressHeaderCreatorDelegate addressHeaderCreatorDelegate, HeaderCreatorDelegate headerCreatorCallback)
      {
         using (ClientContext<IWSTransferContract> ctx = new ClientContext<IWSTransferContract>(_endpointUri, _addressingVersion, _proxyFactory, addressHeaderCreatorDelegate))
         {
            headerCreatorCallback(OperationContext.Current.OutgoingMessageHeaders);
            Message response = ctx.Channel.Delete(_factory.CreateDeleteRequest());
            if (response.IsFault)
            {
               throw _faultHandler.HandleFault(response);
            }
         }
      }      
   }
}