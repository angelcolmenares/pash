using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Microsoft.WSMan.Enumeration;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Eventing
{
   [FilterMapExtensionServiceBehavior]
   [AddressingVersionExtensionServiceBehavior]
   public class EventingServer : 
      IWSEventingContract, 
      IWSEventingPullDeliveryContract,
      IFilterMapProvider
   {
      public void BindWithPullDelivery(
         Uri listeningResourceUri, 
         string dialect, 
         Type filterType,
         IEventingRequestHandler eventSource,
         Uri deliveryResourceUri         
         )
      {
         PullDeliverySubscriptionManager enumHandler = new PullDeliverySubscriptionManager(deliveryResourceUri.ToString(), _pullDeliveryServer, eventSource);
         _filterMap.Bind(dialect, filterType);
         _enumHandlers[new HandlerMapKey(listeningResourceUri.ToString(), dialect)] = enumHandler;
      }

      public SubscribeResponse Subscribe(SubscribeRequest request)
      {
         //Check
         SelectorSetHeader selectorSetHeader = OperationContextProxy.Current.FindHeader<SelectorSetHeader>();         
         //Check
         ResourceUriHeader resourceUriHeader = OperationContextProxy.Current.FindHeader<ResourceUriHeader>();            

         return Subscribe(resourceUriHeader.ResourceUri, selectorSetHeader != null ? selectorSetHeader.Selectors : (IEnumerable<Selector>)new Selector[] { }, request);
      }

      public SubscribeResponse Subscribe(string resourceUri, IEnumerable<Selector> selectors, SubscribeRequest request)
      {
         EndpointAddressBuilder susbcriptionManagerEndpointAddress = new EndpointAddressBuilder();

         Expires expiration = request.Expires ?? Expires.FromTimeSpan(DefaultExpirationTime);

         Subsciption subsciption = GetManager(resourceUri, request.Filter).Subscribe(
            request.Filter,
            selectors,
            expiration,
            susbcriptionManagerEndpointAddress);
         
         lock (_activeSubscriptions)
         {            
            _activeSubscriptions[subsciption.Identifier] = subsciption;            
         }
         //R7.2.4-1
         return new SubscribeResponse
                   {                      
                      SubscriptionManager = new SubscriptionManager(susbcriptionManagerEndpointAddress, subsciption.Identifier, OperationContextProxy.Current.LocalAddress, subsciption.DeliveryResourceUri),                      
                      EnumerationContext = request.Delivery.Mode == Delivery.DeliveryModePull 
                         ? new EnumerationContextKey(subsciption.Identifier) 
                         : null,
                      Expires = expiration
                   };
      }

      public void Unsubscribe(UnsubscribeRequest request)
      {
         IdentifierHeader identifierHeader = OperationContextProxy.Current.FindHeader<IdentifierHeader>();            
         
         lock (_activeSubscriptions)
         {
            Subsciption toRemove;
            if (_activeSubscriptions.TryGetValue(identifierHeader.Value, out toRemove))
            {
               toRemove.Dispose();
               _activeSubscriptions.Remove(identifierHeader.Value);
            }            
         }         
      }

      public RenewResponse Renew(RenewRequest request)
      {
         IdentifierHeader identifierHeader = OperationContextProxy.Current.FindHeader<IdentifierHeader>();            

         lock (_activeSubscriptions)
         {
            Subsciption toRenew;
            if (_activeSubscriptions.TryGetValue(identifierHeader.Value, out toRenew))
            {
               toRenew.Renew(request.Expires ?? Expires.FromTimeSpan(DefaultExpirationTime));               
            }
         }
         return new RenewResponse
                   {
                      Expires = request.Expires
                   };
      }

      public PullResponse Pull(PullRequest request)
      {
         //TODO: Check expiration and fault if expired.
         return _pullDeliveryServer.Pull(request);
      }

      private ISubscriptionManager GetManager(string resourceUri, Filter filter)
      {
         string dialect = (filter != null && filter.Dialect != null)
            ? filter.Dialect
            : FilterMap.DefaultDialect;

         //TODO: Fault is no existing
         return _enumHandlers[new HandlerMapKey(resourceUri, dialect)];
      }

      public FilterMap ProvideFilterMap()
      {
         return _filterMap;
      }

      public EventingServer(EventingPullDeliveryServer pullDeliveryServer)
      {
         _pullDeliveryServer = pullDeliveryServer;
         DefaultExpirationTime = TimeSpan.FromHours(1);
      }

      public EventingServer() : this(new EventingPullDeliveryServer())         
      {         
      }

      public TimeSpan DefaultExpirationTime { get; set; }

      private readonly FilterMap _filterMap = new FilterMap();
      private readonly Dictionary<string, Subsciption> _activeSubscriptions = new Dictionary<string, Subsciption>();
      private readonly Dictionary<HandlerMapKey, ISubscriptionManager> _enumHandlers = new Dictionary<HandlerMapKey, ISubscriptionManager>();
      private readonly EventingPullDeliveryServer _pullDeliveryServer;
   }
}