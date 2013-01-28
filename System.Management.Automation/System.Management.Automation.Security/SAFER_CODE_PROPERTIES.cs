namespace System.Management.Automation.Security
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SAFER_CODE_PROPERTIES
    {
        public int cbSize;
        public int dwCheckFlags;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ImagePath;
        public IntPtr hImageFileHandle;
        public int UrlZoneId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x40, ArraySubType=UnmanagedType.I1)]
        public byte[] ImageHash;
        public int dwImageHashSize;
        public LARGE_INTEGER ImageSize;
        public int HashAlgorithm;
        public IntPtr pByteBlock;
        public IntPtr hWndParent;
        public int dwWVTUIChoice;
    }
}

