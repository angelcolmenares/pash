namespace System.Management.Automation
{
    using System;

    internal static class RemotingConstants
    {
        internal static readonly string ComputerNameNoteProperty = "PSComputerName";
        internal const string DefaultShellName = "Microsoft.PowerShell";
        internal static readonly string EventObject = "PSEventObject";
        internal static readonly Version HostVersion = new Version(1, 0, 0, 0);
        internal const string MaxIdleTimeoutMS = "2147483647";
        internal static readonly Version ProtocolVersionCurrent = new Version(2, 2);
        internal static readonly Version ProtocolVersionWin7RC = new Version(2, 0);
        internal static readonly Version ProtocolVersionWin7RTM = new Version(2, 1);
        internal static readonly Version ProtocolVersionWin8RTM = new Version(2, 2);
		internal static readonly Version ProtocolVersion = ProtocolVersionCurrent;
        internal const string PSPluginDLLName = "pwrshplugin.dll";
        internal const string PSRemotingNoun = "PSRemoting";
        internal const string PSSessionConfigurationNoun = "PSSessionConfiguration";
        internal static readonly string RunspaceIdNoteProperty = "RunspaceId";
        internal static readonly string ShowComputerNameNoteProperty = "PSShowComputerName";
        internal static readonly string SourceJobInstanceId = "PSSourceJobInstanceId";
        internal static readonly string SourceLength = "Length";
    }
}

