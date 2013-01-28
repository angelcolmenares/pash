namespace System.Management.Automation.Internal
{
    using System;

    internal enum PSOpcode : byte
    {
        Close = 11,
        Connect = 12,
        Constructor = 0x10,
        Create = 15,
        Disconnect = 13,
        Dispose = 0x11,
        EventHandler = 0x12,
        Exception = 0x13,
        Method = 20,
        Negotiate = 14,
        Open = 10,
        Receive = 0x16,
        Rehydration = 0x17,
        Send = 0x15,
        SerializationSettings = 0x18,
        ShuttingDown = 0x19,
        WinStart = 1,
        WinStop = 2
    }
}

