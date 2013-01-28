namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, CompilerGenerated, Guid("126C5CD8-B288-41D5-8DBF-E491446ADC5C")]
    public interface IDailyTrigger : ITrigger
    {
        void _VtblGap1_1();
        string Id { [return: MarshalAs(UnmanagedType.BStr)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(2)] set; }
        void _VtblGap2_4();
        string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] [DispId(5)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(5)] set; }
        void _VtblGap3_2();
        bool Enabled { [DispId(7)] get; [param: In] [DispId(7)] set; }
        short DaysInterval { [DispId(0x19)] get; [param: In] [DispId(0x19)] set; }
        string RandomDelay { [return: MarshalAs(UnmanagedType.BStr)] [DispId(20)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(20)] set; }
    }
}

