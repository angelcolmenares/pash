namespace Shell32
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.CustomMarshalers;

    [ComImport, TypeIdentifier, Guid("EAA7C309-BBEC-49D5-821D-64D966CB667F"), CompilerGenerated, DefaultMember("Verbs")]
    public interface FolderItems3 : FolderItems2, FolderItems, IEnumerable
    {
        void _VtblGap1_4();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [DispId(-4)]
        IEnumerator GetEnumerator();
    }
}

