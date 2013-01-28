using System;

namespace System.Runtime.Diagnostics
{
	internal enum EventSeverity : uint
	{
		Success = 0,
		Informational = 1073741824,
		Warning = 2147483648,
		Error = 3221225472
	}
}