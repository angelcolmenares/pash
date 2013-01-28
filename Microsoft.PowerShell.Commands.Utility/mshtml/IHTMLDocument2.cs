namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, CompilerGenerated, TypeIdentifier, Guid("332C4425-26CB-11D0-B483-00C04FD90119")]
    public interface IHTMLDocument2 : IHTMLDocument
    {
        void _VtblGap1_1();
        IHTMLElementCollection all { [return: MarshalAs(UnmanagedType.Interface)] [DispId(0x3eb)] get; }
        IHTMLElement body { [return: MarshalAs(UnmanagedType.Interface)] [DispId(0x3ec)] get; }
        void _VtblGap2_1();
        IHTMLElementCollection images { [return: MarshalAs(UnmanagedType.Interface)] [DispId(0x3f3)] get; }
        void _VtblGap3_1();
        IHTMLElementCollection links { [return: MarshalAs(UnmanagedType.Interface)] [DispId(0x3f1)] get; }
        IHTMLElementCollection forms { [return: MarshalAs(UnmanagedType.Interface)] [DispId(0x3f2)] get; }
        void _VtblGap4_3();
        IHTMLElementCollection scripts { [return: MarshalAs(UnmanagedType.Interface)] [DispId(0x3f5)] get; }
        void _VtblGap5_3();
        string readyState { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3fa)] get; }
        void _VtblGap6_36();
        [DispId(0x41e)]
        void write([In, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType=VarEnum.VT_VARIANT)] params object[] psarray);
        void _VtblGap7_2();
        [DispId(0x421)]
        void close();
    }
}

