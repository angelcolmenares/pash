namespace Microsoft.PowerShell.Cmdletization.Xml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Xml.Serialization;

    [Serializable, GeneratedCode("xsd", "4.0.30319.17361"), XmlType(Namespace="http://schemas.microsoft.com/cmdlets-over-objects/2009/11", IncludeInSchema=false)]
    public enum ItemsChoiceType
    {
        ExcludeQuery,
        MaxValueQuery,
        MinValueQuery,
        RegularQuery
    }
}

