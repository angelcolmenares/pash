namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F5D2-98B5-11CF-BB82-00AA00BDCE0B"), TypeIdentifier, CompilerGenerated]
    public interface IHTMLInputElement
    {
        void _VtblGap1_2();
        string value { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147413011)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(-2147413011)] set; }
        string name { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(-2147418112)] set; }
    }
}

