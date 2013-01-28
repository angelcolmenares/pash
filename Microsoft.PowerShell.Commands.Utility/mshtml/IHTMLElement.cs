namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F1FF-98B5-11CF-BB82-00AA00BDCE0B"), TypeIdentifier, CompilerGenerated]
    public interface IHTMLElement
    {
        void _VtblGap1_5();
        string id { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147417110)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(-2147417110)] set; }
        string tagName { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147417108)] get; }
        void _VtblGap2_42();
        string innerHTML { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147417086)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(-2147417086)] set; }
        string innerText { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147417085)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(-2147417085)] set; }
        string outerHTML { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147417084)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(-2147417084)] set; }
        string outerText { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147417083)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(-2147417083)] set; }
    }
}

