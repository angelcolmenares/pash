namespace System.Management.Automation.Remoting
{
    using System;
    using System.Management.Automation;

    internal static class RemoteHostExceptions
    {
        internal static Exception NewDecodingErrorForErrorRecordException()
        {
            return new PSRemotingDataStructureException(RemotingErrorIdStrings.DecodingErrorForErrorRecord);
        }

        internal static Exception NewDecodingFailedException()
        {
            return new PSRemotingDataStructureException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemoteHostDecodingFailed, new object[0]));
        }

        internal static Exception NewNotImplementedException(RemoteHostMethodId methodId)
        {
            RemoteHostMethodInfo info = RemoteHostMethodInfo.LookUp(methodId);
            return new PSRemotingDataStructureException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemoteHostMethodNotImplemented, new object[] { info.Name }), new PSNotImplementedException());
        }

        internal static Exception NewNullClientHostException()
        {
            return new PSRemotingDataStructureException(RemotingErrorIdStrings.RemoteHostNullClientHost);
        }

        internal static Exception NewRemoteHostCallFailedException(RemoteHostMethodId methodId)
        {
            RemoteHostMethodInfo info = RemoteHostMethodInfo.LookUp(methodId);
            return new PSRemotingDataStructureException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemoteHostCallFailed, new object[] { info.Name }));
        }

        internal static Exception NewRemoteHostDataDecodingNotSupportedException(Type type)
        {
            return new PSRemotingDataStructureException(RemotingErrorIdStrings.RemoteHostDataDecodingNotSupported, new object[] { type.ToString() });
        }

        internal static Exception NewRemoteHostDataEncodingNotSupportedException(Type type)
        {
            return new PSRemotingDataStructureException(RemotingErrorIdStrings.RemoteHostDataEncodingNotSupported, new object[] { type.ToString() });
        }

        internal static Exception NewRemoteRunspaceDoesNotSupportPushRunspaceException()
        {
            return new PSRemotingDataStructureException(PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.RemoteRunspaceDoesNotSupportPushRunspace, new object[0]));
        }

        internal static Exception NewUnknownTargetClassException(string className)
        {
            return new PSRemotingDataStructureException(RemotingErrorIdStrings.UnknownTargetClass, new object[] { className });
        }
    }
}

