namespace Shell32
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, CompilerGenerated, TypeIdentifier, Guid("EFD84B2D-4BCF-4298-BE25-EB542A59FBDA")]
    public interface IShellDispatch4 : IShellDispatch3, IShellDispatch2, IShellDispatch
    {
        void _VtblGap1_2();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(0x60020002)]
        Folder NameSpace([In, MarshalAs(UnmanagedType.Struct)] object vDir);
    }
}

