using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.WSMan.Eventing
{
   public class EventBuffer
   {
      public const int DefaultSize = 100;

      public bool IsEmpty
      {
         get { return _storage.Count == 0; }
      }      
      
      public void Push(object @event)
      {         
         lock (_storage)
         {            
            _storage.Enqueue(@event);            
            if (_storage.Count > _maxSize)
            {
               _storage.Dequeue();
            }
            _event.Set();
         }         
      }
      public IEnumerable<object> FetchNotifications(int maxElements, TimeSpan maxTime)
      {
         if (_event.WaitOne(maxTime))
         {
            lock (_storage)
            {
               int i = 0;
               while (i < maxElements && _storage.Count > 0 )
               {
                  yield return _storage.Dequeue();
                  i++;
               }
               if (_storage.Count == 0)
               {
                  _event.Reset();
               }
            }
         }
      }

      public EventBuffer()
         : this(DefaultSize)
      {
      }
      public EventBuffer(int maxSize)
      {
         _maxSize = maxSize;
      }

      private readonly ManualResetEvent _event = new ManualResetEvent(false);
      private readonly Queue<object> _storage = new Queue<object>();
      private readonly int _maxSize;      
   }
}