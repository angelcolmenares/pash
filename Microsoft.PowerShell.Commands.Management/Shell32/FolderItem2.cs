namespace Shell32
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("EDC817AA-92B8-11D1-B075-00C04FC33AA5"), TypeIdentifier, DefaultMember("Name"), CompilerGenerated]
    public interface FolderItem2 : FolderItem
    {
        void _VtblGap1_2();
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0)] set; }
        string Path { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x60020004)] get; }
        void _VtblGap2_10();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(0x6002000f)]
        FolderItemVerbs Verbs();
        [DispId(0x60020010)]
        void InvokeVerb([In, Optional, MarshalAs(UnmanagedType.Struct)] object vVerb);
        void _VtblGap3_1();
        [return: MarshalAs(UnmanagedType.Struct)]
        [DispId(0x60030001)]
        object ExtendedProperty([In, MarshalAs(UnmanagedType.BStr)] string bstrPropName);
    }
}

