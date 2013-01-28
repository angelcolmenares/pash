namespace TaskScheduler
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, TypeIdentifier, Guid("6A67614B-6828-4FEC-AA54-6D52E8F1F2DB"), CompilerGenerated, DefaultMember("Item")]
    public interface IRunningTaskCollection : IEnumerable
    {
        int Count { [DispId(1)] get; }
    }
}

