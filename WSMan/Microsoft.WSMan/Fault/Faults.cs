using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WSMan.Fault;

namespace Microsoft.WSMan
{
   public class Faults
   {
      public static readonly FaultFactory EndpointUnavailable = new AddressingFaultFactory(
         "The specified endpoint is currently unavailable.", "EndpointUnavailable");

      public static readonly FaultFactory DestinationUnreachable = new AddressingFaultFactory(
         "No route can be determined to reach the destination role defined by the WS-Addressing To.", "DestinationUnreachable");

      public static readonly FaultFactory TimedOut = new ManagementFaultFactory(
         "The operation has timed out.", "TimedOut");
      
      public static readonly FaultFactory InvalidEnumerationContext = new EnumerationFaultFactory(
         "The supplied enumeration context is invalid.", "InvalidEnumerationContext");
   }
}