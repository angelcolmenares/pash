using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.WSMan.Fault;

namespace Microsoft.WSMan
{
   public static class FaultFactoryExtensions
   {
      public static bool IsA(this FaultException exception, FaultFactory factory)
      {
         return factory.Check(exception);
      }      
   }
}