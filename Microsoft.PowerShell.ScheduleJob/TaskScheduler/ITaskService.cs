namespace TaskScheduler
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, Guid("2FABA4C7-4DA9-4013-9697-20CC3FD40F85"), CompilerGenerated, DefaultMember("TargetServer")]
    public interface ITaskService
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(1)]
        ITaskFolder GetFolder([In, MarshalAs(UnmanagedType.BStr)] string Path);
        void _VtblGap1_1();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(3)]
        ITaskDefinition NewTask([In] uint flags);
        [DispId(4)]
        void Connect([In, Optional, MarshalAs(UnmanagedType.Struct)] object serverName, [In, Optional, MarshalAs(UnmanagedType.Struct)] object user, [In, Optional, MarshalAs(UnmanagedType.Struct)] object domain, [In, Optional, MarshalAs(UnmanagedType.Struct)] object password);
    }
}

