namespace System.Management.Automation.Security
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal struct LARGE_INTEGER
    {
        [FieldOffset(0)]
        public long QuadPart;
        [FieldOffset(0)]
        public Anonymous_9320654f_2227_43bf_a385_74cc8c562686 Struct1;
        [FieldOffset(0)]
        public Anonymous_947eb392_1446_4e25_bbd4_10e98165f3a9 u;
    }
}

