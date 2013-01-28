using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Options.Internal
{
	internal static class MiFlagsExtensionMethods
	{
		public static CimFlags ToCimFlags(this MiFlags miFlags)
		{
			return (CimFlags)((ulong)miFlags);
		}
	}
}