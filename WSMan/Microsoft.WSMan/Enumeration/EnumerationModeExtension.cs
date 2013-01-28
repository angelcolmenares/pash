using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;

namespace Microsoft.WSMan.Enumeration
{
   public class EnumerationModeExtension : IExtension<OperationContext>
   {
      private readonly EnumerationMode _mode;
      private readonly Type _enumeratedType;

      private EnumerationModeExtension(EnumerationMode mode, Type enumeratedType)
      {
         _mode = mode;
         _enumeratedType = enumeratedType;
      }

      public static void Activate(EnumerationMode mode, Type enumeratedType)
      {
         OperationContextProxy.Current.AddExtension(new EnumerationModeExtension(mode, enumeratedType));
      }

      public static EnumerationMode CurrentEnumerationMode
      {
         get { return OperationContextProxy.Current.FindExtension<EnumerationModeExtension>()._mode; }
      }

      public static Type CurrentEnumeratedType
      {
         get { return OperationContextProxy.Current.FindExtension<EnumerationModeExtension>()._enumeratedType; }
      }

      public void Attach(OperationContext owner)
      {
      }

      public void Detach(OperationContext owner)
      {
      }
   }
}