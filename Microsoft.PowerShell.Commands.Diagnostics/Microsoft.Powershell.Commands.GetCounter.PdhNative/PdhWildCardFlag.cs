namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using System;

    internal static class PdhWildCardFlag
    {
        public const long PDH_NOEXPANDCOUNTERS = 1;
        public const long PDH_NOEXPANDINSTANCES = 2;
        public const long PDH_REFRESHCOUNTERS = 4;
    }
}

