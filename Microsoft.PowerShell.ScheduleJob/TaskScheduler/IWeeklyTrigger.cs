namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, CompilerGenerated, Guid("5038FC98-82FF-436D-8728-A512A57C9DC1")]
    public interface IWeeklyTrigger : ITrigger
    {
        void _VtblGap1_1();
        string Id { [return: MarshalAs(UnmanagedType.BStr)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(2)] set; }
        void _VtblGap2_4();
        string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] [DispId(5)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(5)] set; }
        void _VtblGap3_2();
        bool Enabled { [DispId(7)] get; [param: In] [DispId(7)] set; }
        short DaysOfWeek { [DispId(0x19)] get; [param: In] [DispId(0x19)] set; }
        short WeeksInterval { [DispId(0x1a)] get; [param: In] [DispId(0x1a)] set; }
        string RandomDelay { [return: MarshalAs(UnmanagedType.BStr)] [DispId(20)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(20)] set; }
    }
}

