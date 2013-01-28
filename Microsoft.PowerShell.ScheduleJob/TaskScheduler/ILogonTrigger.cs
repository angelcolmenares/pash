namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, Guid("72DADE38-FAE4-4B3E-BAF4-5D009AF02B1C"), CompilerGenerated]
    public interface ILogonTrigger : ITrigger
    {
        void _VtblGap1_1();
        string Id { [return: MarshalAs(UnmanagedType.BStr)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(2)] set; }
        void _VtblGap2_8();
        bool Enabled { [DispId(7)] get; [param: In] [DispId(7)] set; }
        string Delay { [return: MarshalAs(UnmanagedType.BStr)] [DispId(20)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(20)] set; }
        string UserId { [return: MarshalAs(UnmanagedType.BStr)] [DispId(0x15)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(0x15)] set; }
    }
}

