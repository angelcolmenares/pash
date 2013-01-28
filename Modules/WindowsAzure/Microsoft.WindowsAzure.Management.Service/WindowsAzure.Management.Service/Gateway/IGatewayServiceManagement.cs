using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Service;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	[ServiceContract(Namespace="http://schemas.microsoft.com/windowsazure")]
	public interface IGatewayServiceManagement
	{
		[OperationContract(AsyncPattern=true)]
		[WebInvoke(UriTemplate="subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway", Method="POST")]
		IAsyncResult BeginCreateGateway(string subscriptionId, string vnetName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(UriTemplate="subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway", Method="DELETE")]
		IAsyncResult BeginDeleteGateway(string subscriptionId, string vnetName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway")]
		IAsyncResult BeginGetGateway(string subscriptionId, string vnetName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="{subscriptionId}/operations/{operationId}")]
		IAsyncResult BeginGetGatewayOperation(string subscriptionId, string operationId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway/connection/{localNetworkSiteName}/sharedkey")]
		IAsyncResult BeginGetSharedKey(string subscriptionId, string vnetName, string localNetworkSiteName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway/connections")]
		IAsyncResult BeginListConnections(string subscriptionId, string vnetName, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebGet(UriTemplate="subscriptions/{subscriptionId}/Services/networking/supporteddevices")]
		IAsyncResult BeginListSupportedDevices(string subscriptionId, AsyncCallback callback, object state);

		[OperationContract(AsyncPattern=true)]
		[WebInvoke(UriTemplate="subscriptions/{subscriptionId}/Services/networking/{vnetName}/gateway/connection/{localNetworkSiteName}", Method="PUT")]
		IAsyncResult BeginUpdateConnection(string subscriptionId, string vnetName, string localNetworkSiteName, UpdateConnection updateConnection, AsyncCallback callback, object state);

		GatewayOperationAsyncResponse EndCreateGateway(IAsyncResult asyncResult);

		GatewayOperationAsyncResponse EndDeleteGateway(IAsyncResult asyncResult);

		VnetGateway EndGetGateway(IAsyncResult asyncResult);

		Operation EndGetGatewayOperation(IAsyncResult asyncResult);

		SharedKey EndGetSharedKey(IAsyncResult asyncResult);

		ConnectionCollection EndListConnections(IAsyncResult asyncResult);

		Stream EndListSupportedDevices(IAsyncResult asyncResult);

		GatewayOperationAsyncResponse EndUpdateConnection(IAsyncResult asyncResult);
	}
}