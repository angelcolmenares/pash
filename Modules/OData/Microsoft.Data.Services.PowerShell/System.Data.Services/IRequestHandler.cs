namespace System.Data.Services
{
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;

    [ServiceContract]
    internal interface IRequestHandler
    {
        [OperationContract, WebInvoke(UriTemplate="*", Method="*")]
        Message ProcessRequestForMessage(Stream messageBody);
    }
}

