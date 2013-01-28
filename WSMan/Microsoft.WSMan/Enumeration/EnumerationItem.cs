using System;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.WSMan.Management;
using System.ServiceModel.Channels;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.WSMan.Enumeration
{
   public class EnumerationItem: IXmlSerializable
   {
      private object _objectValue;
      private EndpointAddress _eprValue;

      public object ObjectValue
      {
         get { return _objectValue; }
      }

      public EndpointAddress EprValue
      {
         get { return _eprValue; }
      }

      public EnumerationItem()
      {         
      }

      public EnumerationItem(EndpointAddress epr, object value)
      {
         _eprValue = epr;
         _objectValue = value;
      }

      public EnumerationItem(EndpointAddress epr)
      {
         _eprValue = epr;
      }

      public XmlSchema GetSchema()
      {
         return null;
      }

      public void ReadXml(XmlReader reader)
      {         
         if (EnumerationModeExtension.CurrentEnumerationMode == EnumerationMode.EnumerateObjectAndEPR)
         {
            reader.ReadStartElement("Item", ManagementNamespaces.Namespace);
            XmlSerializer serializer = new XmlSerializer(EnumerationModeExtension.CurrentEnumeratedType);
            _objectValue = serializer.Deserialize(reader);            
         }
		_eprValue = ReadFrom(AddressingVersion.WSAddressing10, reader);         
         if (EnumerationModeExtension.CurrentEnumerationMode == EnumerationMode.EnumerateObjectAndEPR)
         {
            reader.ReadEndElement();
         }
      }

		private static EndpointAddress ReadFrom (System.ServiceModel.Channels.AddressingVersion addressingVersion, XmlReader xreader)
		{
			string xml = xreader.ReadOuterXml ();
			StringReader sreader = new StringReader (xml);
			XmlReader reader = XmlReader.Create (sreader);
			Uri uri = null;
			EndpointIdentity identity = null;
			reader.MoveToContent ();
			List<AddressHeader> header = new List<AddressHeader> ();
			while (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement) {
				if (reader.LocalName == "EndpointReference") {
					reader.Read ();
				} else if (reader.LocalName == "Address" && 
					reader.NodeType == XmlNodeType.Element &&
					!reader.IsEmptyElement)
					uri = new Uri (reader.ReadElementContentAsString ());
				else if (reader.LocalName == "Identity" && 
					reader.NodeType == XmlNodeType.Element &&
					!reader.IsEmptyElement) {
					//How can key re-empact Identity
					identity = new X509CertificateEndpointIdentity (new System.Security.Cryptography.X509Certificates.X509Certificate2 ("powershell.pfx", "mono"));
					break;
				} else {
					var headerName = reader.LocalName;
					var headerNamespace = reader.NamespaceURI;
					reader.MoveToContent ();
					reader.ReadStartElement ();
					var obj = reader.ReadElementContentAs (GetTypeFromLocalName (reader.LocalName), new XmlNamespaceManager (reader.NameTable));
					header.Add (AddressHeader.CreateAddressHeader (headerName, headerNamespace, obj));
					reader.MoveToContent ();
					reader.ReadEndElement ();
				}
			}
			return identity == null ? new EndpointAddress(uri, header.ToArray ()) : new EndpointAddress(uri, identity, header.ToArray ());
	  }

	  private static Type GetTypeFromLocalName (string localName)
		{
			if (localName.Equals ("string")) return typeof(string);
			if (localName.Equals ("boolean")) return typeof(bool);
			if (localName.Equals ("long")) return typeof(long);
			if (localName.Equals ("int")) return typeof(int);
			if (localName.Equals ("double")) return typeof(bool);
			if (localName.Equals ("short")) return typeof(short);
			else return typeof(object);
		}

      public void WriteXml(XmlWriter writer)
      {
         if (EnumerationModeExtension.CurrentEnumerationMode == EnumerationMode.EnumerateObjectAndEPR)
         {
            writer.WriteStartElement("Item", ManagementNamespaces.Namespace);
            XmlSerializer serializer = new XmlSerializer(_objectValue.GetType());
            serializer.Serialize(writer, _objectValue);
         }
         _eprValue.WriteTo(AddressingVersionExtension.CurrentVersion, writer);
         if (EnumerationModeExtension.CurrentEnumerationMode == EnumerationMode.EnumerateObjectAndEPR)
         {
            writer.WriteEndElement();
         }
      }
   }
}