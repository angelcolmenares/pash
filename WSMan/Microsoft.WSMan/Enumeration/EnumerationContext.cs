using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{
   public class EnumerationContext : IEnumerationContext
   {
      private readonly string _context;
      private readonly Filter _filter;
      private readonly IEnumerable<Selector> _selectors;

      public EnumerationContext(string context, Filter filter, IEnumerable<Selector> selectors)
      {
         _context = context;
         _selectors = selectors;
         _filter = filter;
      }

      public string Context
      {
         get { return _context; }
      }

      public Filter Filter
      {
         get { return _filter; }
      }

      public IEnumerable<Selector> Selectors
      {
         get { return _selectors; }
      }
   }
}