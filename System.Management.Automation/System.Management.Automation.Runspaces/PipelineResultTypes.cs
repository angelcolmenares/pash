namespace System.Management.Automation.Runspaces
{
    using System;

    [Flags]
    public enum PipelineResultTypes
    {
        None,
        Output,
        Error,
        Warning,
        Verbose,
        Debug,
        All,
        Null
    }
}

