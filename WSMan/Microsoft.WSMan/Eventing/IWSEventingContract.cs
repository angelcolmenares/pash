using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Microsoft.WSMan.Eventing
{
   [ServiceContract]
   [XmlSerializerFormat(Style = OperationFormatStyle.Document, Use = OperationFormatUse.Literal)]
   public interface IWSEventingContract
   {
      [OperationContract(Action = EventingActions.SubscribeAction, ReplyAction = EventingActions.SubscribeResponseAction)]
      SubscribeResponse Subscribe(SubscribeRequest request);

      [OperationContract(Action = EventingActions.UnsubscribeAction, IsOneWay = true)]
      void Unsubscribe(UnsubscribeRequest request);

      [OperationContract(Action = EventingActions.RenewAction, ReplyAction = EventingActions.RenewResponseAction)]
      RenewResponse Renew(RenewRequest request);
   }
}
