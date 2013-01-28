using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.WSMan
{
   public class AddressingVersionExtension : IExtension<OperationContext>
   {
      private readonly AddressingVersion _version;

      private AddressingVersionExtension(AddressingVersion version)
      {
         _version = version;
      }

      public static void Activate(AddressingVersion version)
      {
         OperationContextProxy.Current.AddExtension(new AddressingVersionExtension(version));
      }

      public static AddressingVersion CurrentVersion
      {
         get 
		 {
				var ext = OperationContextProxy.Current.FindExtension<AddressingVersionExtension>();
				if (ext == null) return AddressingVersion.WSAddressing10;
				return ext._version; 
		 }
      }

      public void Attach(OperationContext owner)
      {
         
      }

      public void Detach(OperationContext owner)
      {         
      }
   }
}