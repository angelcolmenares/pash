namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, CompilerGenerated, Guid("F5BC8FC5-536D-4F77-B852-FBC1356FDEB6")]
    public interface ITaskDefinition
    {
        void _VtblGap1_2();
        ITriggerCollection Triggers { [return: MarshalAs(UnmanagedType.Interface)] [DispId(2)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [DispId(2)] set; }
        ITaskSettings Settings { [return: MarshalAs(UnmanagedType.Interface)] [DispId(7)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [DispId(7)] set; }
        void _VtblGap2_2();
        IPrincipal Principal { [return: MarshalAs(UnmanagedType.Interface)] [DispId(12)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [DispId(12)] set; }
        IActionCollection Actions { [return: MarshalAs(UnmanagedType.Interface)] [DispId(13)] get; [param: In, MarshalAs(UnmanagedType.Interface)] [DispId(13)] set; }
    }
}

