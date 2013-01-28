namespace System.Management.Automation
{
    using System;

    public enum PSTokenType
    {
        Unknown,
        Command,
        CommandParameter,
        CommandArgument,
        Number,
        String,
        Variable,
        Member,
        LoopLabel,
        Attribute,
        Type,
        Operator,
        GroupStart,
        GroupEnd,
        Keyword,
        Comment,
        StatementSeparator,
        NewLine,
        LineContinuation,
        Position
    }
}

