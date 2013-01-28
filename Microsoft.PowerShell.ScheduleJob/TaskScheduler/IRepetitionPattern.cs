namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, Guid("7FB9ACF1-26BE-400E-85B5-294B9C75DFD6"), CompilerGenerated]
    public interface IRepetitionPattern
    {
        string Interval { [return: MarshalAs(UnmanagedType.BStr)] [DispId(1)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(1)] set; }
        string Duration { [return: MarshalAs(UnmanagedType.BStr)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(2)] set; }
        bool StopAtDurationEnd { [DispId(3)] get; [param: In] [DispId(3)] set; }
    }
}

