using System;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.Protocols
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int DEREFERENCECONNECTIONInternal(IntPtr Connection, IntPtr ConnectionToDereference);
}