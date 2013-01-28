using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.WSMan.Transfer
{
   public class CreateRsponseBodyWriter : BodyWriter
   {
      private readonly EndpointAddress _body;
      private readonly AddressingVersion _version;

      public CreateRsponseBodyWriter(EndpointAddress body, AddressingVersion version)
         : base(false)
      {
         _body = body;
         _version = version;
      }

      protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
      {
         writer.WriteStartElement(TransferActions.CreateResponse_ResourceCreatedElement);         
         _body.WriteContentsTo(_version, writer);
         writer.WriteEndElement();
      }
   }
}