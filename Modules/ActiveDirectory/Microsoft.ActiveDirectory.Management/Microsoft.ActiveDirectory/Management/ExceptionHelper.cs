using Microsoft.ActiveDirectory;
using System;
using System.Security.Authentication;

namespace Microsoft.ActiveDirectory.Management
{
	internal class ExceptionHelper
	{
		internal const string ADSI_ERROR_FACILITY_CODE = "8007";

		internal const int NO_ERROR = 0;

		internal const int ERROR_ACCESS_DENIED = 5;

		internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

		internal const int ERROR_NOT_SUPPORTED = 50;

		internal const int ERROR_INVALID_PASSWORD = 86;

		internal const int ERROR_INVALID_PARAMETER = 87;

		internal const int ERROR_MORE_DATA = 234;

		internal const int ERROR_USER_EXISTS = 0x524;

		internal const int ERROR_PASSWORD_RESTRICTION = 0x52d;

		internal const int ERROR_LOGON_FAILURE = 0x52e;

		internal const int ERROR_OBJECT_ALREADY_EXISTS = 0x1392;

		internal const int ERROR_DS_NO_ATTRIBUTE_OR_VALUE = 0x200a;

		internal const int ERROR_DS_INVALID_ATTRIBUTE_SYNTAX = 0x200b;

		internal const int ERROR_DS_ATTRIBUTE_TYPE_UNDEFINED = 0x200c;

		internal const int ERROR_DS_ATTRIBUTE_OR_VALUE_EXISTS = 0x200d;

		internal const int ERROR_DS_BUSY = 0x200e;

		internal const int ERROR_DS_UNAVAILABLE = 0x200f;

		internal const int ERROR_DS_OBJ_CLASS_VIOLATION = 0x2014;

		internal const int ERROR_DS_CANT_ON_NON_LEAF = 0x2015;

		internal const int ERROR_DS_CANT_ON_RDN = 0x2016;

		internal const int ERROR_DS_CANT_MOD_OBJ_CLASS = 0x2017;

		internal const int ERROR_DS_OPERATIONS_ERROR = 0x2020;

		internal const int ERROR_DS_PROTOCOL_ERROR = 0x2021;

		internal const int ERROR_DS_TIMELIMIT_EXCEEDED = 0x2022;

		internal const int ERROR_DS_SIZELIMIT_EXCEEDED = 0x2023;

		internal const int ERROR_DS_ADMIN_LIMIT_EXCEEDED = 0x2024;

		internal const int ERROR_DS_COMPARE_FALSE = 0x2025;

		internal const int ERROR_DS_COMPARE_TRUE = 0x2026;

		internal const int ERROR_DS_AUTH_METHOD_NOT_SUPPORTED = 0x2027;

		internal const int ERROR_DS_STRONG_AUTH_REQUIRED = 0x2028;

		internal const int ERROR_DS_INAPPROPRIATE_AUTH = 0x2029;

		internal const int ERROR_DS_REFERRAL = 0x202b;

		internal const int ERROR_DS_UNAVAILABLE_CRIT_EXTENSION = 0x202c;

		internal const int ERROR_DS_CONFIDENTIALITY_REQUIRED = 0x202d;

		internal const int ERROR_DS_INAPPROPRIATE_MATCHING = 0x202e;

		internal const int ERROR_DS_CONSTRAINT_VIOLATION = 0x202f;

		internal const int ERROR_DS_NO_SUCH_OBJECT = 0x2030;

		internal const int ERROR_DS_ALIAS_PROBLEM = 0x2031;

		internal const int ERROR_DS_INVALID_DN_SYNTAX = 0x2032;

		internal const int ERROR_DS_ALIAS_DEREF_PROBLEM = 0x2034;

		internal const int ERROR_DS_UNWILLING_TO_PERFORM = 0x2035;

		internal const int ERROR_DS_LOOP_DETECT = 0x2036;

		internal const int ERROR_DS_NAMING_VIOLATION = 0x2037;

		internal const int ERROR_DS_OBJECT_RESULTS_TOO_LARGE = 0x2038;

		internal const int ERROR_DS_AFFECTS_MULTIPLE_DSAS = 0x2039;

		internal const int ERROR_DS_NOT_SUPPORTED = 0x2040;

		internal const int ERROR_DS_ILLEGAL_MOD_OPERATION = 0x2077;

		internal const int ERROR_DS_SINGLE_VALUE_CONSTRAINT = 0x2081;

		internal const int ERROR_DS_OBJ_NOT_FOUND = 0x208d;

		internal const int ERROR_DS_GENERIC_ERROR = 0x2095;

		private ExceptionHelper()
		{
		}

		public static Exception GetExceptionFromErrorCode(int errorCode, string errorMessage, Exception innerException)
		{
			return ExceptionHelper.GetExceptionFromErrorCode(errorCode, errorMessage, null, innerException);
		}

		public static Exception GetExceptionFromErrorCode(int errorCode, string errorMessage, string serverErrorMessage, Exception innerException)
		{
			int num = errorCode;
			if (num > 0x52e)
			{
				if (num > 0x2030)
				{
					if (num == 0x2035 || num == 0x2037)
					{
						return new ADInvalidOperationException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
					else if (num == 0x2036)
					{
						return new ADException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
					if (num == 0x2077)
					{
						return new ADIllegalModifyOperationException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
					else
					{
						if (num == 0x208d)
						{
							return new ADIdentityNotFoundException(errorMessage, innerException);
						}
						return new ADException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
				}
				else
				{
					if (num == 0x1392)
					{
						return new ADIdentityAlreadyExistsException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
					if (num != 0x2014)
					{
						if (num == 0x202f)
						{
							return new ADInvalidOperationException(errorMessage, innerException, errorCode, serverErrorMessage);
						}
						else if (num == 0x2030)
						{
							return new ADIdentityNotFoundException(errorMessage, innerException);
						}
						return new ADException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
					else
					{
						return new ADInvalidOperationException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
				}
				return new ADIdentityNotFoundException(errorMessage, innerException);
			}
			else
			{
				if (num > 8)
				{
					if (num == 86)
					{
						return new ADInvalidPasswordException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
					else if (num == 87)
					{
						return new ADInvalidOperationException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
					if (num == 0x524)
					{
						return new ADIdentityAlreadyExistsException(errorMessage, innerException, errorCode, serverErrorMessage);
					}
					switch (num)
					{
						case 0x52d:
						{
							return new ADPasswordComplexityException(errorMessage, innerException, errorCode, serverErrorMessage);
						}
						case 0x52e:
						{
							return new AuthenticationException(errorMessage, innerException);
						}
					}
				}
				else
				{
					if (num == 0)
					{
						throw new ADException(StringResources.UnspecifiedError, innerException, serverErrorMessage);
					}
					else
					{
						if (num == 5)
						{
							return new UnauthorizedAccessException(errorMessage, innerException);
						}
						else
						{
							if (num == 8)
							{
								throw new ADException(StringResources.ServerOutOfMemory, innerException, errorMessage);
							}
						}
					}
				}
			}
			return new ADException(errorMessage, innerException, errorCode, serverErrorMessage);
		}
	}
}