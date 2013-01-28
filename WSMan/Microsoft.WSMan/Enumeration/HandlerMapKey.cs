using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Enumeration
{
   public struct HandlerMapKey
   {
      private readonly string _resourceUri;
      private readonly string _filterDialect;

      public HandlerMapKey(string resourceUri, string filterDialect)
      {
         if (resourceUri == null)
         {
            throw new ArgumentNullException("resourceUri");
         }
         if (filterDialect == null)
         {
            throw new ArgumentException("filterDialect");
         }
         _resourceUri = resourceUri;
         _filterDialect = filterDialect;
      }

      public override bool Equals(object obj)
      {
         if (!(obj is HandlerMapKey))
         {
            return false;
         }
         HandlerMapKey other = (HandlerMapKey)obj;
         return _resourceUri.Equals(other._resourceUri) &&
            _filterDialect.Equals(other._filterDialect);
      }

      public override int GetHashCode()
      {
         return _filterDialect.GetHashCode() ^ _resourceUri.GetHashCode();
      }
   }
}