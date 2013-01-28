using System;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Microsoft.WSMan.Enumeration;

namespace Microsoft.WSMan.Eventing
{
   [ServiceContract]
   [XmlSerializerFormat(Style = OperationFormatStyle.Document, Use = OperationFormatUse.Literal)]
   public interface IWSEventingPullDeliveryContract
   {
      [OperationContract(Action = Enumeration.EnumerationActions.PullAction, ReplyAction = Enumeration.EnumerationActions.PullResponseAction)]
      PullResponse Pull(PullRequest request);
   }
}
