namespace System.Management.Automation
{
    using System;
    using System.Xml;

    public abstract class PSControl
    {
        protected PSControl()
        {
        }

        internal abstract bool SafeForExport();
        internal abstract void WriteToXML(XmlWriter _writer, bool exportScriptBlock);
    }
}

