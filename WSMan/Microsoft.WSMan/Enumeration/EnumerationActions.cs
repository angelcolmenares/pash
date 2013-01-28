using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Enumeration
{
   public class EnumerationActions
   {
      public const string Namespace = @"http://schemas.xmlsoap.org/ws/2004/09/enumeration";

      public const string FaultAction = @"http://schemas.xmlsoap.org/ws/2004/09/enumeration/fault";
      public const string EnumerateAction = @"http://schemas.xmlsoap.org/ws/2004/09/enumeration/Enumerate";
      public const string EnumerateResponseAction = @"http://schemas.xmlsoap.org/ws/2004/09/enumeration/EnumerateResponse";
      public const string PullAction = @"http://schemas.xmlsoap.org/ws/2004/09/enumeration/Pull";
      public const string PullResponseAction = @"http://schemas.xmlsoap.org/ws/2004/09/enumeration/PullResponse";
   }
}