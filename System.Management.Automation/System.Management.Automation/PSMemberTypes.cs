namespace System.Management.Automation
{
    using System;
    using System.ComponentModel;

    [TypeConverter(typeof(LanguagePrimitives.EnumMultipleTypeConverter)), Flags]
    public enum PSMemberTypes
    {
        AliasProperty = 1,
        All = 0x1fff,
        CodeMethod = 0x80,
        CodeProperty = 2,
        Dynamic = 0x1000,
        Event = 0x800,
        MemberSet = 0x400,
        Method = 0x40,
        Methods = 0x1c0,
        NoteProperty = 8,
        ParameterizedProperty = 0x200,
        Properties = 0x1f,
        Property = 4,
        PropertySet = 0x20,
        ScriptMethod = 0x100,
        ScriptProperty = 0x10
    }
}

