using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace Microsoft.WSMan
{
   public class WSHttpBindingAugust2004 : WSHttpBinding
   {
      public WSHttpBindingAugust2004(SecurityMode securityMode)
         : base(securityMode)
      {
      }

      public override BindingElementCollection CreateBindingElements()
      {
         BindingElementCollection elements = base.CreateBindingElements();
         TextMessageEncodingBindingElement textEncoding = elements.Find<TextMessageEncodingBindingElement>();
         textEncoding.MessageVersion = MessageVersion.Soap12WSAddressingAugust2004;
         return elements;
      }
   }
}
