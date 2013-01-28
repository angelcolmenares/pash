namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Xml.Serialization;

    [Serializable, XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11"), GeneratedCode("xsd", "4.0.30319.17361")]
    public enum ConfirmImpact
    {
        None,
        Low,
        Medium,
        High
    }
}

