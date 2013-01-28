namespace TaskScheduler
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, CompilerGenerated, TypeIdentifier, Guid("8CFAC062-A080-4C15-9A88-AA7C2AF80DFC"), DefaultMember("Path")]
    public interface ITaskFolder
    {
        void _VtblGap1_4();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(5)]
        ITaskFolder CreateFolder([In, MarshalAs(UnmanagedType.BStr)] string subFolderName, [In, Optional, MarshalAs(UnmanagedType.Struct)] object sddl);
        void _VtblGap2_1();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(7)]
        IRegisteredTask GetTask([In, MarshalAs(UnmanagedType.BStr)] string Path);
        void _VtblGap3_1();
        [DispId(9)]
        void DeleteTask([In, MarshalAs(UnmanagedType.BStr)] string Name, [In] int flags);
        void _VtblGap4_1();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(11)]
        IRegisteredTask RegisterTaskDefinition([In, MarshalAs(UnmanagedType.BStr)] string Path, [In, MarshalAs(UnmanagedType.Interface)] ITaskDefinition pDefinition, [In] int flags, [In, MarshalAs(UnmanagedType.Struct)] object UserId, [In, MarshalAs(UnmanagedType.Struct)] object password, [In] _TASK_LOGON_TYPE LogonType, [In, Optional, MarshalAs(UnmanagedType.Struct)] object sddl);
    }
}

