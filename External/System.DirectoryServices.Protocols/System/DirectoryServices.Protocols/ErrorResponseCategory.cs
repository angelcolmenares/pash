namespace System.DirectoryServices.Protocols
{
	public enum ErrorResponseCategory
	{
		NotAttempted,
		CouldNotConnect,
		ConnectionClosed,
		MalformedRequest,
		GatewayInternalError,
		AuthenticationFailed,
		UnresolvableUri,
		Other
	}
}