namespace TaskScheduler
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, DefaultMember("Item"), Guid("02820E19-7B98-4ED2-B2E8-FDCCCEFF619B"), CompilerGenerated]
    public interface IActionCollection : IEnumerable
    {
        void _VtblGap1_5();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(3)]
        IAction Create([In] _TASK_ACTION_TYPE Type);
        void _VtblGap2_1();
        [DispId(5)]
        void Clear();
    }
}

