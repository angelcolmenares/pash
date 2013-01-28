namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, CompilerGenerated, Guid("2A9C35DA-D357-41F4-BBC1-207AC1B1F3CB"), TypeIdentifier]
    public interface IBootTrigger : ITrigger
    {
        void _VtblGap1_1();
        string Id { [return: MarshalAs(UnmanagedType.BStr)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(2)] set; }
        void _VtblGap2_8();
        bool Enabled { [DispId(7)] get; [param: In] [DispId(7)] set; }
        string Delay { [return: MarshalAs(UnmanagedType.BStr)] [DispId(20)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(20)] set; }
    }
}

