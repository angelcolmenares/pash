namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using System;

    internal static class PdhLogAccess
    {
        public const long PDH_LOG_ACCESS_MASK = 0xf0000;
        public const long PDH_LOG_READ_ACCESS = 0x10000;
        public const long PDH_LOG_UPDATE_ACCESS = 0x40000;
        public const long PDH_LOG_WRITE_ACCESS = 0x20000;
    }
}

