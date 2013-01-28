namespace Microsoft.WindowsAzure.Management.Service.Gateway
{
	public enum GatewayErrorCode
	{
		InternalError,
		InvalidLocalSiteName,
		InvalidVnetName,
		InvalidSubscriptionId,
		InvalidOperationId,
		InvalidIPAddress,
		ResourceNotFound,
		ConflictError,
		GatewayNotProvisioned,
		InvalidSharedKeyLength,
		InvalidParameter,
		InvalidCultureName,
		SubscriptionNotAuthorized,
		DestinationUnreachable,
		UserUnauthorized
	}
}