using System;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.WSMan.Transfer
{
   public class TransferActions
   {
      public const string Namespace = "http://schemas.xmlsoap.org/ws/2004/09/transfer";
      public const string GetAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Get";
      public const string GetResponseAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/GetResponse";
      public const string PutAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Put";
      public const string PutResponseAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/PutResponse";
      public const string CreateAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Create";
      public const string CreateResponseAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/CreateResponse";
      public const string DeleteAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/Delete";
      public const string DeleteResponseAction = "http://schemas.xmlsoap.org/ws/2004/09/transfer/DeleteResponse";

      public const string CreateResponse_ResourceCreatedElement = "ResourceCreated";
   }
}