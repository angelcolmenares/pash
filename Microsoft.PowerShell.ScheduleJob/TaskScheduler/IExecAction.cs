namespace TaskScheduler
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, CompilerGenerated, Guid("4C3D624D-FD6B-49A3-B9B7-09CB3CD3F047"), TypeIdentifier]
    public interface IExecAction : IAction
    {
        string Id { [return: MarshalAs(UnmanagedType.BStr)] [DispId(1)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(1)] set; }
        void _VtblGap1_1();
        string Path { [return: MarshalAs(UnmanagedType.BStr)] [DispId(10)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(10)] set; }
        string Arguments { [return: MarshalAs(UnmanagedType.BStr)] [DispId(11)] get; [param: In, MarshalAs(UnmanagedType.BStr)] [DispId(11)] set; }
    }
}

