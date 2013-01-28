namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("84594461-0053-4342-A8FD-088FABF11F32"), TypeIdentifier, CompilerGenerated]
    public interface IIdleSettings
    {
        string IdleDuration { [return: MarshalAs(UnmanagedType.BStr)] [DispId(1)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(1)] set; }
        string WaitTimeout { [return: MarshalAs(UnmanagedType.BStr)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(2)] set; }
        bool StopOnIdleEnd { [DispId(3)] get; [param: In] [DispId(3)] set; }
        bool RestartOnIdle { [DispId(4)] get; [param: In] [DispId(4)] set; }
    }
}

