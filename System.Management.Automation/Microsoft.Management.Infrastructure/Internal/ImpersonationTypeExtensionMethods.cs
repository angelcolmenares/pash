using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;

namespace Microsoft.Management.Infrastructure.Options.Internal
{
	internal static class ImpersonationTypeExtensionMethods
	{
		public static DestinationOptionsMethods.MiImpersonationType ToNativeType(this ImpersonationType impersonationType)
		{
			return (DestinationOptionsMethods.MiImpersonationType)impersonationType;
		}
	}
}