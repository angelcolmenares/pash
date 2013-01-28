using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WSMan.Enumeration;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Eventing
{
   public abstract class Subsciption : IEventingRequestHandlerContext, IDisposable
   {
      public string Identifier
      {
         get { return _identifier; }
      }

      public Filter Filter
      {
         get { return _filter; }
      }

      public IEnumerable<Selector> Selectors
      {
         get { return _selectors; }
      }

      public string DeliveryResourceUri
      {
         get { return _deliveryResourceUri; }
      }

      public Type EventType
      {
         get { return _eventType; }
      }

      public DateTime ExpirationDate
      {
         get { return _expirationDate; }
      }

      public abstract void Push(object @event);

      public void Renew(Expires expires)
      {         
         if (expires.Value is DateTime)
         {
            _expirationDate = (DateTime) expires.Value;
         }
         else
         {
			if (expires.Value == null) expires = Expires.FromTimeSpan (new TimeSpan(1,0,0));
            _expirationDate = DateTime.Now + (TimeSpan) expires.Value;
         }
      }
      
      public void Unsubscribe()
      {
         _manager.Unsubscribe(this);
      }

      protected Subsciption(string identifier, string deliveryResourceUri, Type eventType, Filter filter, Expires expires, IEnumerable<Selector> selectors, ISubscriptionManager manager)
      {
         _identifier = identifier;
         _deliveryResourceUri = deliveryResourceUri;
         _eventType = eventType;
         _filter = filter;
         _selectors = selectors;
         _manager = manager;
         Renew(expires);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (_disposed)
         {
            return;
         }
         if (disposing)
         {       
            Unsubscribe();
         }
         _disposed = true;
      }

      public void Dispose()
      {
         Dispose(true);
      }

      private bool _disposed;
      private readonly string _identifier;
      private readonly string _deliveryResourceUri;
      private readonly Type _eventType;
      private DateTime _expirationDate;
      private readonly Filter _filter;
      private readonly IEnumerable<Selector> _selectors;
      private readonly ISubscriptionManager _manager;      
   }
}