using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace System.DirectoryServices.Protocols
{
	internal sealed class Res
	{
		internal const string DsmlNonHttpUri = "DsmlNonHttpUri";

		internal const string NoNegativeTime = "NoNegativeTime";

		internal const string NoNegativeSizeLimit = "NoNegativeSizeLimit";

		internal const string InvalidDocument = "InvalidDocument";

		internal const string MissingSessionId = "MissingSessionId";

		internal const string MissingResponse = "MissingResponse";

		internal const string ErrorResponse = "ErrorResponse";

		internal const string BadControl = "BadControl";

		internal const string NullDirectoryAttribute = "NullDirectoryAttribute";

		internal const string NullDirectoryAttributeCollection = "NullDirectoryAttributeCollection";

		internal const string WhiteSpaceServerName = "WhiteSpaceServerName";

		internal const string DirectoryAttributeConversion = "DirectoryAttributeConversion";

		internal const string WrongNumValuesCompare = "WrongNumValuesCompare";

		internal const string WrongAssertionCompare = "WrongAssertionCompare";

		internal const string DefaultOperationsError = "DefaultOperationsError";

		internal const string BadSearchLDAPFilter = "BadSearchLDAPFilter";

		internal const string ReadOnlyProperty = "ReadOnlyProperty";

		internal const string MissingOperationResponseResultCode = "MissingOperationResponseResultCode";

		internal const string MissingSearchResultEntryDN = "MissingSearchResultEntryDN";

		internal const string MissingSearchResultEntryAttributeName = "MissingSearchResultEntryAttributeName";

		internal const string BadOperationResponseResultCode = "BadOperationResponseResultCode";

		internal const string MissingErrorResponseType = "MissingErrorResponseType";

		internal const string ErrorResponseInvalidValue = "ErrorResponseInvalidValue";

		internal const string NotSupportOnDsmlErrRes = "NotSupportOnDsmlErrRes";

		internal const string BadBase64Value = "BadBase64Value";

		internal const string WrongAuthType = "WrongAuthType";

		internal const string SessionInUse = "SessionInUse";

		internal const string ReadOnlyDocument = "ReadOnlyDocument";

		internal const string NotWellFormedResponse = "NotWellFormedResponse";

		internal const string NoCurrentSession = "NoCurrentSession";

		internal const string UnknownResponseElement = "UnknownResponseElement";

		internal const string InvalidClientCertificates = "InvalidClientCertificates";

		internal const string InvalidAuthCredential = "InvalidAuthCredential";

		internal const string InvalidLdapSearchRequestFilter = "InvalidLdapSearchRequestFilter";

		internal const string PartialResultsNotSupported = "PartialResultsNotSupported";

		internal const string BerConverterNotMatch = "BerConverterNotMatch";

		internal const string BerConverterUndefineChar = "BerConverterUndefineChar";

		internal const string BerConversionError = "BerConversionError";

		internal const string TLSStopFailure = "TLSStopFailure";

		internal const string NoPartialResults = "NoPartialResults";

		internal const string DefaultLdapError = "DefaultLdapError";

		internal const string LDAP_PARTIAL_RESULTS = "LDAP_PARTIAL_RESULTS";

		internal const string LDAP_IS_LEAF = "LDAP_IS_LEAF";

		internal const string LDAP_SORT_CONTROL_MISSING = "LDAP_SORT_CONTROL_MISSING";

		internal const string LDAP_OFFSET_RANGE_ERROR = "LDAP_OFFSET_RANGE_ERROR";

		internal const string LDAP_RESULTS_TOO_LARGE = "LDAP_RESULTS_TOO_LARGE";

		internal const string LDAP_SERVER_DOWN = "LDAP_SERVER_DOWN";

		internal const string LDAP_LOCAL_ERROR = "LDAP_LOCAL_ERROR";

		internal const string LDAP_ENCODING_ERROR = "LDAP_ENCODING_ERROR";

		internal const string LDAP_DECODING_ERROR = "LDAP_DECODING_ERROR";

		internal const string LDAP_TIMEOUT = "LDAP_TIMEOUT";

		internal const string LDAP_AUTH_UNKNOWN = "LDAP_AUTH_UNKNOWN";

		internal const string LDAP_FILTER_ERROR = "LDAP_FILTER_ERROR";

		internal const string LDAP_USER_CANCELLED = "LDAP_USER_CANCELLED";

		internal const string LDAP_PARAM_ERROR = "LDAP_PARAM_ERROR";

		internal const string LDAP_NO_MEMORY = "LDAP_NO_MEMORY";

		internal const string LDAP_CONNECT_ERROR = "LDAP_CONNECT_ERROR";

		internal const string LDAP_NOT_SUPPORTED = "LDAP_NOT_SUPPORTED";

		internal const string LDAP_NO_RESULTS_RETURNED = "LDAP_NO_RESULTS_RETURNED";

		internal const string LDAP_CONTROL_NOT_FOUND = "LDAP_CONTROL_NOT_FOUND";

		internal const string LDAP_MORE_RESULTS_TO_RETURN = "LDAP_MORE_RESULTS_TO_RETURN";

		internal const string LDAP_CLIENT_LOOP = "LDAP_CLIENT_LOOP";

		internal const string LDAP_REFERRAL_LIMIT_EXCEEDED = "LDAP_REFERRAL_LIMIT_EXCEEDED";

		internal const string LDAP_INVALID_CREDENTIALS = "LDAP_INVALID_CREDENTIALS";

		internal const string LDAP_SUCCESS = "LDAP_SUCCESS";

		internal const string NoSessionIDReturned = "NoSessionIDReturned";

		internal const string LDAP_OPERATIONS_ERROR = "LDAP_OPERATIONS_ERROR";

		internal const string LDAP_PROTOCOL_ERROR = "LDAP_PROTOCOL_ERROR";

		internal const string LDAP_TIMELIMIT_EXCEEDED = "LDAP_TIMELIMIT_EXCEEDED";

		internal const string LDAP_SIZELIMIT_EXCEEDED = "LDAP_SIZELIMIT_EXCEEDED";

		internal const string LDAP_COMPARE_FALSE = "LDAP_COMPARE_FALSE";

		internal const string LDAP_COMPARE_TRUE = "LDAP_COMPARE_TRUE";

		internal const string LDAP_AUTH_METHOD_NOT_SUPPORTED = "LDAP_AUTH_METHOD_NOT_SUPPORTED";

		internal const string LDAP_STRONG_AUTH_REQUIRED = "LDAP_STRONG_AUTH_REQUIRED";

		internal const string LDAP_REFERRAL = "LDAP_REFERRAL";

		internal const string LDAP_ADMIN_LIMIT_EXCEEDED = "LDAP_ADMIN_LIMIT_EXCEEDED";

		internal const string LDAP_UNAVAILABLE_CRIT_EXTENSION = "LDAP_UNAVAILABLE_CRIT_EXTENSION";

		internal const string LDAP_CONFIDENTIALITY_REQUIRED = "LDAP_CONFIDENTIALITY_REQUIRED";

		internal const string LDAP_SASL_BIND_IN_PROGRESS = "LDAP_SASL_BIND_IN_PROGRESS";

		internal const string LDAP_NO_SUCH_ATTRIBUTE = "LDAP_NO_SUCH_ATTRIBUTE";

		internal const string LDAP_UNDEFINED_TYPE = "LDAP_UNDEFINED_TYPE";

		internal const string LDAP_INAPPROPRIATE_MATCHING = "LDAP_INAPPROPRIATE_MATCHING";

		internal const string LDAP_CONSTRAINT_VIOLATION = "LDAP_CONSTRAINT_VIOLATION";

		internal const string LDAP_ATTRIBUTE_OR_VALUE_EXISTS = "LDAP_ATTRIBUTE_OR_VALUE_EXISTS";

		internal const string LDAP_INVALID_SYNTAX = "LDAP_INVALID_SYNTAX";

		internal const string LDAP_NO_SUCH_OBJECT = "LDAP_NO_SUCH_OBJECT";

		internal const string LDAP_ALIAS_PROBLEM = "LDAP_ALIAS_PROBLEM";

		internal const string LDAP_INVALID_DN_SYNTAX = "LDAP_INVALID_DN_SYNTAX";

		internal const string LDAP_ALIAS_DEREF_PROBLEM = "LDAP_ALIAS_DEREF_PROBLEM";

		internal const string LDAP_INAPPROPRIATE_AUTH = "LDAP_INAPPROPRIATE_AUTH";

		internal const string LDAP_INSUFFICIENT_RIGHTS = "LDAP_INSUFFICIENT_RIGHTS";

		internal const string LDAP_BUSY = "LDAP_BUSY";

		internal const string LDAP_UNAVAILABLE = "LDAP_UNAVAILABLE";

		internal const string LDAP_UNWILLING_TO_PERFORM = "LDAP_UNWILLING_TO_PERFORM";

		internal const string LDAP_LOOP_DETECT = "LDAP_LOOP_DETECT";

		internal const string LDAP_NAMING_VIOLATION = "LDAP_NAMING_VIOLATION";

		internal const string LDAP_OBJECT_CLASS_VIOLATION = "LDAP_OBJECT_CLASS_VIOLATION";

		internal const string LDAP_NOT_ALLOWED_ON_NONLEAF = "LDAP_NOT_ALLOWED_ON_NONLEAF";

		internal const string LDAP_NOT_ALLOWED_ON_RDN = "LDAP_NOT_ALLOWED_ON_RDN";

		internal const string LDAP_ALREADY_EXISTS = "LDAP_ALREADY_EXISTS";

		internal const string LDAP_NO_OBJECT_CLASS_MODS = "LDAP_NO_OBJECT_CLASS_MODS";

		internal const string LDAP_AFFECTS_MULTIPLE_DSAS = "LDAP_AFFECTS_MULTIPLE_DSAS";

		internal const string LDAP_VIRTUAL_LIST_VIEW_ERROR = "LDAP_VIRTUAL_LIST_VIEW_ERROR";

		internal const string LDAP_OTHER = "LDAP_OTHER";

		internal const string LDAP_SEND_TIMEOUT = "LDAP_SEND_TIMEOUT";

		internal const string InvalidAsyncResult = "InvalidAsyncResult";

		internal const string ValidDirectoryAttributeType = "ValidDirectoryAttributeType";

		internal const string ValidFilterType = "ValidFilterType";

		internal const string ValidValuesType = "ValidValuesType";

		internal const string ValidValueType = "ValidValueType";

		internal const string SupportedPlatforms = "SupportedPlatforms";

		internal const string TLSNotSupported = "TLSNotSupported";

		internal const string InvalidValueType = "InvalidValueType";

		internal const string ValidValue = "ValidValue";

		internal const string ContainNullControl = "ContainNullControl";

		internal const string InvalidFilterType = "InvalidFilterType";

		internal const string NotReturnedAsyncResult = "NotReturnedAsyncResult";

		internal const string DsmlAuthRequestNotSupported = "DsmlAuthRequestNotSupported";

		internal const string CallBackIsNull = "CallBackIsNull";

		internal const string NullValueArray = "NullValueArray";

		internal const string NonCLSException = "NonCLSException";

		internal const string ConcurrentBindNotSupport = "ConcurrentBindNotSupport";

		internal const string TimespanExceedMax = "TimespanExceedMax";

		internal const string InvliadRequestType = "InvliadRequestType";

		private static Res loader;

		private ResourceManager resources;

		private static CultureInfo Culture
		{
			get
			{
				return null;
			}
		}

		public static ResourceManager Resources
		{
			get
			{
				return Res.GetLoader().resources;
			}
		}

		static Res()
		{
		}

		internal Res()
		{
			this.resources = new ResourceManager("System.DirectoryServices.Protocols", this.GetType().Assembly);
		}

		private static Res GetLoader()
		{
			if (Res.loader == null)
			{
				Res re = new Res();
				Interlocked.CompareExchange<Res>(ref Res.loader, re, null);
			}
			return Res.loader;
		}

		public static object GetObject(string name)
		{
			Res loader = Res.GetLoader();
			if (loader != null)
			{
				return loader.resources.GetObject(name, Res.Culture);
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name, object[] args)
		{
			Res loader = Res.GetLoader();
			if (loader != null)
			{
				string str = loader.resources.GetString(name, Res.Culture);
				if (args == null || (int)args.Length <= 0)
				{
					return str;
				}
				else
				{
					for (int i = 0; i < (int)args.Length; i++)
					{
						string str1 = args[i] as string;
						if (str1 != null && str1.Length > 0x400)
						{
							args[i] = string.Concat(str1.Substring(0, 0x3fd), "...");
						}
					}
					return string.Format(CultureInfo.CurrentCulture, str, args);
				}
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name)
		{
			Res loader = Res.GetLoader();
			if (loader != null)
			{
				return loader.resources.GetString(name, Res.Culture);
			}
			else
			{
				return null;
			}
		}

		public static string GetString(string name, out bool usedFallback)
		{
			usedFallback = false;
			return Res.GetString(name);
		}
	}
}