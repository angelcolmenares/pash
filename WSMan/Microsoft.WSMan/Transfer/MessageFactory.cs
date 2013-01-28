using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Microsoft.WSMan.Transfer
{
   public class MessageFactory
   {
      private readonly MessageVersion _version;

      public MessageFactory()
      {
      }

      public MessageFactory(MessageVersion version)
      {
         _version = version;
      }

      public Message CreateGetRequest()
      {
		 return CreateMessageWithPayload("PAYLOAD", TransferActions.GetAction);
      }

      public Message CreateGetResponse(object body)
      {
         return CreateMessageWithPayload(body, TransferActions.GetResponseAction);
      }

      public Message CreatePutRequest(object payload)
      {
         return CreateMessageWithPayload(payload, TransferActions.PutAction);
      }                   

      public Message CreatePutResponse(object payload)
      {
         return CreateMessageWithPayload(payload, TransferActions.PutResponseAction);
      }

      public Message CreateCreateRequest(object payload)
      {         
         return CreateMessageWithPayload(payload, TransferActions.CreateAction);
      }

      public Message CreateCreateResponse(EndpointAddress result)
      {
         return Message.CreateMessage(GetMessageVersion(), TransferActions.CreateResponseAction,
                                      new CreateRsponseBodyWriter(result, GetMessageVersion().Addressing));
      }

      public Message CreateDeleteRequest()
      {
         return CreateMessageWithPayload("PAYLOAD", TransferActions.DeleteAction);
      }

      public Message CreateDeleteResponse()
      {
		 return CreateMessageWithPayload("PAYLOAD", TransferActions.DeleteResponseAction);
      }

      public EndpointAddress DeserializeCreateResponse(Message createResponseMessage)
      {         
         XmlDictionaryReader reader = createResponseMessage.GetReaderAtBodyContents();
         reader.ReadStartElement(TransferActions.CreateResponse_ResourceCreatedElement, TransferActions.Namespace);

         EndpointAddress result = EndpointAddress.ReadFrom(reader);

         if (reader.NodeType == XmlNodeType.EndElement)
         {
            reader.ReadEndElement();
         }

         return result;
      }

      public object DeserializeMessageWithPayload(Message messageWithPayload, Type expectedType)
      {
         if (messageWithPayload.IsEmpty)
         {
            return null;
         }
         if (typeof(IXmlSerializable).IsAssignableFrom(expectedType))
         {
            IXmlSerializable serializable = (IXmlSerializable)Activator.CreateInstance(expectedType);
            serializable.ReadXml(messageWithPayload.GetReaderAtBodyContents());
            return serializable;
         }
         XmlSerializer xs = new XmlSerializer(expectedType);         
         return xs.Deserialize(messageWithPayload.GetReaderAtBodyContents());
      }

      public Message CreateMessageWithPayload(object payload, string action)
      {
         Message respose = Message.CreateMessage(GetMessageVersion(), action, new SerializerBodyWriter(payload));         
         return respose;
      }  
    
      private MessageVersion GetMessageVersion()
      {
         return _version ?? OperationContext.Current.IncomingMessageVersion;
      }
   }
}