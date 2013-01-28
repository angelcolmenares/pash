namespace System.Management.Automation
{
    using System;
    using System.ComponentModel;

    [TypeConverter(typeof(LanguagePrimitives.EnumMultipleTypeConverter)), Flags]
    public enum PSMemberViewTypes
    {
        Adapted = 2,
        All = 7,
        Base = 4,
        Extended = 1
    }
}

