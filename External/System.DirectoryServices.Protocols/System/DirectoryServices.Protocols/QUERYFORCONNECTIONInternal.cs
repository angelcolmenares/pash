using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int QUERYFORCONNECTIONInternal(IntPtr Connection, IntPtr ReferralFromConnection, IntPtr NewDNPtr, string HostName, int PortNumber, SEC_WINNT_AUTH_IDENTITY_EX SecAuthIdentity, Luid CurrentUserToken, ref ConnectionHandle ConnectionToUse);
}