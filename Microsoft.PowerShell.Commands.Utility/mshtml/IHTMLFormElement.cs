namespace mshtml
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.CustomMarshalers;

    [ComImport, TypeIdentifier, Guid("3050F1F7-98B5-11CF-BB82-00AA00BDCE0B"), DefaultMember("item"), CompilerGenerated]
    public interface IHTMLFormElement : IEnumerable
    {
        string action { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3e9)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3e9)] set; }
        void _VtblGap1_4();
        string method { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x3ec)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x3ec)] set; }
        void _VtblGap2_3();
        string name { [return: MarshalAs(UnmanagedType.BStr)] [DispId(-2147418112)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(-2147418112)] set; }
        void _VtblGap3_8();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [DispId(-4)]
        IEnumerator GetEnumerator();
    }
}

