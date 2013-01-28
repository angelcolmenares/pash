namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation.Internal;

    internal static class PSRemotingErrorInvariants
    {
        internal const string RemotingResourceBaseName = "RemotingErrorIdStrings";

        internal static string FormatResourceString(string resourceString, params object[] args)
        {
            return StringUtil.Format(resourceString, args);
        }
    }
}

