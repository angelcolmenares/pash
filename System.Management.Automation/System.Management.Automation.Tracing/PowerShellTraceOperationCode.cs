namespace System.Management.Automation.Tracing
{
    using System;

    public enum PowerShellTraceOperationCode
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
        None = 0,
        Open = 10,
        Receive = 0x16,
        Send = 0x15,
        SerializationSettings = 0x18,
        WinDCStart = 0x1c,
        WinDCStop = 0x1d,
        WinExtension = 30,
        WinInfo = 0x19,
        WinReply = 0x1f,
        WinResume = 0x20,
        WinStart = 0x1a,
        WinStop = 0x1b,
        WinSuspend = 0x21,
        WorkflowLoad = 0x17
    }
}

