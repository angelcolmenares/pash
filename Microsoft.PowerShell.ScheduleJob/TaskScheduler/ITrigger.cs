namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, CompilerGenerated, TypeIdentifier, Guid("09941815-EA89-4B5B-89E0-2A773801FAC3")]
    public interface ITrigger
    {
        void _VtblGap1_1();
        string Id { [return: MarshalAs(UnmanagedType.BStr)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(2)] set; }
        void _VtblGap2_4();
        string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] [DispId(5)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(5)] set; }
        void _VtblGap3_2();
        bool Enabled { [DispId(7)] get; [param: In] [DispId(7)] set; }
    }
}

