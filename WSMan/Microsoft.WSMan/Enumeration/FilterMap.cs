using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Enumeration
{
   public class FilterMap
   {
      public const string DefaultDialect = @"http://www.w3.org/TR/1999/REC-xpath-19991116";

      private readonly Dictionary<string, Dialect> _mapping = new Dictionary<string, Dialect>();

      public void Bind(string dialect, Type filterObjectType)
      {
         _mapping[dialect] =  new Dialect(filterObjectType, null);         
      }      

      public Type GetFilterType(string dialectName)
      {
         Dialect dialect;
         if (_mapping.TryGetValue(dialectName, out dialect))
         {
            return dialect.FilterType;
         }
         return null;         
      }

      public Type GetEnumeratedObjectType(string dialectName)
      {
         Dialect dialect;
         if (_mapping.TryGetValue(dialectName, out dialect))
         {
            return dialect.EnumeratedType;
         }
         return null;   
      }
    
      private class Dialect
      {
         private readonly Type _filterType;
         private readonly Type _enumeratedType;

         public Dialect(Type filterType, Type enumeratedType)
         {
            _filterType = filterType;
            _enumeratedType = enumeratedType;
         }

         public Type FilterType
         {
            get { return _filterType; }
         }

         public Type EnumeratedType
         {
            get { return _enumeratedType; }
         }
      }      
   }
}