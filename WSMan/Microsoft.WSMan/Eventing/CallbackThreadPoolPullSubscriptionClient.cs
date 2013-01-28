using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.WSMan.Eventing
{
   public class CallbackThreadPoolPullSubscriptionClient<T> : IDisposable
   {
      public CallbackThreadPoolPullSubscriptionClient(Action<T> callback, IPullSubscriptionClient<T> pullSubscriptionClient, bool synchronizeCallbackThread)
      {
         _callback = callback;
         if (synchronizeCallbackThread)
         {
            _completionEvent = new ManualResetEvent(false);
         }
         _pullSubscriptionClient = pullSubscriptionClient;

         ThreadPool.QueueUserWorkItem(WaitCallback, null);
      }

      private void WaitCallback(object state)
      {
         while (true)
         {
            if (_disposed)
            {
               _pullSubscriptionClient.Dispose();
               _completionEvent.Set();
               break;
            }
            IEnumerable<T> result = _pullSubscriptionClient.PullOnce();
            foreach (T item in result)
            {
               _callback(item);
            }
         }
      }

      public void Dispose()
      {
         if (_disposed)
         {            
            return;
         }
         _disposed = true;
         if (_completionEvent != null)
         {
            _completionEvent.WaitOne();
         }
      }

      private readonly Action<T> _callback;
      private readonly IPullSubscriptionClient<T> _pullSubscriptionClient;
      private readonly ManualResetEvent _completionEvent;
      private bool _disposed;
   }
}