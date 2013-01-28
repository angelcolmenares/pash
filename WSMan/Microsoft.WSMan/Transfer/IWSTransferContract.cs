using System;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.WSMan.Transfer
{
   [ServiceContract(Namespace = TransferActions.Namespace)]
   public interface IWSTransferContract
   {
      [OperationContract(Action = TransferActions.GetAction, ReplyAction = TransferActions.GetResponseAction)]
      Message Get(Message getRequest);

      [OperationContract(Action = TransferActions.PutAction, ReplyAction = TransferActions.PutResponseAction)]
      Message Put(Message putRequest);

      [OperationContract(Action = TransferActions.CreateAction, ReplyAction = TransferActions.CreateResponseAction)]
      Message Create(Message createRequest);

      [OperationContract(Action = TransferActions.DeleteAction, ReplyAction = TransferActions.DeleteResponseAction)]
      Message Delete(Message deleteRequest);
   }
}