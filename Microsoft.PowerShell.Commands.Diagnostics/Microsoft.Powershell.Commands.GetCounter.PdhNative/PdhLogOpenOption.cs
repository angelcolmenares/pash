namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using System;

    internal static class PdhLogOpenOption
    {
        public const long PDH_LOG_OPT_APPEND = 0x8000000;
        public const long PDH_LOG_OPT_CIRCULAR = 0x2000000;
        public const long PDH_LOG_OPT_MAX_IS_BYTES = 0x4000000;
        public const long PDH_LOG_OPT_USER_STRING = 0x1000000;
    }
}

