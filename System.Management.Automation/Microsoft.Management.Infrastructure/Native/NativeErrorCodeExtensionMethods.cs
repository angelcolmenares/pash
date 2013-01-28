using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Native;

namespace Microsoft.Management.Infrastructure.Internal
{
    internal static class NativeErrorCodeExtensionMethods
    {
        public static NativeErrorCode ToNativeErrorCode(this MiResult miResult)
        {
            return (NativeErrorCode)miResult;
        }
    }
}