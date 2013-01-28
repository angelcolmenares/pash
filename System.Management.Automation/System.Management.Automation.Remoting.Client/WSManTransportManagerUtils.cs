namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation.Remoting;

    internal static class WSManTransportManagerUtils
    {
        private static Dictionary<int, string> _transportErrorCodeToFQEID;

        static WSManTransportManagerUtils()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            dictionary.Add(5, "AccessDenied");
            dictionary.Add(14, "ServerOutOfMemory");
            dictionary.Add(0x35, "NetworkPathNotFound");
            dictionary.Add(-2144108103, "ComputerNotFound");
            dictionary.Add(0x51f, "AuthenticationFailed");
            dictionary.Add(0x52e, "LogonFailure");
            dictionary.Add(0x6ba, "ImproperResponse");
            dictionary.Add(-2141974624, "IncorrectProtocolVersion");
            dictionary.Add(-2144108250, "WinRMOperationTimeout");
            dictionary.Add(-2144108269, "URLNotAvailable");
            dictionary.Add(-2144108526, "CannotConnect");
            dictionary.Add(-2144108485, "InvalidResourceUri");
            dictionary.Add(-2144108083, "CannotConnectAlreadyConnected");
            dictionary.Add(-2144108274, "InvalidAuthentication");
            dictionary.Add(0x45b, "ShutDownInProgress");
            dictionary.Add(-2144108080, "CannotConnectInvalidOperation");
            dictionary.Add(-2144108090, "CannotConnectMismatchSessions");
            dictionary.Add(-2144108065, "CannotConnectRunAsFailed");
            dictionary.Add(-2144108094, "SessionCreateFailedInvalidName");
            dictionary.Add(-2144108453, "CannotConnectTargetSessionDoesNotExist");
            dictionary.Add(-2144108116, "RemoteSessionDisallowed");
            dictionary.Add(-2144108061, "RemoteConnectionDisallowed");
            dictionary.Add(-2144108542, "InvalidResourceUri");
            dictionary.Add(-2144108539, "CorruptedWinRMConfig");
            dictionary.Add(0x3e3, "WinRMOperationAborted");
            dictionary.Add(-2144108499, "URIExceedsMaxAllowedSize");
            dictionary.Add(-2144108318, "ClientKerberosDisabled");
            dictionary.Add(-2144108316, "ServerNotTrusted");
            dictionary.Add(-2144108276, "WorkgroupCannotUseKerberos");
            dictionary.Add(-2144108315, "ExplicitCredentialsRequired");
            dictionary.Add(-2144108105, "RedirectLocationInvalid");
            dictionary.Add(-2144108135, "RedirectInformationRequired");
            dictionary.Add(-2144108428, "WinRMOperationNotSupportedOnServer");
            dictionary.Add(-2144108270, "CannotConnectWinRMService");
            dictionary.Add(-2144108176, "WinRMHttpError");
            dictionary.Add(-2146893053, "TargetUnknown");
            dictionary.Add(-2144108101, "CannotUseIPAddress");
            _transportErrorCodeToFQEID = dictionary;
        }

        internal static TransportErrorOccuredEventArgs ConstructTransportErrorEventArgs(IntPtr wsmanAPIHandle, WSManClientSessionTransportManager wsmanSessionTM, WSManNativeApi.WSManError errorStruct, TransportMethodEnum transportMethodReportingError, string resourceString, params object[] resourceArgs)
        {
            PSRemotingTransportException exception;
            if ((errorStruct.errorCode == -2144108135) && (wsmanSessionTM != null))
            {
                string redirectLocation = WSManNativeApi.WSManGetSessionOptionAsString(wsmanSessionTM.SessionHandle, WSManNativeApi.WSManSessionOption.WSMAN_OPTION_REDIRECT_LOCATION);
                string str2 = ParseEscapeWSManErrorMessage(WSManNativeApi.WSManGetErrorMessage(wsmanAPIHandle, errorStruct.errorCode)).Trim();
                exception = new PSRemotingTransportRedirectException(redirectLocation, PSRemotingErrorId.URIEndPointNotResolved, RemotingErrorIdStrings.URIEndPointNotResolved, new object[] { str2, redirectLocation });
            }
            else if ((errorStruct.errorCode == -2144108485) && (wsmanSessionTM != null))
            {
                string str3 = wsmanSessionTM.ConnectionInfo.ShellUri.Replace("http://schemas.microsoft.com/powershell/", string.Empty);
                string str4 = PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.InvalidConfigurationName, new object[] { str3, wsmanSessionTM.ConnectionInfo.ComputerName });
                exception = new PSRemotingTransportException(PSRemotingErrorId.InvalidConfigurationName, RemotingErrorIdStrings.ConnectExCallBackError, new object[] { wsmanSessionTM.ConnectionInfo.ComputerName, str4 }) {
                    TransportMessage = ParseEscapeWSManErrorMessage(WSManNativeApi.WSManGetErrorMessage(wsmanAPIHandle, errorStruct.errorCode))
                };
            }
            else
            {
                string str5 = PSRemotingErrorInvariants.FormatResourceString(resourceString, resourceArgs);
                exception = new PSRemotingTransportException(PSRemotingErrorId.TroubleShootingHelpTopic, RemotingErrorIdStrings.TroubleShootingHelpTopic, new object[] { str5 }) {
                    TransportMessage = ParseEscapeWSManErrorMessage(WSManNativeApi.WSManGetErrorMessage(wsmanAPIHandle, errorStruct.errorCode))
                };
            }
            exception.ErrorCode = errorStruct.errorCode;
            return new TransportErrorOccuredEventArgs(exception, transportMethodReportingError);
        }

        internal static string GetFQEIDFromTransportError(int transportErrorCode, string defaultFQEID)
        {
            string str;
            if (_transportErrorCodeToFQEID.TryGetValue(transportErrorCode, out str))
            {
                return (str + "," + defaultFQEID);
            }
            if (transportErrorCode != 0)
            {
                return (transportErrorCode.ToString(NumberFormatInfo.InvariantInfo) + "," + defaultFQEID);
            }
            return defaultFQEID;
        }

        internal static string ParseEscapeWSManErrorMessage(string errorMessage)
        {
            if (!string.IsNullOrEmpty(errorMessage) && errorMessage.Contains("@{"))
            {
                return errorMessage.Replace("@{", "'@{").Replace("}", "}'");
            }
            return errorMessage;
        }

        internal enum tmStartModes
        {
            Connect = 3,
            Create = 2,
            None = 1
        }
    }
}

