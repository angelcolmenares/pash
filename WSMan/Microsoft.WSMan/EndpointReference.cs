using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WSMan
{
   public class EndpointReference : IXmlSerializable
   {
      protected EndpointAddress _address;

      public EndpointReference()
      {
      }

      public EndpointReference(EndpointAddress address)
      {
         _address = address;
      }

      public EndpointReference(EndpointAddressBuilder addressBuilder)
      {
         _address = addressBuilder.ToEndpointAddress();
      }

      public EndpointAddress Address
      {
         get { return _address; }
      }

      #region IXmlSerializable Members
      public System.Xml.Schema.XmlSchema GetSchema()
      {
         return null;
      }
      public void ReadXml(XmlReader reader)
      {         
         _address = EndpointAddress.ReadFrom(AddressingVersionExtension.CurrentVersion, reader);
      }
      public void WriteXml(XmlWriter writer)
      {
         _address.WriteContentsTo(AddressingVersionExtension.CurrentVersion, writer);
      }
      #endregion
   }
}