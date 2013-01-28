namespace Shell32
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, Guid("08EC3E00-50B0-11CF-960C-0080C7F4EE85"), CompilerGenerated, DefaultMember("Name")]
    public interface FolderItemVerb
    {
        void _VtblGap1_2();
        string Name { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0)] get; }
    }
}

