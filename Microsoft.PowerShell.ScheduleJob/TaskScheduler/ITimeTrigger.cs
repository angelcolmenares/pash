namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, CompilerGenerated, Guid("B45747E0-EBA7-4276-9F29-85C5BB300006")]
    public interface ITimeTrigger : ITrigger
    {
        void _VtblGap1_1();
        string Id { [return: MarshalAs(UnmanagedType.BStr)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(2)] set; }
        IRepetitionPattern Repetition { [return: MarshalAs(UnmanagedType.Interface)] [DispId(3)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [DispId(3)] set; }
        void _VtblGap2_2();
        string StartBoundary { [return: MarshalAs(UnmanagedType.BStr)] [DispId(5)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(5)] set; }
        void _VtblGap3_2();
        bool Enabled { [DispId(7)] get; [param: In] [DispId(7)] set; }
        string RandomDelay { [return: MarshalAs(UnmanagedType.BStr)] [DispId(20)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(20)] set; }
    }
}

