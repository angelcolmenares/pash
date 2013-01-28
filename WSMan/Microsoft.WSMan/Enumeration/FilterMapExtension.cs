using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;

namespace Microsoft.WSMan.Enumeration
{
   public class FilterMapExtension : IExtension<OperationContext>
   {
      private readonly FilterMap _map;

      public static void Activate(FilterMap map)
      {
         OperationContextProxy.Current.AddExtension(new FilterMapExtension(map));
      }

      public static Type GetDialectType(string dialect)
      {
         return OperationContextProxy.Current.FindExtension<FilterMapExtension>()._map.GetFilterType(dialect);
      }

      public static Type GetEnumeratedObjectType(string dialect)
      {
         return OperationContextProxy.Current.FindExtension<FilterMapExtension>()._map.GetEnumeratedObjectType(dialect);
      }

      private FilterMapExtension(FilterMap map)
      {
         _map = map;
      }

      public void Attach(OperationContext owner)
      {
			var headers = owner.IncomingMessageHeaders;
      }

      public void Detach(OperationContext owner)
      {
			var headers = owner.IncomingMessageHeaders;
      }
   }
}