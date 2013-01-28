using System;

namespace System.DirectoryServices.Protocols
{
	internal class ErrorChecking
	{
		public ErrorChecking()
		{
		}

		public static void CheckAndSetLdapError(int error)
		{
			string str;
			if (error == 0)
			{
				return;
			}
			else
			{
				if (!Utility.IsResultCode((ResultCode)error))
				{
					if (!Utility.IsLdapError((LdapError)error))
					{
						throw new LdapException(error);
					}
					else
					{
						str = LdapErrorMappings.MapResultCode(error);
						throw new LdapException(error, str);
					}
				}
				else
				{
					str = OperationErrorMappings.MapResultCode(error);
					throw new DirectoryOperationException(null, str);
				}
			}
		}
	}
}