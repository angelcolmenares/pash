namespace System.Management.Automation.Language
{
    using System;

    public interface IScriptPosition
    {
        string GetFullScript();

        int ColumnNumber { get; }

        string File { get; }

        string Line { get; }

        int LineNumber { get; }

        int Offset { get; }
    }
}

