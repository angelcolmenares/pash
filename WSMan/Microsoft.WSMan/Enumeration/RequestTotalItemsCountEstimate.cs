using System.ServiceModel.Channels;
using System.Xml;
using Microsoft.WSMan.Management;

namespace Microsoft.WSMan.Enumeration
{
   public class RequestTotalItemsCountEstimate : MessageHeader
   {
      private const string ElementName = "RequestTotalItemsCountEstimate";

      public static bool IsPresent
      {
         get
         {
            RequestTotalItemsCountEstimate header =
               OperationContextProxy.Current.FindHeader<RequestTotalItemsCountEstimate>();
            return header != null;
         }      
      }

      public static RequestTotalItemsCountEstimate ReadFrom(MessageHeaders messageHeaders)
      {
         int index = messageHeaders.FindHeader(ElementName, ManagementNamespaces.Namespace);
         if (index < 0)
         {
            return null;
         }         
         MessageHeaderInfo headerInfo = messageHeaders[index];
         return new RequestTotalItemsCountEstimate();
      }

      public override string Name
      {
         get { return ElementName; }
      }

      public override string Namespace
      {
         get { return ManagementNamespaces.Namespace; }
      }

      protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
      {         
      }
   }
}