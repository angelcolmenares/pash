namespace TaskScheduler
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, DefaultMember("Path"), Guid("9C86F320-DEE3-4DD1-B972-A303F26B061E"), CompilerGenerated, TypeIdentifier]
    public interface IRegisteredTask
    {
        void _VtblGap1_7();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(7)]
        IRunningTaskCollection GetInstances([In] int flags);
        void _VtblGap2_4();
        ITaskDefinition Definition { [return: MarshalAs(UnmanagedType.Interface)] [DispId(13)] get; }
        void _VtblGap3_3();
        [DispId(0x11)]
        void Stop([In] int flags);
    }
}

