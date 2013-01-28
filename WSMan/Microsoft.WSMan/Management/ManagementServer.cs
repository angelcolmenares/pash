using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.WSMan.Enumeration;
using Microsoft.WSMan.Eventing;
using Microsoft.WSMan.Transfer;
using RenewRequest=Microsoft.WSMan.Eventing.RenewRequest;
using RenewResponse=Microsoft.WSMan.Eventing.RenewResponse;
using Microsoft.WSMan.Configuration;

namespace Microsoft.WSMan.Management
{
   public delegate PullResponse PullDelegate(PullRequest request);

   public delegate void BindManagementDelegate(Uri resourceUri, IManagementRequestHandler managementRequestHandler);
  
   public delegate  void BindEnumerationDelegate(Uri resoureceUri, string dialect, Type filterType, IEnumerationRequestHandler enumerationRequestHandler);

	public delegate void BindPullEventingDelegate(Uri resourceUri, string dialect, Type filterType, IEventingRequestHandler eventingRequestHandler, Uri deliveryResourceUri);

   [FilterMapExtensionServiceBehavior]
   [AddressingVersionExtensionServiceBehavior]
   public class ManagementServer :
      IWSTransferContract,
      IWSEnumerationContract,
      IWSEventingContract,
      IFilterMapProvider
   {
		#region IChannel implementation

		public T GetProperty<T> () where T : class
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region ICommunicationObject implementation

		public void Abort ()
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginClose (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginClose (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginOpen (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginOpen (TimeSpan timeout, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		public void Close ()
		{
			throw new NotImplementedException ();
		}

		public void Close (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public void EndClose (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public void EndOpen (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public void Open ()
		{
			throw new NotImplementedException ();
		}

		public void Open (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		#endregion

      Message IWSTransferContract.Get(Message getRequest)
      {
         return _transferServer.Get(getRequest);
      }

      Message IWSTransferContract.Put(Message putRequest)
      {
         return _transferServer.Put(putRequest);
      }

      Message IWSTransferContract.Create(Message createRequest)
      {
         return _transferServer.Create(createRequest);
      }

      Message IWSTransferContract.Delete(Message deleteRequest)
      {
         return _transferServer.Delete(deleteRequest);
      }

      EnumerateResponse IWSEnumerationContract.Enumerate(EnumerateRequest request)
      {
         return _enumerationServer.Enumerate(request);
      }

      PullResponse IWSEnumerationContract.Pull(PullRequest request)
      {
         ResourceUriHeader resourceUriHeader = OperationContextProxy.Current.FindHeader<ResourceUriHeader>();
            
         //TODO: Fault
         PullDelegate handler = _pullRoutingTable[resourceUriHeader.ResourceUri];
         return handler(request);
      }

      SubscribeResponse IWSEventingContract.Subscribe(SubscribeRequest request)
      {
         return _eventingServer.Subscribe(request);
      }

      void IWSEventingContract.Unsubscribe(UnsubscribeRequest request)
      {
         _eventingServer.Unsubscribe(request);
      }

      public RenewResponse Renew(RenewRequest request)
      {
         return _eventingServer.Renew(request);
      }

      FilterMap IFilterMapProvider.ProvideFilterMap()
      {
         return _filterMap;
      }

	  public static ManagementServer Create ()
	  {
			var server = new ManagementServer();
			WSManConfigurationFactory.Bind(server.BindManagement, server.BindEnumeration, server.BindPullEventing);
			return server;
	  }

      public ManagementServer()
      {
         _managementHandler = new ManagementTransferRequestHandler();
         _transferServer = new TransferServer(_managementHandler);
         _enumerationServer = new EnumerationServer();
         _pullDeliveryServer = new EventingPullDeliveryServer();
         _eventingServer = new EventingServer(_pullDeliveryServer);         
      }

      public void BindManagement(Uri resourceUri, IManagementRequestHandler managementRequestHandler)
      {
         _managementHandler.Bind(resourceUri, managementRequestHandler);
      }

      public void BindEnumeration(Uri resoureceUri, string dialect, Type filterType, IEnumerationRequestHandler enumerationRequestHandler)
      {
         _filterMap.Bind(dialect, filterType);
         _pullRoutingTable[resoureceUri.ToString()] = _enumerationServer.Pull;
         _enumerationServer.Bind(resoureceUri, dialect, filterType, enumerationRequestHandler);
      }

      public void BindPullEventing(Uri resourceUri, string dialect, Type filterType, IEventingRequestHandler eventingRequestHandler, Uri deliveryResourceUri)
      {
         _filterMap.Bind(dialect, filterType);
         _pullRoutingTable[deliveryResourceUri.ToString()] = _pullDeliveryServer.Pull;
         _eventingServer.BindWithPullDelivery(resourceUri, dialect, filterType, eventingRequestHandler, deliveryResourceUri);
      }      
      
      private readonly Dictionary<string, PullDelegate> _pullRoutingTable = new Dictionary<string, PullDelegate>();
      private readonly TransferServer _transferServer;      
      private readonly EventingServer _eventingServer;
      private readonly EnumerationServer _enumerationServer;
      private readonly EventingPullDeliveryServer _pullDeliveryServer;
      private readonly ManagementTransferRequestHandler _managementHandler;
      private readonly FilterMap _filterMap = new FilterMap();     
   }
}