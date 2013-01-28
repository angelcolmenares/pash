using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;

namespace Microsoft.Management.Infrastructure.Options.Internal
{
	internal static class OperationFlagsExtensionMethods
	{
		public static MiOperationFlags ToNative(this CimOperationFlags operationFlags)
		{
			return (MiOperationFlags)((uint)operationFlags);
		}
	}
}