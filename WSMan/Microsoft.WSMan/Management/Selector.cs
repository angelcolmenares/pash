using System;
using System.ServiceModel.Channels;
using System.Xml;
using System.ServiceModel;

namespace Microsoft.WSMan.Management
{
   public sealed class Selector
   {
      internal const string ElementName = "Selector";

      private readonly string _name;
      private readonly string _simpleValue;
      private readonly EndpointAddress _addressReferenceValue;

	  public Selector (string name, byte[] value)
		: this(name, value == null ? null : Convert.ToBase64String(value))
	  {

	  }

      public Selector(string name, string value)
         : this(name)
      {
         if (value == null)
         {
            throw new ArgumentNullException("value");
         }
         _simpleValue = value;         
      }

      public Selector(string name, EndpointAddress value)
         : this(name)
      {
         if (value == null)
         {
            throw new ArgumentNullException("value");
         }
         _addressReferenceValue = value;
      }

      private Selector(string name)
      {
         _name = name;
      }

      public string Name
      {
         get { return _name; }
      }

      public object Value
      {
         get
         {
            if (SimpleValue != null)
            {
               return SimpleValue;
            }
            return AddressReference;
         }
      }

      public bool IsSimpleValue
      {
         get { return _simpleValue != null; }
      }

      public bool IsAddressReference
      {
         get { return _addressReferenceValue != null; }
      }

      public string SimpleValue
      {
         get
         {
            if (!IsSimpleValue)
            {
               throw new InvalidOperationException("This selector contains address reference value.");
            }
            return _simpleValue;
         }
      }

      public EndpointAddress AddressReference
      {
         get
         {
            if (!IsAddressReference)
            {
               throw new InvalidOperationException("This selctor contains simple value.");
            }
            return _addressReferenceValue;
         }
      }

      internal static Selector ReadFrom(XmlReader reader)
      {
         Selector result;
         string name = reader.GetAttribute("Name");
		reader.ReadStartElement(ElementName, ManagementNamespaces.Namespace);         
         if (reader.NodeType == XmlNodeType.Text)
         {
            result = new Selector(name, reader.Value);
            reader.Read();
         }
         else
         {
            EndpointAddress addr = EndpointAddress.ReadFrom(AddressingVersion.WSAddressing10, reader);
            result = new Selector(name, addr);
         }
         reader.ReadEndElement();
         return result;
      }

      internal void WriteTo(XmlDictionaryWriter writer)
      {
		 writer.WriteStartElement(ElementName, ManagementNamespaces.Namespace);
         writer.WriteAttributeString("Name", Name);
         if (_simpleValue != null)
         {
            writer.WriteValue(_simpleValue);
         }
         else if (_addressReferenceValue != null)
         {
			_addressReferenceValue.WriteTo(AddressingVersion.WSAddressing10, writer);
         }
         writer.WriteEndElement();
      }
   }
}