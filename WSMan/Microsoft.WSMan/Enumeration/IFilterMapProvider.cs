using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Enumeration
{
   public interface IFilterMapProvider
   {
      FilterMap ProvideFilterMap();
   }
}