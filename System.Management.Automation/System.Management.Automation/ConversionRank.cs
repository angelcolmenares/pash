namespace System.Management.Automation
{
    using System;

    internal enum ConversionRank
    {
        Assignable = 0x10f,
        AssignableS2A = 0x107,
        Constructor = 0x6f,
        ConstructorS2A = 0x67,
        Create = 0x73,
        Custom = 0x2f,
        CustomS2A = 0x27,
        ExplicitCast = 0x5f,
        ExplicitCastS2A = 0x57,
        IConvertible = 0x3f,
        IConvertibleS2A = 0x37,
        Identity = 0x11f,
        IdentityS2A = 0x117,
        ImplicitCast = 0x4f,
        ImplicitCastS2A = 0x47,
        Language = 0x9f,
        LanguageS2A = 0x97,
        None = 0,
        NullToRef = 0xbf,
        NullToValue = 0xaf,
        NumericExplicit = 0xcf,
        NumericExplicit1 = 0xdf,
        NumericExplicit1S2A = 0xd7,
        NumericExplicitS2A = 0xc7,
        NumericImplicit = 0xff,
        NumericImplicitS2A = 0xf7,
        NumericString = 0xef,
        NumericStringS2A = 0xe7,
        Parse = 0x7f,
        ParseS2A = 0x77,
        PSObject = 0x8f,
        PSObjectS2A = 0x87,
        StringToCharArray = 0x11a,
        ToString = 0x1f,
        ToStringS2A = 0x17,
        UnrelatedArrays = 15,
        UnrelatedArraysS2A = 7,
        ValueDependent = 0xfff7
    }
}

