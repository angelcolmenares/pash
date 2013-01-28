using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Enumeration
{
   public class EnumerationState
   {
      private readonly IEnumerator<object> _enumerator;
      private readonly EnumerationMode _mode;
      
      public EnumerationState(IEnumerator<object> enumerator, EnumerationMode mode)
      {
         _enumerator = enumerator;
         _mode = mode;
      }

      public IEnumerator<object> Enumerator
      {
         get { return _enumerator; }
      }

      public EnumerationMode Mode
      {
         get { return _mode; }
      }
   }
}