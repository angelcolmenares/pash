using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool QUERYCLIENTCERT(IntPtr Connection, IntPtr trusted_CAs, ref IntPtr certificateHandle);
}