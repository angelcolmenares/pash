namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum DeserializationOptions
    {
        DeserializeScriptBlocks = 0x400,
        NoNamespace = 0x200,
        None = 0,
        NoRootElement = 0x100,
        RemotingOptions = 0x300
    }
}

