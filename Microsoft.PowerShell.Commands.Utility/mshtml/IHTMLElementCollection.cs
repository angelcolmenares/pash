namespace mshtml
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.CustomMarshalers;

    [ComImport, Guid("3050F21F-98B5-11CF-BB82-00AA00BDCE0B"), DefaultMember("item"), TypeIdentifier, CompilerGenerated]
    public interface IHTMLElementCollection : IEnumerable
    {
        void _VtblGap1_3();
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalType="", MarshalTypeRef=typeof(EnumeratorToEnumVariantMarshaler), MarshalCookie="")]
        [DispId(-4)]
        IEnumerator GetEnumerator();
    }
}

