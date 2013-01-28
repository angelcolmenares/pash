namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum SearchResolutionOptions
    {
        CommandNameIsPattern = 4,
        None = 0,
        ResolveAliasPatterns = 1,
        ResolveFunctionPatterns = 2,
        SearchAllScopes = 8
    }
}

