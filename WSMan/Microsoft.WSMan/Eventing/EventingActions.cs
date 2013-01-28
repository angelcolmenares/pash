using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Eventing
{
   public class EventingActions
   {
      public const string Namespace = @"http://schemas.xmlsoap.org/ws/2004/08/eventing";

      public const string SubscribeAction = @"http://schemas.xmlsoap.org/ws/2004/08/eventing/Subscribe";
      public const string SubscribeResponseAction = @"http://schemas.xmlsoap.org/ws/2004/08/eventing/SubscribeResponse";

      public const string RenewAction = @"http://schemas.xmlsoap.org/ws/2004/08/eventing/Renew";
      public const string RenewResponseAction = @"http://schemas.xmlsoap.org/ws/2004/08/eventing/RenewResponse";

      public const string UnsubscribeAction = @"http://schemas.xmlsoap.org/ws/2004/08/eventing/Unsubscribe";
   }
}