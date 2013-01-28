using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.WSMan.Enumeration;

namespace Microsoft.WSMan.Eventing
{   
   public class EventingPullDeliveryServer : IWSEventingPullDeliveryContract
   {      
      public PullResponse Pull(PullRequest request)
      {                  
         //TODO: Check URI
         PullSubscription subsciption;
			if (request.EnumerationContext == null || !_subscriptions.TryGetValue(request.EnumerationContext.Text, out subsciption))
         {
            throw Faults.InvalidEnumerationContext.Create();
         }

         EnumerationModeExtension.Activate(EnumerationMode.EnumerateObjectAndEPR, subsciption.EventType);

         int maxElements = request.MaxElements != null
                              ? request.MaxElements.Value
                              : 1;

         TimeSpan maxTime = request.MaxTime != null 
            ? request.MaxTime.Value 
            : TimeSpan.FromSeconds(10);

         EnumerationItemList items = new EnumerationItemList(PullItems(subsciption.Buffer.FetchNotifications(maxElements, maxTime)));

         //R7.2.13-5
         if (items.Items.Count() == 0)
         {
            throw Faults.TimedOut.Create();
         }
         return new PullResponse
                   {
                      Items = items,
                      EndOfSequence = null,
                      EnumerationContext = request.EnumerationContext
                   };
      }

      private static IEnumerable<EnumerationItem> PullItems(IEnumerable<object> enumerable)
      {         
         return enumerable.Select(x => new EnumerationItem(
                                          new EndpointAddress("http://tempuri.org"),
                                          x));
      }

      public void AddSubscription(PullSubscription subscription)
      {
         _subscriptions[subscription.Identifier] = subscription;
      }

      public void RemoveSubscription(PullSubscription subscription)
      {
         _subscriptions.Remove(subscription.Identifier);
      }

      private readonly Dictionary<string, PullSubscription> _subscriptions = new Dictionary<string, PullSubscription>();
   }
}