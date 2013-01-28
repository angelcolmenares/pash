namespace Microsoft.Data.OData.Query
{
    using System;

    internal enum ExpressionTokenKind
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
        DateTimeOffsetLiteral,
        TimeLiteral,
        DecimalLiteral,
        DoubleLiteral,
        GuidLiteral,
        BinaryLiteral,
        GeographyLiteral,
        GeometryLiteral,
        Exclamation,
        OpenParen,
        CloseParen,
        Comma,
        Minus,
        Slash,
        Question,
        Dot,
        Star
    }
}

