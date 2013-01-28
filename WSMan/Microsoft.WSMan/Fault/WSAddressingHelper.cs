using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel.Channels;

namespace Microsoft.WSMan.Fault
{
   internal static class WSAddressingHelper
   {
      public static string GetFaultAction(this AddressingVersion version)
      {
         return (string)typeof(AddressingVersion).GetProperty("FaultAction", BindingFlags.NonPublic | BindingFlags.Instance)
                           .GetValue(version, new object[] { });
      }
      public static string GetFaultAction()
      {
         return GetFaultAction(AddressingVersionExtension.CurrentVersion);
      }
      public static string GetNamespace(this AddressingVersion version)
      {
         return (string)typeof(AddressingVersion).GetProperty("Namespace", BindingFlags.NonPublic | BindingFlags.Instance)
                           .GetValue(version, new object[] { });
      }
      public static string GetNamespace()
      {
         return GetNamespace(AddressingVersionExtension.CurrentVersion);
      }
   }
}