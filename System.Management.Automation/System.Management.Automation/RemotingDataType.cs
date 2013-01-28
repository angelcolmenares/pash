namespace System.Management.Automation
{
    using System;

    internal enum RemotingDataType : int
    {
        ApplicationPrivateData = 0x21009,
        AvailableRunspaces = 0x21007,
        CloseSession = 0x10003,
        ConnectRunspacePool = 0x10008,
        CreatePowerShell = 0x21006,
        CreateRunspacePool = 0x10004,
        EncryptedSessionKey = 0x10006,
        ExceptionAsErrorRecord = 1,
        GetCommandMetadata = 0x2100a,
        InvalidDataType = 0,
        PowerShellDebug = 0x41007,
        PowerShellErrorRecord = 0x41005,
        PowerShellInput = 0x41002,
        PowerShellInputEnd = 0x41003,
        PowerShellOutput = 0x41004,
        PowerShellProgress = 0x41010,
        PowerShellStateInfo = 0x41006,
        PowerShellVerbose = 0x41008,
        PowerShellWarning = 0x41009,
        PSEventArgs = 0x21008,
        PublicKey = 0x10005,
        PublicKeyRequest = 0x10007,
        RemoteHostCallUsingPowerShellHost = 0x41100,
        RemoteHostCallUsingRunspaceHost = 0x21100,
        RemotePowerShellHostResponseData = 0x41101,
        RemoteRunspaceHostResponseData = 0x21101,
        RunspacePoolInitData = 0x2100b,
        RunspacePoolOperationResponse = 0x21004,
        RunspacePoolStateInfo = 0x21005,
        SessionCapability = 0x10002,
        SetMaxRunspaces = 0x21002,
        SetMinRunspaces = 0x21003,
        StopPowerShell = 0x41012
    }
}

