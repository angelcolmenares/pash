using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Eventing
{
   public class SubscriptionManager : EndpointReference
   {
      public SubscriptionManager(EndpointAddressBuilder existingAddressBuilder, string identifier, Uri toUri, string deliveryResourceUri)
      {
         existingAddressBuilder.Uri = toUri;
         existingAddressBuilder.Headers.Add(new IdentifierHeader(identifier));
         existingAddressBuilder.Headers.Add(new ResourceUriHeader(deliveryResourceUri));
         _address = existingAddressBuilder.ToEndpointAddress();
      }

      public SubscriptionManager(string identifier, Uri toUri, string deliveryResourceUri)
         : this(new EndpointAddressBuilder(), identifier, toUri, deliveryResourceUri )
      {         
      }

      public string Identifier
      {
         get
         {
            return IdentifierHeader.GetFrom(Address.Headers).Value;
         }
      }

      public string ResourceUri
      {
         get
         {
            return ResourceUriHeader.GetFrom(Address.Headers).ResourceUri;
         }
      }

      public SubscriptionManager()
      {
         
      }
   } 
}