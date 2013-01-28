using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Microsoft.WSMan.Enumeration
{
   [ServiceContract]
   [XmlSerializerFormat(Style = OperationFormatStyle.Document, Use = OperationFormatUse.Literal)]
   public interface IWSEnumerationContract
   {
      [OperationContract(Action = EnumerationActions.EnumerateAction, ReplyAction = EnumerationActions.EnumerateResponseAction)]
      EnumerateResponse Enumerate(EnumerateRequest request);

      [OperationContract(Action = EnumerationActions.PullAction, ReplyAction = EnumerationActions.PullResponseAction)]
      PullResponse Pull(PullRequest request);
   }
}
