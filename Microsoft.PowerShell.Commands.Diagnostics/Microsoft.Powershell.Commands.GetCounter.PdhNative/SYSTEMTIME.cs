namespace Microsoft.Powershell.Commands.GetCounter.PdhNative
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=0x10, Pack=2)]
    internal struct SYSTEMTIME
    {
        public ushort year;
        public ushort month;
        public ushort dayOfWeek;
        public ushort day;
        public ushort hour;
        public ushort minute;
        public ushort second;
        public ushort milliseconds;
    }
}

