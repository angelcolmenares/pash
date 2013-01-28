using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.Xml;

namespace Microsoft.WSMan.Management
{
   public class ResourceUriHeader : AddressHeader
   {
      private const string ElementName = "ResourceURI";

      private readonly string _resourceUri;

      public ResourceUriHeader(string resourceUri)
      {
         _resourceUri = resourceUri;
      }

      public static ResourceUriHeader GetFrom(AddressHeaderCollection headerCollection)
      {
		 return (ResourceUriHeader)headerCollection.FindHeader(ElementName, ManagementNamespaces.Namespace);
      }

      public static ResourceUriHeader ReadFrom(XmlDictionaryReader reader)
      {
		 reader.ReadStartElement(ElementName, ManagementNamespaces.Namespace);
         string result = reader.Value;
         reader.Read();
         reader.ReadEndElement();
         return new ResourceUriHeader(result);
      }

      public static ResourceUriHeader ReadFrom(Message message)
      {
         return ReadFrom(message.Headers);
      }

      public static ResourceUriHeader ReadFrom(MessageHeaders messageHeaders)
      {
         ResourceUriHeader result;
		 int index = messageHeaders.FindHeader(ElementName, ManagementNamespaces.Namespace);
         if (index < 0)
         {
            return null;
         }
         using (XmlDictionaryReader readerAtHeader = messageHeaders.GetReaderAtHeader(index))
         {
            result = ReadFrom(readerAtHeader);
         }
         return result;
      }

      protected override void OnWriteAddressHeaderContents(XmlDictionaryWriter writer)
      {
         writer.WriteValue(ResourceUri);
      }

      public override string Name
      {
         get { return ElementName; }
      }

      public override string Namespace
      {
		 get { return ManagementNamespaces.Namespace; }
      }

      public string ResourceUri
      {
         get { return _resourceUri; }
      }
   }
}