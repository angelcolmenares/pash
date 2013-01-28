namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum HelpCategory
    {
        Alias = 1,
        All = 0xffff,
        Cmdlet = 2,
        DefaultHelp = 0x1000,
        ExternalScript = 0x800,
        FAQ = 0x20,
        Filter = 0x400,
        Function = 0x200,
        General = 0x10,
        Glossary = 0x40,
        HelpFile = 0x80,
        None = 0,
        Provider = 4,
        ScriptCommand = 0x100,
        Workflow = 0x2000
    }
}

