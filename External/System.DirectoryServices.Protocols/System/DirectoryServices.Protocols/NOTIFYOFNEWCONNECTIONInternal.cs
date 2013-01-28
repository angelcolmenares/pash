using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool NOTIFYOFNEWCONNECTIONInternal(IntPtr Connection, IntPtr ReferralFromConnection, IntPtr NewDNPtr, string HostName, IntPtr NewConnection, int PortNumber, SEC_WINNT_AUTH_IDENTITY_EX SecAuthIdentity, Luid CurrentUser, int ErrorCodeFromBind);
}