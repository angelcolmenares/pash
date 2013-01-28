using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Service;
using System;
using System.IO;

namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	public static class GatewayServiceManagementExtensions
	{
		public static GatewayOperationAsyncResponse DeleteVirtualNetworkGateway(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName)
		{
			return proxy.EndDeleteGateway(proxy.BeginDeleteGateway(subscriptionId, virtualNetworkName, null, null));
		}

		public static Operation GetGatewayOperation(this IGatewayServiceManagement proxy, string subscriptionId, string operationId)
		{
			return proxy.EndGetGatewayOperation(proxy.BeginGetGatewayOperation(subscriptionId, operationId, null, null));
		}

		public static VnetGateway GetVirtualNetworkGateway(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName)
		{
			return proxy.EndGetGateway(proxy.BeginGetGateway(subscriptionId, virtualNetworkName, null, null));
		}

		public static SharedKey GetVirtualNetworkSharedKey(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName, string localNetworkSiteName)
		{
			return proxy.EndGetSharedKey(proxy.BeginGetSharedKey(subscriptionId, virtualNetworkName, localNetworkSiteName, null, null));
		}

		public static Stream GetVirtualNetworkSupportedDevices(this IGatewayServiceManagement proxy, string subscriptionId)
		{
			return proxy.EndListSupportedDevices(proxy.BeginListSupportedDevices(subscriptionId, null, null));
		}

		public static ConnectionCollection ListVirtualNetworkConnections(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName)
		{
			return proxy.EndListConnections(proxy.BeginListConnections(subscriptionId, virtualNetworkName, null, null));
		}

		public static GatewayOperationAsyncResponse NewVirtualNetworkGateway(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName)
		{
			return proxy.EndCreateGateway(proxy.BeginCreateGateway(subscriptionId, virtualNetworkName, null, null));
		}

		public static GatewayOperationAsyncResponse UpdateVirtualNetworkGatewayConnection(this IGatewayServiceManagement proxy, string subscriptionId, string virtualNetworkName, string localNetworkSiteName, UpdateConnection updateConnection)
		{
			return proxy.EndUpdateConnection(proxy.BeginUpdateConnection(subscriptionId, virtualNetworkName, localNetworkSiteName, updateConnection, null, null));
		}
	}
}