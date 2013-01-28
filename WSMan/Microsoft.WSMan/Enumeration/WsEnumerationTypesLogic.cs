//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ServiceModel;
//using System.ServiceModel.Channels;
//using System.Text;
//using System.Xml;
//using System.Xml.Schema;
//using System.Xml.Serialization;

//namespace Microsoft.WSMan.Enumeration
//{
//   public sealed class OptimizeEnumerationType
//   {
//   }

//   public enum EnumerationMode
//   {
//      EnumerateEPR
//   }

//   [MessageContract(IsWrapped = true, WrapperNamespace = Schema.EnumerationNamespace)]
//   public partial class Enumerate
//   {
//      [XmlElement(Namespace = Schema.EnumerationNamespace)]
//      public OptimizeEnumerationType OptimizeEnumeration { get; set; }

//      [XmlElement(Namespace = Schema.EnumerationNamespace)]
//      public string EnumerationMode { get; set; }

//      public Enumerate()
//      {
//      }

//      public Enumerate(bool optimized, EnumerationMode? mode, string filterUri, QueryExpr filterExpression)
//      {
//         if (optimized)
//         {
//            OptimizeEnumeration = new OptimizeEnumerationType();
//         }
//         if (mode != null)
//         {
//            EnumerationMode = mode.ToString();
//         }
//         if (filterUri != null)
//         {
//            filterField = new FilterType { Dialect = filterUri, QueryExpr = filterExpression };
//         }
//      }
//   }

//   public partial class PullResponse // : IXmlSerializable
//   {
//      [XmlArray(ElementName = "Items")]
//      [XmlArrayItem(ElementName = "EndpointReference", Type = typeof(EndpointAddress10), Namespace = WsAddressing.Namespace)]
//      public List<EndpointAddress10> EnumerateEPRItems { get; set; }

//      [XmlElement(Namespace = Schema.EnumerationNamespace)]
//      public string EnumerationContext { get; set; }

//      [XmlElement(Namespace = Schema.EnumerationNamespace)]
//      public string EndOfSequence { get; set; }

//      public PullResponse()
//      {
//         EnumerateEPRItems = new List<EndpointAddress10>();
//         //EnumerationContext = Guid.NewGuid().ToString();
//         EndOfSequence = "";
//      }
//   }

//   public partial class EnumerateResponse // : IXmlSerializable
//   {
//      [XmlArray(ElementName = "Items", Namespace = Schema.ManagementNamespace)]
//      [XmlArrayItem(ElementName = "EndpointReference", Type = typeof(EndpointAddressAugust2004), Namespace = WsAddressing.Namespace)]
//      public List<EndpointAddressAugust2004> EnumerateEPRItems { get; set; }

//      [XmlElement(Namespace = Schema.EnumerationNamespace)]
//      public string EnumerationContext { get; set; }

//      [XmlElement(Namespace = Schema.ManagementNamespace)]
//      public string EndOfSequence { get; set; }

//      public EnumerateResponse()
//      {
//         EndOfSequence = "";
//         //EnumerationContext = Guid.NewGuid().ToString();
//      }

//      public EnumerateResponse(IEnumerable<EndpointAddress> eprs)
//         : this()
//      {
//         EnumerateEPRItems = new List<EndpointAddressAugust2004>(eprs.Select(x => EndpointAddressAugust2004.FromEndpointAddress(x)));
//      }

//      public IEnumerable<EndpointAddress> DeserializeAsEPRs()
//      {
//         return EnumerateEPRItems.Select(x => x.ToEndpointAddress());
//      }

//      public XmlSchema GetSchema()
//      {
//         throw new NotImplementedException();
//      }

//      public void ReadXml(XmlReader reader)
//      {
//         throw new NotImplementedException();
//      }

//      public void WriteXml(XmlWriter writer)
//      {
//         //           if (EnumerateEPRItems.Count > 0)
//         {
//            //               writer.WriteElementString("Expires", Simon.WsManagement.Schema.EnumerationNamespace, "P0Y0M0DT0H10M0.000S");
//            writer.WriteElementString("Expires", Schema.EnumerationNamespace, "PT10M");
//            writer.WriteElementString("EnumerationContext", Schema.EnumerationNamespace, EnumerationContext);
//            writer.WriteStartElement("Items", Schema.ManagementNamespace);
//            foreach (var epa in EnumerateEPRItems)
//            {
//               //                   writer.WriteStartElement("EndpointReference", Simon.WsManagement.Schema.AddressingNamespace);
//               epa.ToEndpointAddress().WriteTo(AddressingVersion.WSAddressingAugust2004, writer); //, "EndpointReference", Simon.WsManagement.Schema.AddressingNamespace);
//               //                   writer.WriteEndElement();
//            }
//            writer.WriteEndElement();
//         }
//         writer.WriteStartElement("EndOfSequence", Schema.ManagementNamespace);
//         writer.WriteEndElement();
//      }
//   }
//}
