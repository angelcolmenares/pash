using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Principal;

namespace Microsoft.Management.PowerShellWebAccess
{
	internal static class NativeMethods
	{
		public const int NO_ERROR = 0;

		public const int ERROR_INSUFFICIENT_BUFFER = 122;

		public const int ERROR_NONE_MAPPED = 0x534;

		public const int ERROR_NOT_AUTHENTICATED = 0x4dc;

		public const int SEC_E_LOGON_DENIED = -2146893044;

		public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;

		public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;

		public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

		/*
		[DllImport("advapi32", CharSet=CharSet.Unicode)]
		public static extern bool ConvertSidToStringSid(byte[] pSID, out IntPtr ptrSid);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool ConvertStringSidToSid(string StringSid, out IntPtr ptrSid);
		*/

		public static bool ConvertSidToStringSid (byte[] pSID, out IntPtr ptrSid)
		{
			unsafe {
				var ident = new SecurityIdentifier (pSID, 0);
				ptrSid = GCHandle.ToIntPtr (GCHandle.Alloc (ident.Value));
			}
			return true;
		}


		public static bool ConvertStringSidToSid (string StringSid, out IntPtr ptrSid)
		{
			unsafe {
				var ident = new SecurityIdentifier (StringSid);
				byte[] pSID = new byte[ident.BinaryLength];
				ident.GetBinaryForm (pSID, 0);
				ptrSid = GCHandle.ToIntPtr (GCHandle.Alloc (pSID));
			}
			return true;
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		public static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, int dwMessageId, uint dwLanguageId, ref IntPtr lpBuffer, uint nSize, IntPtr Arguments);

		/*
		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int GetLengthSid(IntPtr pSID);
		*/

		public static int GetLengthSid(IntPtr pSID)
		{
			byte[] obj = (byte[])GCHandle.FromIntPtr (pSID).Target;
			return obj.Length;
		}

		/*
		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		public static extern IntPtr LocalFree(IntPtr hMem);
		*/

		public static IntPtr LocalFree(IntPtr hMem)
		{
			return hMem;
		}

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool LookupAccountName(string lpSystemName, string lpAccountName, byte[] Sid, ref int cbSid, StringBuilder ReferencedDomainName, ref int cchReferencedDomainName, out NativeMethods.SID_NAME_USE peUse);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		public static extern bool LookupAccountSid(string lpSystemName, byte[] Sid, StringBuilder lpName, ref int cchName, StringBuilder ReferencedDomainName, ref int cchReferencedDomainName, out NativeMethods.SID_NAME_USE peUse);

		[DllImport("Netapi32.dll", CharSet=CharSet.None)]
		public static extern int NetApiBufferFree(IntPtr Buffer);

		[DllImport("Netapi32.dll", CharSet=CharSet.Unicode)]
		public static extern int NetGetJoinInformation(string server, out IntPtr domain, out NativeMethods.NetJoinStatus status);

		public enum NetJoinStatus
		{
			NetSetupUnknownStatus,
			NetSetupUnjoined,
			NetSetupWorkgroupName,
			NetSetupDomainName
		}

		public enum SID_NAME_USE
		{
			SidTypeUser = 1,
			SidTypeGroup = 2,
			SidTypeDomain = 3,
			SidTypeAlias = 4,
			SidTypeWellKnownGroup = 5,
			SidTypeDeletedAccount = 6,
			SidTypeInvalid = 7,
			SidTypeUnknown = 8,
			SidTypeComputer = 9
		}
	}
}