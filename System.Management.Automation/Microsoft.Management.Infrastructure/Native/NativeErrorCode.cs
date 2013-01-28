namespace Microsoft.Management.Infrastructure
{
	public enum NativeErrorCode
	{
		Ok = 0,
		Failed = 1,
		AccessDenied = 2,
		InvalidNamespace = 3,
		InvalidParameter = 4,
		InvalidClass = 5,
		NotFound = 6,
		NotSupported = 7,
		ClassHasChildren = 8,
		ClassHasInstances = 9,
		InvalidSuperClass = 10,
		AlreadyExists = 11,
		NoSuchProperty = 12,
		TypeMismatch = 13,
		QueryLanguageNotSupported = 14,
		InvalidQuery = 15,
		MethodNotAvailable = 16,
		MethodNotFound = 17,
		NamespaceNotEmpty = 20,
		InvalidEnumerationContext = 21,
		InvalidOperationTimeout = 22,
		PullHasBeenAbandoned = 23,
		PullCannotBeAbandoned = 24,
		FilteredEnumerationNotSupported = 25,
		ContinuationOnErrorNotSupported = 26,
		ServerLimitsExceeded = 27,
		ServerIsShuttingDown = 28
	}
}