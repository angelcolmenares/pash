namespace System.Management.Automation
{
    using System;

    [Flags]
    internal enum SerializationOptions
    {
        NoNamespace = 4,
        None = 0,
        NoObjectRefIds = 8,
        NoRootElement = 2,
        PreserveSerializationSettingOfOriginal = 0x10,
        RemotingOptions = 0x17,
        UseDepthFromTypes = 1
    }
}

