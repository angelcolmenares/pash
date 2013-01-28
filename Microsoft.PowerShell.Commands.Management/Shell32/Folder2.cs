namespace Shell32
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, Guid("F0D2D8EF-3890-11D2-BF8B-00C04FB93661"), CompilerGenerated, DefaultMember("Title")]
    public interface Folder2 : Folder
    {
        void _VtblGap1_4();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(0x60020004)]
        FolderItems Items();
    }
}

