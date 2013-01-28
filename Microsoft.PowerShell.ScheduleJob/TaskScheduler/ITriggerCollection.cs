namespace TaskScheduler
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.CustomMarshalers;

    [ComImport, DefaultMember("Item"), Guid("85DF5081-1B24-4F32-878A-D9D14DF4CB77"), CompilerGenerated, TypeIdentifier]
    public interface ITriggerCollection : IEnumerable
    {
        void _VtblGap1_2();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [DispId(-4)]
        IEnumerator GetEnumerator();
        [return: MarshalAs(UnmanagedType.Interface)]
        [DispId(2)]
        ITrigger Create([In] _TASK_TRIGGER_TYPE2 Type);
        void _VtblGap2_1();
        [DispId(5)]
        void Clear();
    }
}

