using System;

namespace Microsoft.Samples.WindowsAzure.ServiceManagement
{
	public static class ErrorCode
	{
		public const string MissingOrIncorrectVersionHeader = "MissingOrIncorrectVersionHeader";

		public const string InvalidRequest = "InvalidRequest";

		public const string InvalidXmlRequest = "InvalidXmlRequest";

		public const string InvalidContentType = "InvalidContentType";

		public const string MissingOrInvalidRequiredQueryParameter = "MissingOrInvalidRequiredQueryParameter";

		public const string InvalidHttpVerb = "InvalidHttpVerb";

		public const string InternalError = "InternalError";

		public const string BadRequest = "BadRequest";

		public const string AuthenticationFailed = "AuthenticationFailed";

		public const string ResourceNotFound = "ResourceNotFound";

		public const string SubscriptionDisabled = "SubscriptionDisabled";

		public const string ServerBusy = "ServerBusy";

		public const string TooManyRequests = "TooManyRequests";

		public const string ConflictError = "ConflictError";

		public const string PreconditionError = "PreconditionError";

		public const string ConfiguraitonError = "ConfigurationError";

		public const string ForbiddenError = "ForbiddenError";

	}
}