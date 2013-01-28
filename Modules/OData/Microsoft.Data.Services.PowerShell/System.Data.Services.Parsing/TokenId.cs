namespace System.Data.Services.Parsing
{
    using System;

    internal enum TokenId
    {
        Unknown,
        End,
        Equal,
        Identifier,
        NullLiteral,
        BooleanLiteral,
        StringLiteral,
        IntegerLiteral,
        Int64Literal,
        SingleLiteral,
        DateTimeLiteral,
        DecimalLiteral,
        DoubleLiteral,
        GuidLiteral,
        BinaryLiteral,
        DateTimeOffsetLiteral,
        TimeLiteral,
        Exclamation,
        OpenParen,
        CloseParen,
        Comma,
        Minus,
        Slash,
        Question,
        Dot,
        Star,
        Colon,
        Semicolon,
        GeographylLiteral,
        GeometryLiteral,
        WhiteSpace
    }
}

