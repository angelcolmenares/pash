using System;

namespace System.DirectoryServices.AccountManagement
{
	internal class Constants
	{
		internal static byte[] GUID_USERS_CONTAINER_BYTE;

		internal static byte[] GUID_COMPUTRS_CONTAINER_BYTE;

		internal static byte[] GUID_FOREIGNSECURITYPRINCIPALS_CONTAINER_BYTE;

		static Constants()
		{
			byte[] numArray = new byte[] { 169, 209, 202, 21, 118, 136, 17, 209, 173, 237, 0, 192, 79, 216, 213, 205 };
			Constants.GUID_USERS_CONTAINER_BYTE = numArray;
			byte[] numArray1 = new byte[] { 170, 49, 40, 37, 118, 136, 17, 209, 173, 237, 0, 192, 79, 216, 213, 205 };
			Constants.GUID_COMPUTRS_CONTAINER_BYTE = numArray1;
			byte[] numArray2 = new byte[] { 34, 183, 12, 103, 213, 110, 78, 251, 145, 233, 48, 15, 202, 61, 193, 170 };
			Constants.GUID_FOREIGNSECURITYPRINCIPALS_CONTAINER_BYTE = numArray2;
		}

		private Constants()
		{
		}
	}
}