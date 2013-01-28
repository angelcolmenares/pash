namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIDispatch), Guid("3050F613-98B5-11CF-BB82-00AA00BDCE0B"), CompilerGenerated, TypeIdentifier]
    public interface HTMLDocumentEvents2
    {
        void _VtblGap1_11();
        [PreserveSig, DispId(-609)]
        void onreadystatechange([In, MarshalAs(UnmanagedType.Interface)] IHTMLEventObj pEvtObj);
    }
}

