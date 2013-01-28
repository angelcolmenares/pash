using System;
using System.DirectoryServices.Protocols;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ADStoreAccess
	{
		private string _name;

		public string Name
		{
			get
			{
				return this._name;
			}
		}

		protected ADStoreAccess(string name)
		{
			this._name = name;
		}

		internal static int MapResultCodeToErrorCode(ResultCode resultCode)
		{
			ResultCode resultCode1 = resultCode;
			switch (resultCode1)
			{
				case ResultCode.Success:
				{
					return 0;
				}
				case ResultCode.OperationsError:
				{
					return 0x2020;
				}
				case ResultCode.ProtocolError:
				{
					return 0x2021;
				}
				case ResultCode.TimeLimitExceeded:
				{
					return 0x2022;
				}
				case ResultCode.SizeLimitExceeded:
				{
					return 0x2023;
				}
				case ResultCode.CompareFalse:
				{
					return 0x2025;
				}
				case ResultCode.CompareTrue:
				{
					return 0x2026;
				}
				case ResultCode.AuthMethodNotSupported:
				{
					return 0x2027;
				}
				case ResultCode.StrongAuthRequired:
				{
					return 0x2028;
				}
				case ResultCode.ReferralV2:
				{
					return 234;
				}
				case ResultCode.Referral:
				{
					return 0x202b;
				}
				case ResultCode.AdminLimitExceeded:
				{
					return 0x2024;
				}
				case ResultCode.UnavailableCriticalExtension:
				{
					return 0x202c;
				}
				case ResultCode.ConfidentialityRequired:
				{
					return 0x202d;
				}
				case ResultCode.SaslBindInProgress:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress:
				case ResultCode.ProtocolError | ResultCode.SizeLimitExceeded | ResultCode.CompareTrue | ResultCode.NoSuchAttribute | ResultCode.InappropriateMatching | ResultCode.AttributeOrValueExists:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax:
				case ResultCode.StrongAuthRequired | ResultCode.NoSuchAttribute:
				case ResultCode.OperationsError | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType:
				case ResultCode.ProtocolError | ResultCode.StrongAuthRequired | ResultCode.Referral | ResultCode.NoSuchAttribute | ResultCode.InappropriateMatching:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation:
				case ResultCode.SizeLimitExceeded | ResultCode.StrongAuthRequired | ResultCode.UnavailableCriticalExtension | ResultCode.NoSuchAttribute | ResultCode.AttributeOrValueExists:
				case ResultCode.OperationsError | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax:
				case ResultCode.ProtocolError | ResultCode.SizeLimitExceeded | ResultCode.CompareTrue | ResultCode.StrongAuthRequired | ResultCode.Referral | ResultCode.UnavailableCriticalExtension | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.InappropriateMatching | ResultCode.AttributeOrValueExists:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax:
				case ResultCode.OperationsError | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.AliasDereferencingProblem:
				case ResultCode.ProtocolError | ResultCode.SizeLimitExceeded | ResultCode.CompareTrue | ResultCode.NoSuchObject | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem:
				case ResultCode.StrongAuthRequired | ResultCode.NoSuchObject:
				case ResultCode.OperationsError | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.NoSuchObject | ResultCode.AliasProblem:
				case ResultCode.ProtocolError | ResultCode.StrongAuthRequired | ResultCode.Referral | ResultCode.NoSuchObject | ResultCode.InvalidDNSyntax:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax:
				case ResultCode.SizeLimitExceeded | ResultCode.StrongAuthRequired | ResultCode.UnavailableCriticalExtension | ResultCode.NoSuchObject | ResultCode.AliasDereferencingProblem:
				case ResultCode.OperationsError | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.AliasDereferencingProblem:
				case ResultCode.ProtocolError | ResultCode.SizeLimitExceeded | ResultCode.CompareTrue | ResultCode.StrongAuthRequired | ResultCode.Referral | ResultCode.UnavailableCriticalExtension | ResultCode.SaslBindInProgress | ResultCode.NoSuchObject | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Busy | ResultCode.Unavailable | ResultCode.UnwillingToPerform | ResultCode.LoopDetect:
				case ResultCode.StrongAuthRequired | ResultCode.NoSuchAttribute | ResultCode.NoSuchObject | ResultCode.InappropriateAuthentication:
				case ResultCode.OperationsError | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InappropriateAuthentication:
				case ResultCode.ProtocolError | ResultCode.StrongAuthRequired | ResultCode.Referral | ResultCode.NoSuchAttribute | ResultCode.InappropriateMatching | ResultCode.NoSuchObject | ResultCode.InvalidDNSyntax | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Busy:
				case ResultCode.SortControlMissing:
				case ResultCode.OffsetRangeError:
				case ResultCode.ProtocolError | ResultCode.SizeLimitExceeded | ResultCode.CompareTrue | ResultCode.StrongAuthRequired | ResultCode.Referral | ResultCode.UnavailableCriticalExtension | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.InappropriateMatching | ResultCode.AttributeOrValueExists | ResultCode.NoSuchObject | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Unavailable | ResultCode.LoopDetect | ResultCode.SortControlMissing:
				case ResultCode.OperationsError | ResultCode.ProtocolError | ResultCode.TimeLimitExceeded | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.CompareTrue | ResultCode.AuthMethodNotSupported | ResultCode.StrongAuthRequired | ResultCode.ReferralV2 | ResultCode.Referral | ResultCode.AdminLimitExceeded | ResultCode.UnavailableCriticalExtension | ResultCode.ConfidentialityRequired | ResultCode.SaslBindInProgress | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.InappropriateMatching | ResultCode.ConstraintViolation | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InvalidDNSyntax | ResultCode.AliasDereferencingProblem | ResultCode.InappropriateAuthentication | ResultCode.InsufficientAccessRights | ResultCode.Busy | ResultCode.Unavailable | ResultCode.UnwillingToPerform | ResultCode.LoopDetect | ResultCode.SortControlMissing | ResultCode.OffsetRangeError:
				{
					return 0x2095;
				}
				case ResultCode.NoSuchAttribute:
				{
					return 0x200a;
				}
				case ResultCode.UndefinedAttributeType:
				{
					return 0x200c;
				}
				case ResultCode.InappropriateMatching:
				{
					return 0x202e;
				}
				case ResultCode.ConstraintViolation:
				{
					return 0x202f;
				}
				case ResultCode.AttributeOrValueExists:
				{
					return 0x200d;
				}
				case ResultCode.InvalidAttributeSyntax:
				{
					return 0x200b;
				}
				case ResultCode.NoSuchObject:
				{
					return 0x2030;
				}
				case ResultCode.AliasProblem:
				{
					return 0x2031;
				}
				case ResultCode.InvalidDNSyntax:
				{
					return 0x2032;
				}
				case ResultCode.AliasDereferencingProblem:
				{
					return 0x2034;
				}
				case ResultCode.InappropriateAuthentication:
				{
					return 0x2029;
				}
				case ResultCode.OperationsError | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.NoSuchObject | ResultCode.AliasProblem | ResultCode.InappropriateAuthentication:
				{
					return 0x52e;
				}
				case ResultCode.InsufficientAccessRights:
				{
					return 5;
				}
				case ResultCode.Busy:
				{
					return 0x200e;
				}
				case ResultCode.Unavailable:
				{
					return 0x200f;
				}
				case ResultCode.UnwillingToPerform:
				{
					return 0x2035;
				}
				case ResultCode.LoopDetect:
				{
					return 0x2036;
				}
				case ResultCode.NamingViolation:
				{
					return 0x2037;
				}
				case ResultCode.ObjectClassViolation:
				{
					return 0x2014;
				}
				case ResultCode.NotAllowedOnNonLeaf:
				{
					return 0x2015;
				}
				case ResultCode.NotAllowedOnRdn:
				{
					return 0x2016;
				}
				case ResultCode.EntryAlreadyExists:
				{
					return 0x1392;
				}
				case ResultCode.ObjectClassModificationsProhibited:
				{
					return 0x2017;
				}
				case ResultCode.ResultsTooLarge:
				{
					return 0x2038;
				}
				case ResultCode.AffectsMultipleDsas:
				{
					return 0x2039;
				}
				default:
				{
					if (resultCode1 == (ResultCode.OperationsError | ResultCode.SizeLimitExceeded | ResultCode.CompareFalse | ResultCode.NoSuchAttribute | ResultCode.UndefinedAttributeType | ResultCode.AttributeOrValueExists | ResultCode.InvalidAttributeSyntax | ResultCode.NamingViolation | ResultCode.ObjectClassViolation | ResultCode.EntryAlreadyExists | ResultCode.ObjectClassModificationsProhibited | ResultCode.Other))
					{
						return 0x2022;
					}
					return 0x2095;
				}
			}
		}

		internal static void ThrowExceptionForResultCodeError(ResultCode resultCode, string message, Exception innerException)
		{
			ADStoreAccess.ThrowExceptionForResultCodeError(resultCode, message, null, innerException);
		}

		internal static void ThrowExceptionForResultCodeError(ResultCode resultCode, string message, string serverErrorMessage, Exception innerException)
		{
			if (resultCode != ResultCode.Success)
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(ADStoreAccess.MapResultCodeToErrorCode(resultCode), message, serverErrorMessage, innerException);
			}
			else
			{
				return;
			}
		}

		private enum LdapError
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

		internal enum LdapSessionOption
		{
			LDAP_OPT_DESC = 1,
			LDAP_OPT_DEREF = 2,
			LDAP_OPT_SIZELIMIT = 3,
			LDAP_OPT_TIMELIMIT = 4,
			LDAP_OPT_REFERRALS = 8,
			LDAP_OPT_RESTART = 9,
			LDAP_OPT_SSL = 10,
			LDAP_OPT_REFERRAL_HOP_LIMIT = 16,
			LDAP_OPT_VERSION = 17,
			LDAP_OPT_API_FEATURE_INFO = 21,
			LDAP_OPT_HOST_NAME = 48,
			LDAP_OPT_ERROR_NUMBER = 49,
			LDAP_OPT_ERROR_STRING = 50,
			LDAP_OPT_SERVER_ERROR = 51,
			LDAP_OPT_SERVER_EXT_ERROR = 52,
			LDAP_OPT_PING_KEEP_ALIVE = 54,
			LDAP_OPT_PING_WAIT_TIME = 55,
			LDAP_OPT_PING_LIMIT = 56,
			LDAP_OPT_DNSDOMAIN_NAME = 59,
			LDAP_OPT_GETDSNAME_FLAGS = 61,
			LDAP_OPT_HOST_REACHABLE = 62,
			LDAP_OPT_PROMPT_CREDENTIALS = 63,
			LDAP_OPT_TCP_KEEPALIVE = 64,
			LDAP_OPT_FAST_CONCURRENT_BIND = 65,
			LDAP_OPT_SEND_TIMEOUT = 66,
			LDAP_OPT_REFERRAL_CALLBACK = 112,
			LDAP_OPT_CLIENT_CERTIFICATE = 128,
			LDAP_OPT_SERVER_CERTIFICATE = 129,
			LDAP_OPT_AUTO_RECONNECT = 145,
			LDAP_OPT_SSPI_FLAGS = 146,
			LDAP_OPT_SSL_INFO = 147,
			LDAP_OPT_SIGN = 149,
			LDAP_OPT_ENCRYPT = 150,
			LDAP_OPT_SASL_METHOD = 151,
			LDAP_OPT_AREC_EXCLUSIVE = 152,
			LDAP_OPT_SECURITY_CONTEXT = 153,
			LDAP_OPT_ROOTDSE_CACHE = 154
		}
	}
}