namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using System;

    internal static class PdhLogOpenMode
    {
        public const long PDH_LOG_CREATE_ALWAYS = 2;
        public const long PDH_LOG_CREATE_MASK = 15;
        public const long PDH_LOG_CREATE_NEW = 1;
        public const long PDH_LOG_OPEN_ALWAYS = 3;
        public const long PDH_LOG_OPEN_EXISTING = 4;
    }
}

