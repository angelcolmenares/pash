namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Xml.Serialization;

    [GeneratedCode("sgen", "4.0")]
    internal abstract class XmlSerializer1 : XmlSerializer
    {
        protected XmlSerializer1()
        {
        }

        protected override XmlSerializationReader CreateReader()
        {
            return new XmlSerializationReader1();
        }

        protected override XmlSerializationWriter CreateWriter()
        {
            return new XmlSerializationWriter1();
        }
    }
}

