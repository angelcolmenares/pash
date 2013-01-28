using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Eventing
{
   public interface IPullSubscriptionClient<T> : IDisposable
   {
      IEnumerable<T> Pull();
      IEnumerable<T> PullOnce();
   }
}