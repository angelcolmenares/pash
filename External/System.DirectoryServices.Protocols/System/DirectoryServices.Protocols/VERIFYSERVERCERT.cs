using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool VERIFYSERVERCERT(IntPtr Connection, IntPtr pServerCert);
}