namespace Shell32
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.CustomMarshalers;

    [ComImport, CompilerGenerated, TypeIdentifier, Guid("1F8352C0-50B0-11CF-960C-0080C7F4EE85")]
    public interface FolderItemVerbs : IEnumerable
    {
        void _VtblGap1_4();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [DispId(-4)]
        IEnumerator GetEnumerator();
    }
}

