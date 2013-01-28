namespace System.Management.Automation
{
    using System;

    [Flags]
    public enum CommandTypes
    {
        Alias = 1,
        All = 0xff,
        Application = 0x20,
        Cmdlet = 8,
        ExternalScript = 0x10,
        Filter = 4,
        Function = 2,
        Script = 0x40,
        Workflow = 0x80
    }
}

