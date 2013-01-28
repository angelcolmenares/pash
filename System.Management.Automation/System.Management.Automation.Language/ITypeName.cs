namespace System.Management.Automation.Language
{
    using System;

    public interface ITypeName
    {
        Type GetReflectionAttributeType();
        Type GetReflectionType();

        string AssemblyName { get; }

        IScriptExtent Extent { get; }

        string FullName { get; }

        bool IsArray { get; }

        bool IsGeneric { get; }

        string Name { get; }
    }
}

