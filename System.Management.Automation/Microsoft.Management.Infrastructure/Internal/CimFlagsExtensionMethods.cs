using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Options.Internal
{
	internal static class CimFlagsExtensionMethods
	{
		public static MiFlags ToMiFlags(this CimFlags cimFlags)
		{
			return (MiFlags)((uint)cimFlags);
		}
	}
}