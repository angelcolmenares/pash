using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Authentication;
using System.Text;

namespace System.DirectoryServices.AccountManagement
{
	internal class ExceptionHelper
	{
		private static int ERROR_NOT_ENOUGH_MEMORY;

		private static int ERROR_OUTOFMEMORY;

		private static int ERROR_DS_DRA_OUT_OF_MEM;

		private static int ERROR_NO_SUCH_DOMAIN;

		private static int ERROR_ACCESS_DENIED;

		private static int ERROR_NO_LOGON_SERVERS;

		private static int ERROR_DS_DRA_ACCESS_DENIED;

		private static int RPC_S_OUT_OF_RESOURCES;

		internal static int RPC_S_SERVER_UNAVAILABLE;

		internal static int RPC_S_CALL_FAILED;

		public static int ERROR_HRESULT_LOGON_FAILURE;

		public static int ERROR_HRESULT_CONSTRAINT_VIOLATION;

		public static int ERROR_LOGON_FAILURE;

		static ExceptionHelper()
		{
			ExceptionHelper.ERROR_NOT_ENOUGH_MEMORY = 8;
			ExceptionHelper.ERROR_OUTOFMEMORY = 14;
			ExceptionHelper.ERROR_DS_DRA_OUT_OF_MEM = 0x20fe;
			ExceptionHelper.ERROR_NO_SUCH_DOMAIN = 0x54b;
			ExceptionHelper.ERROR_ACCESS_DENIED = 5;
			ExceptionHelper.ERROR_NO_LOGON_SERVERS = 0x51f;
			ExceptionHelper.ERROR_DS_DRA_ACCESS_DENIED = 0x2105;
			ExceptionHelper.RPC_S_OUT_OF_RESOURCES = 0x6b9;
			ExceptionHelper.RPC_S_SERVER_UNAVAILABLE = 0x6ba;
			ExceptionHelper.RPC_S_CALL_FAILED = 0x6be;
			ExceptionHelper.ERROR_HRESULT_LOGON_FAILURE = -2147023570;
			ExceptionHelper.ERROR_HRESULT_CONSTRAINT_VIOLATION = -2147016657;
			ExceptionHelper.ERROR_LOGON_FAILURE = 49;
		}

		private ExceptionHelper()
		{
		}

		[SecurityCritical]
		internal static string GetErrorMessage(int errorCode, bool hresult)
		{
			string str;
			int num = errorCode;
			if (!hresult)
			{
				num = num & 0xffff | 0x70000 | -2147483648;
			}
			StringBuilder stringBuilder = new StringBuilder(0x100);
			int num1 = UnsafeNativeMethods.FormatMessageW(0x3200, IntPtr.Zero, num, 0, stringBuilder, stringBuilder.Capacity + 1, IntPtr.Zero);
			if (num1 == 0)
			{
				str = string.Concat(StringResources.DSUnknown, Convert.ToString((ulong)num, 16));
			}
			else
			{
				str = stringBuilder.ToString(0, num1);
			}
			return str;
		}

		internal static Exception GetExceptionFromCOMException(COMException e)
		{
			Exception passwordException;
			int errorCode = e.ErrorCode;
			string message = e.Message;
			if (errorCode != -2147024891)
			{
				if (errorCode == -2147022651 || errorCode == -2147024810 || errorCode == 0x8007052)
				{
					passwordException = new PasswordException(message, e);
				}
				else
				{
					if (errorCode == -2147022672 || errorCode == -2147019886)
					{
						passwordException = new PrincipalExistsException(message, e);
					}
					else
					{
						if (errorCode != -2147023570)
						{
							if (errorCode != -2147016657)
							{
								if (errorCode != -2147016651)
								{
									if (errorCode != -2147024888)
									{
										if (errorCode == -2147016646 || errorCode == -2147016690 || errorCode == -2147016689)
										{
											passwordException = new PrincipalServerDownException(message, e, errorCode, null);
										}
										else
										{
											passwordException = new PrincipalOperationException(message, e, errorCode);
										}
									}
									else
									{
										passwordException = new OutOfMemoryException();
									}
								}
								else
								{
									passwordException = new InvalidOperationException(message, e);
								}
							}
							else
							{
								passwordException = new InvalidOperationException(message, e);
							}
						}
						else
						{
							passwordException = new AuthenticationException(message, e);
						}
					}
				}
			}
			else
			{
				passwordException = new UnauthorizedAccessException(message, e);
			}
			return passwordException;
		}

		[SecuritySafeCritical]
		internal static Exception GetExceptionFromErrorCode(int errorCode)
		{
			return ExceptionHelper.GetExceptionFromErrorCode(errorCode, null);
		}

		[SecurityCritical]
		internal static Exception GetExceptionFromErrorCode(int errorCode, string targetName)
		{
			string errorMessage = ExceptionHelper.GetErrorMessage(errorCode, false);
			if (errorCode == ExceptionHelper.ERROR_ACCESS_DENIED || errorCode == ExceptionHelper.ERROR_DS_DRA_ACCESS_DENIED)
			{
				return new UnauthorizedAccessException(errorMessage);
			}
			else
			{
				if (errorCode == ExceptionHelper.ERROR_NOT_ENOUGH_MEMORY || errorCode == ExceptionHelper.ERROR_OUTOFMEMORY || errorCode == ExceptionHelper.ERROR_DS_DRA_OUT_OF_MEM || errorCode == ExceptionHelper.RPC_S_OUT_OF_RESOURCES)
				{
					return new OutOfMemoryException();
				}
				else
				{
					if (errorCode == ExceptionHelper.ERROR_NO_LOGON_SERVERS || errorCode == ExceptionHelper.ERROR_NO_SUCH_DOMAIN || errorCode == ExceptionHelper.RPC_S_SERVER_UNAVAILABLE || errorCode == ExceptionHelper.RPC_S_CALL_FAILED)
					{
						return new PrincipalServerDownException(errorMessage, errorCode);
					}
					else
					{
						return new PrincipalOperationException(errorMessage, errorCode);
					}
				}
			}
		}
	}
}