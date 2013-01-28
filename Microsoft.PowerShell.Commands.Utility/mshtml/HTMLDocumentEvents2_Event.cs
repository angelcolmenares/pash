namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [ComImport, Guid("3050F1C5-98B5-11CF-BB82-00AA00BDCE0B"), ComEventInterface(typeof(HTMLDocumentEvents2), typeof(HTMLDocumentEvents2)), CompilerGenerated, TypeIdentifier("3050F1C5-98B5-11CF-BB82-00AA00BDCE0B", "mshtml.HTMLDocumentEvents2_Event")]
    public interface HTMLDocumentEvents2_Event
    {
        event HTMLDocumentEvents2_onreadystatechangeEventHandler onreadystatechange;

        void _VtblGap1_22();
    }
}

