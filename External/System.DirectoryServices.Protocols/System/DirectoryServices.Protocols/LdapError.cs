namespace System.DirectoryServices.Protocols
{
	internal enum LdapError
	{
		IsLeaf = 35,
		InvalidCredentials = 49,
		ServerDown = 81,
		LocalError = 82,
		EncodingError = 83,
		DecodingError = 84,
		TimeOut = 85,
		AuthUnknown = 86,
		FilterError = 87,
		UserCancelled = 88,
		ParameterError = 89,
		NoMemory = 90,
		ConnectError = 91,
		NotSupported = 92,
		ControlNotFound = 93,
		NoResultsReturned = 94,
		MoreResults = 95,
		ClientLoop = 96,
		ReferralLimitExceeded = 97,
		SendTimeOut = 112
	}
}