using System.ServiceModel.Channels;
using System.Xml;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{
   public sealed class TotalItemsCountEstimate : MessageHeader
   {
      private const string ElementName = "TotalItemsCountEstimate";

      private readonly int _value;

      public TotalItemsCountEstimate(int value)
      {
         _value = value;
      }

      public static TotalItemsCountEstimate ReadFrom(XmlDictionaryReader reader)
      {
         reader.ReadStartElement(ElementName, ManagementNamespaces.Namespace);
         int value = XmlConvert.ToInt32(reader.ReadString());
         TotalItemsCountEstimate result = new TotalItemsCountEstimate(value);
         reader.ReadEndElement();
         return result;
      }

      public static TotalItemsCountEstimate ReadFrom(Message message)
      {
         return ReadFrom(message.Headers);
      }

      public static TotalItemsCountEstimate ReadFrom(MessageHeaders messageHeaders)
      {
         TotalItemsCountEstimate result;
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

      public override string Name
      {
         get { return ElementName; }
      }

      public override string Namespace
      {
         get { return ManagementNamespaces.Namespace; }
      }

      public int Value
      {
         get { return _value; }
      }

      protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
      {
         writer.WriteValue(Value);
      }
   }
}