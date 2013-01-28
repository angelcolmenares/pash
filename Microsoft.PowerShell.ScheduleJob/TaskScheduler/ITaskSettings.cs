namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("8FD4711D-2D02-4C8C-87E3-EFF699DE127E"), CompilerGenerated, TypeIdentifier]
    public interface ITaskSettings
    {
        bool AllowDemandStart { [DispId(3)] get; [param: In] [DispId(3)] set; }
        void _VtblGap1_4();
        _TASK_INSTANCES_POLICY MultipleInstances { [DispId(6)] get; [param: In] [DispId(6)] set; }
        bool StopIfGoingOnBatteries { [DispId(7)] get; [param: In] [DispId(7)] set; }
        bool DisallowStartIfOnBatteries { [DispId(8)] get; [param: In] [DispId(8)] set; }
        void _VtblGap2_6();
        bool RunOnlyIfNetworkAvailable { [DispId(12)] get; [param: In] [DispId(12)] set; }
        void _VtblGap3_2();
        bool Enabled { [DispId(14)] get; [param: In] [DispId(14)] set; }
        void _VtblGap4_6();
        bool Hidden { [DispId(0x12)] get; [param: In] [DispId(0x12)] set; }
        IIdleSettings IdleSettings { [return: MarshalAs(UnmanagedType.Interface)] [DispId(0x13)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [DispId(0x13)] set; }
        bool RunOnlyIfIdle { [DispId(20)] get; [param: In] [DispId(20)] set; }
        bool WakeToRun { [DispId(0x15)] get; [param: In] [DispId(0x15)] set; }
    }
}

