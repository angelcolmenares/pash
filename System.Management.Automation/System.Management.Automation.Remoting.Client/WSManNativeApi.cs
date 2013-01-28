using System.IO;
using System.Management.Automation.Remoting.WSMan;
using System.Xml;

namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Management.Automation.Remoting;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    internal static class WSManNativeApi
    {
        internal const int ERROR_WSMAN_ACCESS_DENIED = 5;
        internal const int ERROR_WSMAN_AUTHENTICATION_FAILED = 0x51f;
        internal const int ERROR_WSMAN_BAD_METHOD = -2144108428;
       	internal const int ERROR_WSMAN_CANNOT_CONNECT_INVALID = -2144108080;
        internal const int ERROR_WSMAN_CANNOT_CONNECT_MISMATCH = -2144108090;
        internal const int ERROR_WSMAN_CANNOT_CONNECT_RUNASFAILED = -2144108065;
        internal const int ERROR_WSMAN_CANNOTUSE_IP = -2144108101;
        internal const int ERROR_WSMAN_CLIENT_KERBEROS_DISABLED = -2144108318;
        internal const int ERROR_WSMAN_COMPUTER_NOTFOUND = -2144108103;
        internal const int ERROR_WSMAN_CORRUPTED_CONFIG = -2144108539;
        internal const int ERROR_WSMAN_CREATEFAILED_INVALIDNAME = -2144108094;
        internal const int ERROR_WSMAN_EXPLICIT_CREDENTIALS_REQUIRED = -2144108315;
        internal const int ERROR_WSMAN_HTTP_SERVICE_ERROR = -2144108176;
        internal const int ERROR_WSMAN_HTTP_SERVICE_UNAVAILABLE = -2144108270;
        internal const int ERROR_WSMAN_IMPROPER_RESPONSE = 0x6ba;
        internal const int ERROR_WSMAN_INCORRECT_PROTOCOLVERSION = -2141974624;
        internal const int ERROR_WSMAN_INUSE_CANNOT_RECONNECT = -2144108083;
        internal const int ERROR_WSMAN_INVALID_AUTHENTICATION = -2144108274;
        internal const int ERROR_WSMAN_INVALID_RESOURCE_URI = -2144108485;
        internal const int ERROR_WSMAN_INVALID_RESOURCE_URI2 = -2144108542;
        internal const int ERROR_WSMAN_LOGON_FAILURE = 0x52e;
        internal const int ERROR_WSMAN_NETWORKPATH_NOTFOUND = 0x35;
        internal const int ERROR_WSMAN_OPERATION_ABORTED = 0x3e3;
        internal const int ERROR_WSMAN_OUTOF_MEMORY = 14;
        internal const int ERROR_WSMAN_REDIRECT_LOCATION_INVALID = -2144108105;
        internal const int ERROR_WSMAN_REDIRECT_REQUESTED = -2144108135;
        internal const int ERROR_WSMAN_REMOTECONNECTION_DISALLOWED = -2144108061;
        internal const int ERROR_WSMAN_REMOTESESSION_DISALLOWED = -2144108116;
        internal const int ERROR_WSMAN_SENDDATA_CANNOT_COMPLETE = -2144108250;
        internal const int ERROR_WSMAN_SENDDATA_CANNOT_CONNECT = -2144108526;
        internal const int ERROR_WSMAN_SERVER_NOTTRUSTED = -2144108316;
        internal const int ERROR_WSMAN_SHUTDOWN_INPROGRESS = 0x45b;
        internal const int ERROR_WSMAN_TARGET_UNKOWN = -2146893053;
        internal const int ERROR_WSMAN_TARGETSESSION_DOESNOTEXIST = -2144108453;
        internal const int ERROR_WSMAN_URI_LIMIT = -2144108499;
        internal const int ERROR_WSMAN_URL_NOTAVAILABLE = -2144108269;
        internal const int ERROR_WSMAN_WORKGROUP_NO_KERBEROS = -2144108276;
        internal const int INFINITE = int.MaxValue;
        internal const string PS_CONNECT_XML_TAG = "connectXml";
        internal const string PS_CONNECTRESPONSE_XML_TAG = "connectResponseXml";
        internal const string PS_CREATION_XML_TAG = "creationXml";
        internal const string PS_XML_NAMESPACE = "http://schemas.microsoft.com/powershell";
        internal const string ResourceURIPrefix = "http://schemas.microsoft.com/powershell/";
        internal const int WSMAN_DEFAULT_MAX_ENVELOPE_SIZE_KB_V2 = 150;
        internal const int WSMAN_DEFAULT_MAX_ENVELOPE_SIZE_KB_V3 = 500;
        internal const int WSMAN_FLAG_REQUESTED_API_VERSION_1_1 = 1;
        internal static readonly Version WSMAN_STACK_VERSION = new Version(3, 0);
        internal const string WSMAN_STREAM_ID_PROMPTRESPONSE = "pr";
        internal const string WSMAN_STREAM_ID_STDIN = "stdin";
        internal const string WSMAN_STREAM_ID_STDOUT = "stdout";
        internal const string WSManApiDll = "WsmSvc.dll";

		private static IWSManService _svc;

		private static void Trace (string message)
		{
			System.Diagnostics.Debug.WriteLine ("{1}", new object[] { "WSManNativeApi", message });
		}

		/*
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern void WSManCloseCommand(IntPtr cmdHandle, int flags, IntPtr asyncCallback);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern void WSManCloseOperation(IntPtr operationHandle, int flags);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern void WSManCloseSession(IntPtr wsManSessionHandle, int flags);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern void WSManCloseShell(IntPtr shellHandle, int flags, IntPtr asyncCallback);
        [DllImport("WsmSvc.dll", EntryPoint="WSManConnectShellCommand", CharSet=CharSet.Unicode)]
        internal static extern void WSManConnectShellCommandEx(IntPtr shellOperationHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string commandID, IntPtr optionSet, IntPtr connectXml, IntPtr asyncCallback, ref IntPtr commandOperationHandle);
        [DllImport("WsmSvc.dll", EntryPoint="WSManConnectShell", CharSet=CharSet.Unicode)]
        internal static extern void WSManConnectShellEx(IntPtr wsManSessionHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string resourceUri, [MarshalAs(UnmanagedType.LPWStr)] string shellId, IntPtr optionSet, IntPtr connectXml, IntPtr asyncCallback, [In, Out] ref IntPtr shellOperationHandle);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern int WSManCreateSession(IntPtr wsManAPIHandle, [MarshalAs(UnmanagedType.LPWStr)] string connection, int flags, IntPtr authenticationCredentials, IntPtr proxyInfo, [In, Out] ref IntPtr wsManSessionHandle);
        */

		internal static void WSManCloseCommand (IntPtr cmdHandle, int flags, IntPtr asyncCallback)
		{
			Trace ("WSManCloseCommand");
			WSManCommandHandle handle = MarshalledObject.FromPointer<WSManCommandHandle>(cmdHandle);

			cmdHandle = IntPtr.Zero;
			
			WSManShellAsync.WSManShellAsyncInternal callAsync = MarshalledObject.FromPointer<WSManShellAsync.WSManShellAsyncInternal>(asyncCallback);
			WSManShellCompletionFunction func = (WSManShellCompletionFunction)Marshal.GetDelegateForFunctionPointer (callAsync.asyncCallback, typeof(WSManShellCompletionFunction));
			WSManNativeApi.WSManCreateShellDataResult result = new WSManCreateShellDataResult();
			result.data = "success";
			var resultPtr = result.ToPtr();
			func(callAsync.operationContext, 0x20, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, resultPtr);
		}

		internal static void WSManCloseOperation (IntPtr operationHandle, int flags)
		{
			Trace("WSManCloseOperation");
			operationHandle = IntPtr.Zero;
		}

		internal static void WSManCloseSession(IntPtr wsManSessionHandle, int flags)
		{
			Trace("WSManCloseSession");
			_svc.CloseSession(GetSessionFromSessionHandle (wsManSessionHandle), null);
			wsManSessionHandle = IntPtr.Zero;
		}

		internal static void WSManCloseShell(IntPtr shellHandle, int flags, IntPtr asyncCallback)
		{
			Trace("WSManCloseShell");
			WSManUnixShellOperationHandle op = MarshalledObject.FromPointer<WSManUnixShellOperationHandle>(shellHandle);
			WSManShellAsync.WSManShellAsyncInternal callAsync = MarshalledObject.FromPointer<WSManShellAsync.WSManShellAsyncInternal>(asyncCallback);
			WSManShellCompletionFunction func = (WSManShellCompletionFunction)Marshal.GetDelegateForFunctionPointer (callAsync.asyncCallback, typeof(WSManShellCompletionFunction));

			_svc.CloseShell (op.SessionId, op.ShellId);
			
			func(callAsync.operationContext, 0x20, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
		}

		internal static void WSManConnectShellCommandEx (IntPtr shellOperationHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string commandID, IntPtr optionSet, IntPtr connectXml, IntPtr asyncCallback, ref IntPtr commandOperationHandle)
		{
			Trace("WSManConnectShellCommandEx");
			WSManUnixShellOperationHandle op = MarshalledObject.FromPointer<WSManUnixShellOperationHandle>(shellOperationHandle);
		}

		private static Guid GetSessionFromSessionHandle (IntPtr ptr)
		{
			WSManUnixSession session = MarshalledObject.FromPointer<WSManUnixSession>(ptr);
			return session.SessionId;
		}

		internal static void WSManConnectShellEx (IntPtr wsManSessionHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string resourceUri, [MarshalAs(UnmanagedType.LPWStr)] string shellId, IntPtr optionSet, IntPtr connectXml, IntPtr asyncCallback, [In, Out] ref IntPtr shellOperationHandle)
		{
			Guid sessionId = GetSessionFromSessionHandle(wsManSessionHandle);
			Trace("WSManConnectShellEx");
			byte[] content = null;
			if (connectXml != IntPtr.Zero) {
				var data = MarshalledObject.FromPointer<WSManData.WSManDataInternal> (connectXml);
				if (data.type == 1) {
					XmlDocument doc = new XmlDocument();
					doc.LoadXml (Marshal.PtrToStringUni (data.binaryOrTextData.data));
					content = Convert.FromBase64String (doc.DocumentElement.InnerText);
				} else if (data.type == 2) {
					content = new byte[data.binaryOrTextData.bufferLength];
					Marshal.Copy (data.binaryOrTextData.data, content, 0, content.Length);
				}
			}
			var response = _svc.ConnectShell (sessionId, new Guid(shellId), content);

			string responseText = Convert.ToBase64String (response);
			WSManConnectDataResult result = new WSManConnectDataResult();
			result.data = string.Format ("<{0}>{1}</{0}>", PS_CONNECTRESPONSE_XML_TAG, responseText);
			WSManShellAsync.WSManShellAsyncInternal callAsync = MarshalledObject.FromPointer<WSManShellAsync.WSManShellAsyncInternal>(asyncCallback);
			WSManShellCompletionFunction func = (WSManShellCompletionFunction)Marshal.GetDelegateForFunctionPointer (callAsync.asyncCallback, typeof(WSManShellCompletionFunction));
			var resultPtr = result.ToPtr();

			func(callAsync.operationContext, 0x20, IntPtr.Zero, shellOperationHandle, IntPtr.Zero, IntPtr.Zero, resultPtr);
		}

		internal static int WSManCreateSession (IntPtr wsManAPIHandle, [MarshalAs(UnmanagedType.LPWStr)] string connection, int flags, IntPtr authenticationCredentials, IntPtr proxyInfo, [In, Out] ref IntPtr wsManSessionHandle)
		{
			Trace("WSManCreateSession");
			WSManUnixSession session = new WSManUnixSession();
			session.ProtocolVersion = 3;
			session.Connection = connection;
			if (authenticationCredentials != IntPtr.Zero) {
				var credentials = MarshalledObject.FromPointer<WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct>(authenticationCredentials);
				session.Username =  credentials.userName;
				session.Password = Marshal.PtrToStringUni (credentials.password);
				session.AuthenticationMechanism = (int)credentials.authenticationMechanism;
			}

			session.SessionId = _svc.CreateSession(session.Connection, session.Username, session.Password, session.AuthenticationMechanism, session.ProtocolVersion);
			wsManSessionHandle = (IntPtr)MarshalledObject.Create<WSManUnixSession>(session);
			return 0;
		}


		internal static void WSManCreateShellEx(IntPtr wsManSessionHandle, int flags, string resourceUri, string shellId, WSManShellStartupInfo startupInfo, WSManOptionSet optionSet, WSManData openContent, IntPtr asyncCallback, ref IntPtr shellOperationHandle)
        {
            WSManCreateShellExInternal(wsManSessionHandle, flags, resourceUri, shellId, (IntPtr) startupInfo, (IntPtr) optionSet, (IntPtr) openContent, asyncCallback, ref shellOperationHandle);
        }
		/*
        [DllImport("WsmSvc.dll", EntryPoint="WSManCreateShellEx", CharSet=CharSet.Unicode)]
        private static extern void WSManCreateShellExInternal(IntPtr wsManSessionHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string resourceUri, [MarshalAs(UnmanagedType.LPWStr)] string shellId, IntPtr startupInfo, IntPtr optionSet, IntPtr openContent, IntPtr asyncCallback, [In, Out] ref IntPtr shellOperationHandle);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern int WSManDeinitialize(IntPtr wsManAPIHandle, int flags);
        [DllImport("WsmSvc.dll", EntryPoint="WSManDisconnectShell", CharSet=CharSet.Unicode)]
        internal static extern void WSManDisconnectShellEx(IntPtr wsManSessionHandle, int flags, IntPtr disconnectInfo, IntPtr asyncCallback);
        */

		
		private static void WSManCreateShellExInternal (IntPtr wsManSessionHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string resourceUri, [MarshalAs(UnmanagedType.LPWStr)] string shellId, IntPtr startupInfo, IntPtr optionSet, IntPtr openContent, IntPtr asyncCallback, [In, Out] ref IntPtr shellOperationHandle)
		{
			Trace("WSManCreateShellExInternal");
			Guid sessionId = GetSessionFromSessionHandle (wsManSessionHandle);
			var startup = MarshalledObject.FromPointer<WSManShellStartupInfo.WSManShellStartupInfoInternal>(startupInfo);
			var shellName = startup.name;
			var workingDir = startup.workingDirectory;
			string[] outputSet = WSManStreamIDSet.FromPointer (startup.outputStreamSet);
			string[] inputSet = WSManStreamIDSet.FromPointer (startup.inputStreamSet);
			int idleTimeout = startup.idleTimeoutMs;

			WSManUnixShellOperationHandle op = new WSManUnixShellOperationHandle ();
			op.ResourceUri = resourceUri;
			op.SessionId  = sessionId;
			op.ShellId = new Guid (shellId);
			op.ShellName = shellName;
			byte[] content = null;
			//var option = MarshalledObject.FromPointer<WSManOptionSet.WSManOptionSetInternal> (optionSet);
			if (openContent != IntPtr.Zero) {
				var data = MarshalledObject.FromPointer<WSManData.WSManDataInternal> (openContent);
				if (data.type == 1) {
					XmlDocument doc = new XmlDocument();
					doc.LoadXml (Marshal.PtrToStringUni (data.binaryOrTextData.data));
					content = Convert.FromBase64String (doc.DocumentElement.InnerText);
				} else if (data.type == 2) {
					content = new byte[data.binaryOrTextData.bufferLength];
					Marshal.Copy (data.binaryOrTextData.data, content, 0, content.Length);
				}
			}
			var handlePtr = (IntPtr)MarshalledObject.Create<WSManUnixShellOperationHandle>(op);
			shellOperationHandle = handlePtr;
			WSManShellAsync.WSManShellAsyncInternal callAsync = MarshalledObject.FromPointer<WSManShellAsync.WSManShellAsyncInternal>(asyncCallback);
			WSManShellCompletionFunction func = (WSManShellCompletionFunction)Marshal.GetDelegateForFunctionPointer (callAsync.asyncCallback, typeof(WSManShellCompletionFunction));

			WSManNativeApi.WSManCreateShellDataResult result = new WSManCreateShellDataResult();
			result.data = "<shellXml><IdleTimeOut>006.500</IdleTimeOut><MaxIdleTimeOut>006.500</MaxIdleTimeOut><BufferMode>Block</BufferMode></shellXml>";
			var resultPtr = result.ToPtr();
			_svc.CreateShell (op.SessionId, op.ShellId, content);
			func(callAsync.operationContext, 0x20, IntPtr.Zero, handlePtr, IntPtr.Zero, IntPtr.Zero, resultPtr);
		}

		internal static int WSManDeinitialize (IntPtr wsManAPIHandle, int flags)
		{
			Trace("WSManDeinitialize");
			wsManAPIHandle = IntPtr.Zero;
			return 0;
		}

		internal static void WSManDisconnectShellEx (IntPtr wsManSessionHandle, int flags, IntPtr disconnectInfo, IntPtr asyncCallback)
		{
			Trace ("WSManDisconnectShellEx");
		}


		internal static string WSManGetErrorMessage(IntPtr wsManAPIHandle, int errorCode)
        {
            string name = Thread.CurrentThread.CurrentUICulture.Name;
            string str2 = "";
            int messageLengthUsed = 0;
            if (0x7a == WSManGetErrorMessage(wsManAPIHandle, 0, name, errorCode, 0, null, out messageLengthUsed))
            {
                int num2;
                byte[] message = new byte[messageLengthUsed * 2];
                if (WSManGetErrorMessage(wsManAPIHandle, 0, name, errorCode, messageLengthUsed * 2, message, out num2) != 0)
                {
                    return str2;
                }
                try
                {
                    str2 = Encoding.Unicode.GetString(message);
                }
                catch (ArgumentNullException)
                {
                }
                catch (DecoderFallbackException)
                {
                }
            }
			Trace("WSManGetErrorMessage");
            return str2;
        }

		/*
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern int WSManGetErrorMessage(IntPtr wsManAPIHandle, int flags, string languageCode, int errorCode, int messageLength, byte[] message, out int messageLengthUsed);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern void WSManGetSessionOptionAsDword(IntPtr wsManSessionHandle, WSManSessionOption option, out int value);
		*/

		internal static int WSManGetErrorMessage (IntPtr wsManAPIHandle, int flags, string languageCode, int errorCode, int messageLength, byte[] message, out int messageLengthUsed)
		{
			Trace("WSManGetErrorMessage");
			messageLengthUsed = messageLength;
			return 0;
		}

		internal static void WSManGetSessionOptionAsDword (IntPtr wsManSessionHandle, WSManSessionOption option, out int value)
		{
			Trace("WSManGetSessionOptionAsDword");
			
			value = 0;
			switch (option) {
				case WSManSessionOption.WSMAN_OPTION_MAX_ENVELOPE_SIZE_KB:
					value = 1024 * 32;
					break;
				case WSManSessionOption.WSMAN_OPTION_ALLOW_NEGOTIATE_IMPLICIT_CREDENTIALS:
					value = 1;
					break;
				case WSManSessionOption.WSMAN_OPTION_DEFAULT_OPERATION_TIMEOUTMS:
					value = 1000 * 60;
					break;
				case WSManSessionOption.WSMAN_OPTION_ENABLE_SPN_SERVER_PORT:
					value = 0;
					break;
				case WSManSessionOption.WSMAN_OPTION_LOCALE: 
					value = 1033;
					break;
				case WSManSessionOption.WSMAN_OPTION_MACHINE_ID:
					value = 1;
					break;
				case WSManSessionOption.WSMAN_OPTION_MAX_RETRY_TIME:
					value = 3;
					break;
				case WSManSessionOption.WSMAN_OPTION_REDIRECT_LOCATION:
					value = 0;
					break;
				case WSManSessionOption.WSMAN_OPTION_SHELL_MAX_DATA_SIZE_PER_MESSAGE_KB:
					value = 1024 * 32;
					break;
				case WSManSessionOption.WSMAN_OPTION_SKIP_CA_CHECK:
					value = 1;
					break;
				case WSManSessionOption.WSMAN_OPTION_SKIP_CN_CHECK:
					value = 1;
					break;
				case WSManSessionOption.WSMAN_OPTION_SKIP_REVOCATION_CHECK:
					value = 1;
					break;
				case WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_CLOSE_SHELL_OPERATION:
					value = 1000 * 60;
					break;
				case WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_CREATE_SHELL:
					value = 1000 * 60;
					break;
				case WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_RECEIVE_SHELL_OUTPUT:
					value = 1000 * 60;
					break;
				case WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_SEND_SHELL_INPUT:
					value = 1000 * 60;
					break;
				case WSManSessionOption.WSMAN_OPTION_TIMEOUTMS_SIGNAL_SHELL:
					value = 1000 * 60;
					break;
				case WSManSessionOption.WSMAN_OPTION_UI_LANGUAGE:
					value = 1033;
					break;
				case WSManSessionOption.WSMAN_OPTION_UNENCRYPTED_MESSAGES:
					value = 1;
					break;
				case WSManSessionOption.WSMAN_OPTION_USE_INTERACTIVE_TOKEN:
					value = 0;
					break;
				case WSManSessionOption.WSMAN_OPTION_USE_SSL:
					value = 0; //TODO: CHANGE
					break;
				case WSManSessionOption.WSMAN_OPTION_UTF16:
					value = 0;
					break;
				default:
					value = 0;
					break;
			}
		}

		internal static string WSManGetSessionOptionAsString(IntPtr wsManAPIHandle, WSManSessionOption option)
        {
			Trace("WSManGetSessionOptionAsString");
            string str = "";
            int optionLengthUsed = 0;
            if (0x7a == WSManGetSessionOptionAsString(wsManAPIHandle, option, 0, null, out optionLengthUsed))
            {
                int num2;
                byte[] optionAsString = new byte[optionLengthUsed * 2];
                if (WSManGetSessionOptionAsString(wsManAPIHandle, option, optionLengthUsed * 2, optionAsString, out num2) != 0)
                {
                    return str;
                }
                try
                {
                    str = Encoding.Unicode.GetString(optionAsString);
                }
                catch (ArgumentNullException)
                {
                }
                catch (DecoderFallbackException)
                {
                }
            }
            return str;
        }

		/*
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        private static extern int WSManGetSessionOptionAsString(IntPtr wsManSessionHandle, WSManSessionOption option, int optionLength, byte[] optionAsString, out int optionLengthUsed);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern int WSManInitialize(int flags, [In, Out] ref IntPtr wsManAPIHandle);
        [DllImport("WsmSvc.dll", EntryPoint="WSManReceiveShellOutput", CharSet=CharSet.Unicode)]
        internal static extern void WSManReceiveShellOutputEx(IntPtr shellOperationHandle, IntPtr commandOperationHandle, int flags, IntPtr desiredStreamSet, IntPtr asyncCallback, [In, Out] ref IntPtr receiveOperationHandle);
        [DllImport("WsmSvc.dll", EntryPoint="WSManReconnectShellCommand", CharSet=CharSet.Unicode)]
        internal static extern void WSManReconnectShellCommandEx(IntPtr wsManCommandHandle, int flags, IntPtr asyncCallback);
        [DllImport("WsmSvc.dll", EntryPoint="WSManReconnectShell", CharSet=CharSet.Unicode)]
        internal static extern void WSManReconnectShellEx(IntPtr wsManSessionHandle, int flags, IntPtr asyncCallback);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern void WSManRunShellCommandEx(IntPtr shellOperationHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string commandId, [MarshalAs(UnmanagedType.LPWStr)] string commandLine, IntPtr commandArgSet, IntPtr optionSet, IntPtr asyncCallback, ref IntPtr commandOperationHandle);
		*/

		private static int WSManGetSessionOptionAsString (IntPtr wsManSessionHandle, WSManSessionOption option, int optionLength, byte[] optionAsString, out int optionLengthUsed)
		{
			Trace("WSManGetSessionOptionAsString");
			int value = 0;
			WSManGetSessionOptionAsDword (wsManSessionHandle, option, out value);
			optionAsString = Encoding.UTF8.GetBytes (value.ToString ());
			optionLengthUsed = optionAsString.Length;
			return 0;
		}

		private static bool _initialized = false;

		internal static int WSManInitialize (int flags, [In, Out] ref IntPtr wsManAPIHandle)
		{
			Trace ("WSManInitialize");
			wsManAPIHandle = IntPtr.Zero;
			if (_initialized) return 0;
			_initialized = true;
			bool isLocal = PowerShellConfiguration.GetPolicyValue ("WSManLocal", false);
			if (isLocal) 
			{
				_svc = new WSManLocalService();
			}
			else 
			{
				//var assembly = System.Reflection.Assembly.Load ("System.Management.Automation.Extensions, Version=3.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
				//_svc = (IWSManService)assembly.CreateInstance ("System.Management.Automation.Extensions.Remoting.WSMan.WSManClientHandler");
				var assembly = System.Reflection.Assembly.Load ("Microsoft.WSMan.PowerShell, Version=3.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
				var wsmanHandlerType = Type.GetType ("Microsoft.WSMan.PowerShell.WSManClientHandler, Microsoft.WSMan.PowerShell, Version=3.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
				_svc = (IWSManService)Activator.CreateInstance (wsmanHandlerType);
			}
			_svc.Initialize();
			return 0;
		}

		internal static void WSManReceiveShellOutputEx (IntPtr shellOperationHandle, IntPtr commandOperationHandle, int flags, IntPtr desiredStreamSet, IntPtr asyncCallback, [In, Out] ref IntPtr receiveOperationHandle)
		{
			Trace ("WSManReceiveShellOutputEx");
			string customStream = null;
			WSManUnixShellOperationHandle op = MarshalledObject.FromPointer<WSManUnixShellOperationHandle>(shellOperationHandle);
			byte[] results = null;
			Guid commandId = Guid.Empty;
			string commandLine = "";
			if (commandOperationHandle != IntPtr.Zero) {
				WSManCommandHandle commandHandle = MarshalledObject.FromPointer<WSManCommandHandle>(commandOperationHandle);
				commandId = new Guid(commandHandle.CommandId);
				commandLine = commandHandle.CommandLine;
			}
			string[] streamIDs = WSManStreamIDSet.FromPointer (desiredStreamSet);
			WSManShellAsync.WSManShellAsyncInternal callAsync = MarshalledObject.FromPointer<WSManShellAsync.WSManShellAsyncInternal> (asyncCallback);
			WSManShellCompletionFunction func = (WSManShellCompletionFunction)Marshal.GetDelegateForFunctionPointer (callAsync.asyncCallback, typeof(WSManShellCompletionFunction));
			
			results = _svc.ReceiveData (op.SessionId, commandId);
			if (commandId != Guid.Empty) {
				if (commandLine.Equals ("prompt", StringComparison.OrdinalIgnoreCase))
				{
					customStream = WSMAN_STREAM_ID_PROMPTRESPONSE;
				}
			}
			if (results.Length > 0) {
				WSManNativeApi.WSManReceiveDataResult result = new WSManNativeApi.WSManReceiveDataResult ();
				result.data = results;
				result.stream = string.IsNullOrEmpty (customStream) ? (streamIDs.Length == 0 ? "stdin" : streamIDs [0]) : customStream;
				IntPtr resultPtr = result.ToPtr ();
				func (callAsync.operationContext, 0x20, IntPtr.Zero, shellOperationHandle, commandOperationHandle, IntPtr.Zero, resultPtr);
			} else {
				Trace ("ERROR: WSManReceiveShellOutputEx returned 0 bytes...");
			}
		}

		internal static void WSManReconnectShellCommandEx(IntPtr wsManCommandHandle, int flags, IntPtr asyncCallback)
		{
			Trace("WSManReconnectShellCommandEx");
			if (wsManCommandHandle != IntPtr.Zero) {
				WSManCommandHandle commandHandle = MarshalledObject.FromPointer<WSManCommandHandle>(wsManCommandHandle);
			}
		}

		internal static void WSManReconnectShellEx(IntPtr wsManSessionHandle, int flags, IntPtr asyncCallback)
		{
			Trace("WSManReconnectShellEx");
			
		}


		internal static void WSManRunShellCommandEx (IntPtr shellOperationHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string commandId, [MarshalAs(UnmanagedType.LPWStr)] string commandLine, IntPtr commandArgSet, IntPtr optionSet, IntPtr asyncCallback, ref IntPtr commandOperationHandle)
		{
			Trace ("WSManRunShellCommandEx");
			WSManUnixShellOperationHandle op = MarshalledObject.FromPointer<WSManUnixShellOperationHandle> (shellOperationHandle);
			var options = WSManOptionSet.FromPointer (optionSet);
			if (options.Length > 0) {
				string val = options [0].value;
			}
			WSManCommandHandle commandHandle = new WSManCommandHandle { CommandId = commandId, CommandLine = commandLine };
			var handlePtr = (IntPtr)MarshalledObject.Create<WSManCommandHandle> (commandHandle);
			var argument = WSManCommandArgSet.FromPointer (commandArgSet);
			commandOperationHandle = handlePtr;
			WSManShellAsync.WSManShellAsyncInternal callAsync = MarshalledObject.FromPointer<WSManShellAsync.WSManShellAsyncInternal> (asyncCallback);
			WSManShellCompletionFunction func = (WSManShellCompletionFunction)Marshal.GetDelegateForFunctionPointer (callAsync.asyncCallback, typeof(WSManShellCompletionFunction));
			WSManNativeApi.WSManReceiveDataResult result = new WSManNativeApi.WSManReceiveDataResult ();
			result.data = new byte[0];
			result.stream = WSMAN_STREAM_ID_STDOUT;
			IntPtr resultPtr = IntPtr.Zero;
			_svc.RunCommand (op.SessionId, commandHandle.CommandLine, op.ShellId, new Guid (commandHandle.CommandId), argument);
			func (callAsync.operationContext, 0x20, IntPtr.Zero, shellOperationHandle, handlePtr, IntPtr.Zero, resultPtr); 
			//_svc.CompleteCommand (op.SessionId);
		}
	
		internal static void WSManSendShellInputEx(IntPtr shellOperationHandle, IntPtr commandOperationHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string streamId, WSManData streamData, IntPtr asyncCallback, ref IntPtr sendOperationHandle)
        {
            WSManSendShellInputExInternal(shellOperationHandle, commandOperationHandle, flags, streamId, (IntPtr) streamData, false, asyncCallback, ref sendOperationHandle);
        }

		/*
        [DllImport("WsmSvc.dll", EntryPoint="WSManSendShellInput", CharSet=CharSet.Unicode)]
        private static extern void WSManSendShellInputExInternal(IntPtr shellOperationHandle, IntPtr commandOperationHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string streamId, IntPtr streamData, bool endOfStream, IntPtr asyncCallback, [In, Out] ref IntPtr sendOperationHandle);
        [DllImport("WsmSvc.dll", CharSet=CharSet.Unicode)]
        internal static extern int WSManSetSessionOption(IntPtr wsManSessionHandle, WSManSessionOption option, IntPtr data);
		*/

		private static void WSManSendShellInputExInternal (IntPtr shellOperationHandle, IntPtr commandOperationHandle, int flags, [MarshalAs(UnmanagedType.LPWStr)] string streamId, IntPtr streamData, bool endOfStream, IntPtr asyncCallback, [In, Out] ref IntPtr sendOperationHandle)
		{
			Trace ("WSManSendShellInputExInternal");
			WSManUnixShellOperationHandle op = MarshalledObject.FromPointer<WSManUnixShellOperationHandle>(shellOperationHandle);
			byte[] content = null;
			WSManUnixSendOperationHandle sendHandle;
			sendHandle.StreamId = streamId;
			sendOperationHandle = (IntPtr)MarshalledObject.Create<WSManUnixSendOperationHandle>(sendHandle);
			Guid commandId = Guid.Empty;
			if (commandOperationHandle != IntPtr.Zero) {
				WSManCommandHandle commandHandle = MarshalledObject.FromPointer<WSManCommandHandle>(commandOperationHandle);
				commandId = new Guid(commandHandle.CommandId);
			}

			if (streamData != IntPtr.Zero) {
				var data = MarshalledObject.FromPointer<WSManData.WSManDataInternal> (streamData);
				if (data.type == 1) {
					var dataText = Marshal.PtrToStringUni (data.binaryOrTextData.data);
					content = Convert.FromBase64String (dataText);
				} else if (data.type == 2) {
					content = new byte[data.binaryOrTextData.bufferLength];
					Marshal.Copy (data.binaryOrTextData.data, content, 0, content.Length);
				}
			}
			_svc.SendInput(op.SessionId, op.ShellId, commandId, streamId, content);

			WSManShellAsync.WSManShellAsyncInternal callAsync = MarshalledObject.FromPointer<WSManShellAsync.WSManShellAsyncInternal>(asyncCallback);
			WSManShellCompletionFunction func = (WSManShellCompletionFunction)Marshal.GetDelegateForFunctionPointer (callAsync.asyncCallback, typeof(WSManShellCompletionFunction));
			WSManNativeApi.WSManReceiveDataResult result = new WSManNativeApi.WSManReceiveDataResult();
			result.data = new byte[0];
			result.stream = WSMAN_STREAM_ID_STDOUT;
			IntPtr resultPtr = IntPtr.Zero;
			func(callAsync.operationContext, 0x20, IntPtr.Zero, shellOperationHandle, commandOperationHandle, IntPtr.Zero, resultPtr);
		}

		internal static int WSManSetSessionOption (IntPtr wsManSessionHandle, WSManSessionOption option, IntPtr data)
		{
			Trace("WSManSetSessionOption");
			return 0;
		}

        internal static int WSManSetSessionOption(IntPtr wsManSessionHandle, WSManSessionOption option, WSManDataDWord data)
        {
			Trace("WSManSetSessionOption");
            MarshalledObject obj2 = data.Marshal();
            using (obj2)
            {
                return WSManSetSessionOption(wsManSessionHandle, option, obj2.DataPtr);
            }
        }

		/*
        [DllImport("WsmSvc.dll", EntryPoint="WSManSignalShell", CharSet=CharSet.Unicode)]
        internal static extern void WSManSignalShellEx(IntPtr shellOperationHandle, IntPtr cmdOperationHandle, int flags, string code, IntPtr asyncCallback, [In, Out] ref IntPtr signalOperationHandle);
		*/

		internal static void WSManSignalShellEx(IntPtr shellOperationHandle, IntPtr cmdOperationHandle, int flags, string code, IntPtr asyncCallback, [In, Out] ref IntPtr signalOperationHandle)
		{
			Trace("WSManSignalShellEx");
			WSManShellAsync.WSManShellAsyncInternal callAsync = MarshalledObject.FromPointer<WSManShellAsync.WSManShellAsyncInternal>(asyncCallback);
			WSManShellCompletionFunction func = (WSManShellCompletionFunction)Marshal.GetDelegateForFunctionPointer (callAsync.asyncCallback, typeof(WSManShellCompletionFunction));
			WSManNativeApi.WSManReceiveDataResult result = new WSManNativeApi.WSManReceiveDataResult();
			result.data = new byte[0];
			result.stream = WSMAN_STREAM_ID_STDOUT;
			IntPtr resultPtr = IntPtr.Zero;
			WSManUnixSignalOperationHandle signalOp;
			signalOp.Code = code;
			signalOperationHandle = (IntPtr)MarshalledObject.Create<WSManUnixSignalOperationHandle>(signalOp);
			func(callAsync.operationContext, 0x20, IntPtr.Zero, shellOperationHandle, cmdOperationHandle, IntPtr.Zero, resultPtr);
		}


        internal abstract class BaseWSManAuthenticationCredentials : IDisposable
        {
            protected BaseWSManAuthenticationCredentials()
            {
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool isDisposing)
            {
            }

			public abstract WSManNativeApi.MarshalledObject GetMarshalledObject();
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct MarshalledObject : IDisposable
        {
            private IntPtr dataPtr;
            internal MarshalledObject(IntPtr dataPtr)
            {
                this.dataPtr = dataPtr;
            }

            internal IntPtr DataPtr
            {
                get
                {
                    return this.dataPtr;
                }
            }
            internal static WSManNativeApi.MarshalledObject Create<T>(T obj)
            {
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
                Marshal.StructureToPtr(obj, ptr, false);
                return new WSManNativeApi.MarshalledObject { dataPtr = ptr };
            }

			internal static T FromPointer<T>(IntPtr ptr)
			{
				return (T)Marshal.PtrToStructure (ptr, typeof(T));
			}

            public void Dispose()
            {
                if (IntPtr.Zero != this.dataPtr)
                {
                    Marshal.FreeHGlobal(this.dataPtr);
                    this.dataPtr = IntPtr.Zero;
                }
            }

            public static implicit operator IntPtr(WSManNativeApi.MarshalledObject obj)
            {
                return obj.dataPtr;
            }
        }

        [Flags]
        internal enum WSManAuthenticationMechanism
        {
            WSMAN_FLAG_AUTH_BASIC = 8,
            WSMAN_FLAG_AUTH_CLIENT_CERTIFICATE = 0x20,
            WSMAN_FLAG_AUTH_CREDSSP = 0x80,
            WSMAN_FLAG_AUTH_DIGEST = 2,
            WSMAN_FLAG_AUTH_KERBEROS = 0x10,
            WSMAN_FLAG_AUTH_NEGOTIATE = 4,
            WSMAN_FLAG_DEFAULT_AUTHENTICATION = 0,
            WSMAN_FLAG_NO_AUTHENTICATION = 1
        }

        internal enum WSManCallbackFlags
        {
            WSMAN_FLAG_CALLBACK_END_OF_OPERATION = 1,
            WSMAN_FLAG_CALLBACK_END_OF_STREAM = 8,
            WSMAN_FLAG_CALLBACK_NETWORK_FAILURE_DETECTED = 0x100,
            WSMAN_FLAG_CALLBACK_RECONNECTED_AFTER_NETWORK_FAILURE = 0x400,
            WSMAN_FLAG_CALLBACK_RETRY_ABORTED_DUE_TO_INTERNAL_ERROR = 0x1000,
            WSMAN_FLAG_CALLBACK_RETRYING_AFTER_NETWORK_FAILURE = 0x200,
            WSMAN_FLAG_CALLBACK_SHELL_AUTODISCONNECTED = 0x40,
            WSMAN_FLAG_CALLBACK_SHELL_AUTODISCONNECTING = 0x800,
            WSMAN_FLAG_CALLBACK_SHELL_SUPPORTS_DISCONNECT = 0x20,
            WSMAN_FLAG_RECEIVE_DELAY_STREAM_REQUEST_PROCESSED = 0x2000
        }

        internal class WSManCertificateThumbprintCredentials : WSManNativeApi.BaseWSManAuthenticationCredentials
        {
            private WSManNativeApi.MarshalledObject data;

            internal WSManCertificateThumbprintCredentials(string thumbPrint)
            {
                WSManThumbprintStruct struct2 = new WSManThumbprintStruct {
                    authenticationMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CLIENT_CERTIFICATE,
                    certificateThumbprint = thumbPrint,
                    reserved = IntPtr.Zero
                };
                this.data = WSManNativeApi.MarshalledObject.Create<WSManThumbprintStruct>(struct2);
            }

            protected override void Dispose(bool isDisposing)
            {
                this.data.Dispose();
            }

            public override WSManNativeApi.MarshalledObject GetMarshalledObject()
            {
                return this.data;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManThumbprintStruct
            {
                internal WSManNativeApi.WSManAuthenticationMechanism authenticationMechanism;
                [MarshalAs(UnmanagedType.LPWStr)]
                internal string certificateThumbprint;
                internal IntPtr reserved;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSManCommandArgSet : IDisposable
        {
            private WSManCommandArgSetInternal internalData;
            private WSManNativeApi.MarshalledObject data;
            internal WSManCommandArgSet(byte[] firstArgument)
            {
                this.internalData = new WSManCommandArgSetInternal();
                this.internalData.argsCount = 1;
                this.internalData.args = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
                IntPtr val = Marshal.StringToHGlobalUni(Convert.ToBase64String(firstArgument, Base64FormattingOptions.None));
                Marshal.WriteIntPtr(this.internalData.args, val);
                this.data = WSManNativeApi.MarshalledObject.Create<WSManCommandArgSetInternal>(this.internalData);
            }

			internal static byte[] FromPointer (IntPtr ptr)
			{
				var argPtr = MarshalledObject.FromPointer<WSManCommandArgSetInternal>(ptr);
				int num = Marshal.SizeOf(typeof(IntPtr));
				var dataPtr = Marshal.ReadIntPtr (argPtr.args, 0 * num);
				string base64Value = Marshal.PtrToStringUni (dataPtr);
				return Convert.FromBase64String (base64Value);
			}

            public void Dispose()
            {
                IntPtr hglobal = Marshal.ReadIntPtr(this.internalData.args);
                if (IntPtr.Zero != hglobal)
                {
                    Marshal.FreeHGlobal(hglobal);
                }
                Marshal.FreeHGlobal(this.internalData.args);
                this.data.Dispose();
            }

            public static implicit operator IntPtr(WSManNativeApi.WSManCommandArgSet obj)
            {
                return obj.data.DataPtr;
            }
            [StructLayout(LayoutKind.Sequential)]
            internal struct WSManCommandArgSetInternal
            {
                internal int argsCount;
                internal IntPtr args;
            }
        }

        internal class WSManConnectDataResult
        {
            internal string data;

            internal static WSManNativeApi.WSManConnectDataResult UnMarshal(IntPtr unmanagedData)
            {
                WSManConnectDataResultInternal internal2 = (WSManConnectDataResultInternal) Marshal.PtrToStructure(unmanagedData, typeof(WSManConnectDataResultInternal));
                string str = null;
                if (internal2.data.textData.textLength > 0)
                {
                    str = Marshal.PtrToStringUni(internal2.data.textData.text, internal2.data.textData.textLength);
                }
                return new WSManNativeApi.WSManConnectDataResult { data = str };
            }

			
			internal IntPtr ToPtr()
			{
				var internalData = new WSManTextDataInternal { textLength = this.data.Length, text = Marshal.StringToHGlobalUni (this.data) };
				var internalResult = new WSManConnectDataResultInternal { data = new WSManDataInternal { type = 1, textData = internalData  } };
				return (IntPtr)MarshalledObject.Create<WSManConnectDataResultInternal>(internalResult);
			}

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManConnectDataResultInternal
            {
                internal WSManNativeApi.WSManConnectDataResult.WSManDataInternal data;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManDataInternal
            {
                internal int type;
                internal WSManNativeApi.WSManConnectDataResult.WSManTextDataInternal textData;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManTextDataInternal
            {
                internal int textLength;
                internal IntPtr text;
            }
        }

        internal class WSManCreateShellDataResult
        {
            internal string data;

            internal static WSManNativeApi.WSManCreateShellDataResult UnMarshal(IntPtr unmanagedData)
            {
                WSManCreateShellDataResultInternal internal2 = (WSManCreateShellDataResultInternal) Marshal.PtrToStructure(unmanagedData, typeof(WSManCreateShellDataResultInternal));
                string str = null;
                if (internal2.data.textData.textLength > 0)
                {
                    str = Marshal.PtrToStringUni(internal2.data.textData.text, internal2.data.textData.textLength);
                }
                return new WSManNativeApi.WSManCreateShellDataResult { data = str };
            }

			internal IntPtr ToPtr()
			{
				var internalData = new WSManTextDataInternal { textLength = this.data.Length, text = Marshal.StringToHGlobalUni (this.data) };
				var internalResult = new WSManCreateShellDataResultInternal { data = new WSManDataInternal { type = 1, textData = internalData  } };
				return (IntPtr)MarshalledObject.Create<WSManCreateShellDataResultInternal>(internalResult);
			}

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManCreateShellDataResultInternal
            {
                internal WSManNativeApi.WSManCreateShellDataResult.WSManDataInternal data;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManDataInternal
            {
                internal int type;
                internal WSManNativeApi.WSManCreateShellDataResult.WSManTextDataInternal textData;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManTextDataInternal
            {
                internal int textLength;
                internal IntPtr text;
            }
        }

        internal class WSManData : IDisposable
        {
            private WSManDataInternal internalData;
            private IntPtr marshalledObject;

            internal WSManData()
            {
                this.marshalledObject = IntPtr.Zero;
            }

            internal WSManData(string data)
            {
                this.marshalledObject = IntPtr.Zero;
                this.internalData = new WSManDataInternal();
                this.internalData.binaryOrTextData = new WSManBinaryOrTextDataInternal();
                this.internalData.binaryOrTextData.bufferLength = data.Length;
                this.internalData.type = 1;
                this.internalData.binaryOrTextData.data = Marshal.StringToHGlobalUni(data);
                this.marshalledObject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WSManDataInternal)));
                Marshal.StructureToPtr(this.internalData, this.marshalledObject, false);
            }

            internal WSManData(byte[] data)
            {
                this.marshalledObject = IntPtr.Zero;
                this.internalData = new WSManDataInternal();
                this.internalData.binaryOrTextData = new WSManBinaryOrTextDataInternal();
                this.internalData.binaryOrTextData.bufferLength = data.Length;
                this.internalData.type = 2;
                IntPtr ptr = Marshal.AllocHGlobal(this.internalData.binaryOrTextData.bufferLength);
                this.internalData.binaryOrTextData.data = ptr;
                Marshal.Copy(data, 0, this.internalData.binaryOrTextData.data, this.internalData.binaryOrTextData.bufferLength);
                this.marshalledObject = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WSManDataInternal)));
                Marshal.StructureToPtr(this.internalData, this.marshalledObject, false);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool isDisposing)
            {
                if (this.internalData.binaryOrTextData.data != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.internalData.binaryOrTextData.data);
                    this.internalData.binaryOrTextData.data = IntPtr.Zero;
                }
                if (this.marshalledObject != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(this.marshalledObject);
                    this.marshalledObject = IntPtr.Zero;
                }
            }

            ~WSManData()
            {
                this.Dispose(false);
            }

            public static implicit operator IntPtr(WSManNativeApi.WSManData data)
            {
                if (data != null)
                {
                    return data.marshalledObject;
                }
                return IntPtr.Zero;
            }

            internal int BufferLength
            {
                get
                {
                    return this.internalData.binaryOrTextData.bufferLength;
                }
                set
                {
                    this.internalData.binaryOrTextData.bufferLength = value;
                }
            }

            internal int Type
            {
                get
                {
                    return this.internalData.type;
                }
                set
                {
                    this.internalData.type = value;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WSManBinaryOrTextDataInternal
            {
                internal int bufferLength;
                internal IntPtr data;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WSManDataInternal
            {
                internal int type;
                internal WSManNativeApi.WSManData.WSManBinaryOrTextDataInternal binaryOrTextData;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSManDataDWord
        {
            private WSManNativeApi.WSManDataType type;
            private WSManDWordDataInternal dwordData;
            internal WSManDataDWord(int data)
            {
                this.dwordData = new WSManDWordDataInternal();
                this.dwordData.number = data;
                this.type = WSManNativeApi.WSManDataType.WSMAN_DATA_TYPE_DWORD;
            }

            internal WSManNativeApi.MarshalledObject Marshal()
            {
                return WSManNativeApi.MarshalledObject.Create<WSManNativeApi.WSManDataDWord>(this);
            }
            [StructLayout(LayoutKind.Sequential)]
            private struct WSManDWordDataInternal
            {
                internal int number;
                internal IntPtr reserved;
            }
        }

        internal enum WSManDataType : int
        {
            WSMAN_DATA_NONE = 0,
            WSMAN_DATA_TYPE_BINARY = 2,
            WSMAN_DATA_TYPE_DWORD = 4,
            WSMAN_DATA_TYPE_TEXT = 1,
            WSMAN_DATA_TYPE_WS_XML_READER = 3
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct WSManError
        {
            internal int errorCode;
            internal string errorDetail;
            internal string language;
            internal string machineName;
            internal static WSManNativeApi.WSManError UnMarshal(IntPtr unmanagedData)
            {
                return (WSManNativeApi.WSManError) Marshal.PtrToStructure(unmanagedData, typeof(WSManNativeApi.WSManError));
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct WSManOption
        {
            internal const string NoProfile = "WINRS_NOPROFILE";
            internal const string CodePage = "WINRS_CODEPAGE";
            internal string name;
            internal string value;
            internal bool mustComply;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSManOptionSet : IDisposable
        {
            private WSManOptionSetInternal optionSet;
            private WSManNativeApi.MarshalledObject data;
            internal WSManOptionSet(WSManNativeApi.WSManOption[] options)
            {
                int num = Marshal.SizeOf(typeof(WSManNativeApi.WSManOption));
                this.optionSet = new WSManOptionSetInternal();
                this.optionSet.optionsCount = options.Length;
                this.optionSet.optionsMustUnderstand = true;
                this.optionSet.options = Marshal.AllocHGlobal((int) (num * options.Length));
                for (int i = 0; i < options.Length; i++)
                {
                    Marshal.StructureToPtr(options[i], (IntPtr) (this.optionSet.options.ToInt64() + (num * i)), false);
                }
                this.data = WSManNativeApi.MarshalledObject.Create<WSManOptionSetInternal>(this.optionSet);
            }

			internal static WSManOption[] FromPointer (IntPtr ptr)
			{
				if (ptr == IntPtr.Zero) return new WSManOption[0];
				int num = Marshal.SizeOf (typeof(WSManNativeApi.WSManOption));
				WSManOptionSetInternal optionSet = MarshalledObject.FromPointer<WSManOptionSetInternal> (ptr);
				WSManOption[] results = new WSManOption[optionSet.optionsCount];
				for (var i = 0; i < optionSet.optionsCount; i++) {
					IntPtr optionPtr = Marshal.ReadIntPtr (optionSet.options, optionSet.options.ToInt32() + (num * i));
					results[i] = (WSManOption)Marshal.PtrToStructure (optionPtr, typeof(WSManOption));
				}
				return results;
			}

            public void Dispose()
            {
                if (IntPtr.Zero != this.optionSet.options)
                {
                    Marshal.FreeHGlobal(this.optionSet.options);
                    this.optionSet.options = IntPtr.Zero;
                }
                this.data.Dispose();
            }

            public static implicit operator IntPtr(WSManNativeApi.WSManOptionSet optionSet)
            {
                return optionSet.data.DataPtr;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct WSManOptionSetInternal
            {
                internal int optionsCount;
                internal IntPtr options;
                internal bool optionsMustUnderstand;
            }
        }

        internal class WSManProxyInfo : IDisposable
        {
            private WSManNativeApi.MarshalledObject data;

            internal WSManProxyInfo(ProxyAccessType proxyAccessType, WSManNativeApi.WSManUserNameAuthenticationCredentials authCredentials)
            {
                WSManProxyInfoInternal internal2 = new WSManProxyInfoInternal {
                    proxyAccessType = (int) proxyAccessType,
                    proxyAuthCredentialsStruct = new WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct()
                };
                internal2.proxyAuthCredentialsStruct.authenticationMechanism = WSManNativeApi.WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION;
                if (authCredentials != null)
                {
                    internal2.proxyAuthCredentialsStruct = authCredentials.CredentialStruct;
                }
                this.data = WSManNativeApi.MarshalledObject.Create<WSManProxyInfoInternal>(internal2);
            }

            public void Dispose()
            {
                this.data.Dispose();
                GC.SuppressFinalize(this);
            }

            public static implicit operator IntPtr(WSManNativeApi.WSManProxyInfo proxyInfo)
            {
                return proxyInfo.data.DataPtr;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            private struct WSManProxyInfoInternal
            {
                public int proxyAccessType;
                public WSManNativeApi.WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct proxyAuthCredentialsStruct;
            }
        }

        internal class WSManReceiveDataResult
        {
            internal byte[] data;
            internal string stream;

            internal static WSManNativeApi.WSManReceiveDataResult UnMarshal(IntPtr unmanagedData)
            {
                WSManReceiveDataResultInternal internal2 = (WSManReceiveDataResultInternal) Marshal.PtrToStructure(unmanagedData, typeof(WSManReceiveDataResultInternal));
                byte[] destination = null;
                if (internal2.data.binaryData.bufferLength > 0)
                {
                    destination = new byte[internal2.data.binaryData.bufferLength];
                    Marshal.Copy(internal2.data.binaryData.buffer, destination, 0, internal2.data.binaryData.bufferLength);
                }
                return new WSManNativeApi.WSManReceiveDataResult { data = destination, stream = internal2.streamId };
            }

			
			internal IntPtr ToPtr()
			{
				IntPtr bufferPtr = Marshal.AllocHGlobal(this.data.Length);
				Marshal.Copy (this.data, 0, bufferPtr, this.data.Length);
				var byteData = new WSManBinaryDataInternal { bufferLength = this.data.Length, buffer = bufferPtr };
				var internalData = new WSManDataInternal { type = 1, binaryData = byteData };
				var internalResult = new WSManReceiveDataResultInternal { streamId = this.stream, data = internalData };
				return (IntPtr)MarshalledObject.Create<WSManReceiveDataResultInternal>(internalResult);
			}

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManBinaryDataInternal
            {
                internal int bufferLength;
                internal IntPtr buffer;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManDataInternal
            {
                internal int type;
                internal WSManNativeApi.WSManReceiveDataResult.WSManBinaryDataInternal binaryData;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WSManReceiveDataResultInternal
            {
                [MarshalAs(UnmanagedType.LPWStr)]
                internal string streamId;
                internal WSManNativeApi.WSManReceiveDataResult.WSManDataInternal data;
                [MarshalAs(UnmanagedType.LPWStr)]
                internal string commandState;
                internal int exitCode;
            }
        }

        internal enum WSManSessionOption
        {
            WSMAN_OPTION_ALLOW_NEGOTIATE_IMPLICIT_CREDENTIALS = 0x20,
            WSMAN_OPTION_DEFAULT_OPERATION_TIMEOUTMS = 1,
            WSMAN_OPTION_ENABLE_SPN_SERVER_PORT = 0x16,
            WSMAN_OPTION_LOCALE = 0x19,
            WSMAN_OPTION_MACHINE_ID = 0x17,
            WSMAN_OPTION_MAX_ENVELOPE_SIZE_KB = 0x1c,
            WSMAN_OPTION_MAX_RETRY_TIME = 11,
            WSMAN_OPTION_REDIRECT_LOCATION = 30,
            WSMAN_OPTION_SHELL_MAX_DATA_SIZE_PER_MESSAGE_KB = 0x1d,
            WSMAN_OPTION_SKIP_CA_CHECK = 0x12,
            WSMAN_OPTION_SKIP_CN_CHECK = 0x13,
            WSMAN_OPTION_SKIP_REVOCATION_CHECK = 0x1f,
            WSMAN_OPTION_TIMEOUTMS_CLOSE_SHELL_OPERATION = 0x11,
            WSMAN_OPTION_TIMEOUTMS_CREATE_SHELL = 12,
            WSMAN_OPTION_TIMEOUTMS_RECEIVE_SHELL_OUTPUT = 14,
            WSMAN_OPTION_TIMEOUTMS_SEND_SHELL_INPUT = 15,
            WSMAN_OPTION_TIMEOUTMS_SIGNAL_SHELL = 0x10,
            WSMAN_OPTION_UI_LANGUAGE = 0x1a,
            WSMAN_OPTION_UNENCRYPTED_MESSAGES = 20,
            WSMAN_OPTION_USE_INTERACTIVE_TOKEN = 0x22,
            WSMAN_OPTION_USE_SSL = 0x21,
            WSMAN_OPTION_UTF16 = 0x15
        }

        internal class WSManShellAsync
        {
            private WSManNativeApi.MarshalledObject data;
            private WSManShellAsyncInternal internalData = new WSManShellAsyncInternal();

            internal WSManShellAsync(IntPtr context, WSManNativeApi.WSManShellAsyncCallback callback)
            {
                this.internalData.operationContext = context;
                this.internalData.asyncCallback = (IntPtr) callback;
                this.data = WSManNativeApi.MarshalledObject.Create<WSManShellAsyncInternal>(this.internalData);
            }

            public void Dispose()
            {
                this.data.Dispose();
            }

            public static implicit operator IntPtr(WSManNativeApi.WSManShellAsync async)
            {
                return (IntPtr) async.data;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WSManShellAsyncInternal
            {
                internal IntPtr operationContext;
                internal IntPtr asyncCallback;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSManShellAsyncCallback
        {
            private GCHandle gcHandle;
            private IntPtr asyncCallback;
            internal WSManShellAsyncCallback(WSManNativeApi.WSManShellCompletionFunction callback)
            {
                this.gcHandle = GCHandle.Alloc(callback);
                this.asyncCallback = Marshal.GetFunctionPointerForDelegate(callback);
            }

            public static implicit operator IntPtr(WSManNativeApi.WSManShellAsyncCallback callback)
            {
                return callback.asyncCallback;
            }
        }

        internal delegate void WSManShellCompletionFunction(IntPtr operationContext, int flags, IntPtr error, IntPtr shellOperationHandle, IntPtr commandOperationHandle, IntPtr operationHandle, IntPtr data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSManShellDisconnectInfo : IDisposable
        {
            private WSManShellDisconnectInfoInternal internalInfo;
            internal WSManNativeApi.MarshalledObject data;
            internal WSManShellDisconnectInfo(int serverIdleTimeOut)
            {
                this.internalInfo = new WSManShellDisconnectInfoInternal();
                this.internalInfo.idleTimeoutMs = serverIdleTimeOut;
                this.data = WSManNativeApi.MarshalledObject.Create<WSManShellDisconnectInfoInternal>(this.internalInfo);
            }

            public void Dispose()
            {
                this.data.Dispose();
            }

            public static implicit operator IntPtr(WSManNativeApi.WSManShellDisconnectInfo disconnectInfo)
            {
                return disconnectInfo.data.DataPtr;
            }
            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            private struct WSManShellDisconnectInfoInternal
            {
                internal int idleTimeoutMs;
            }
        }

        internal enum WSManShellFlag
        {
            WSMAN_FLAG_NO_COMPRESSION = 1,
            WSMAN_FLAG_RECEIVE_DELAY_OUTPUT_STREAM = 0x10,
            WSMAN_FLAG_SERVER_BUFFERING_MODE_BLOCK = 8,
            WSMAN_FLAG_SERVER_BUFFERING_MODE_DROP = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSManShellStartupInfo : IDisposable
        {
            private WSManShellStartupInfoInternal internalInfo;
            internal WSManNativeApi.MarshalledObject data;
            internal WSManShellStartupInfo(WSManNativeApi.WSManStreamIDSet inputStreamSet, WSManNativeApi.WSManStreamIDSet outputStreamSet, int serverIdleTimeOut, string name)
            {
                this.internalInfo = new WSManShellStartupInfoInternal();
                this.internalInfo.inputStreamSet = (IntPtr) inputStreamSet;
                this.internalInfo.outputStreamSet = (IntPtr) outputStreamSet;
                this.internalInfo.idleTimeoutMs = serverIdleTimeOut;
                this.internalInfo.workingDirectory = null;
                this.internalInfo.environmentVariableSet = IntPtr.Zero;
                this.internalInfo.name = name;
                this.data = WSManNativeApi.MarshalledObject.Create<WSManShellStartupInfoInternal>(this.internalInfo);
            }

            public void Dispose()
            {
                this.data.Dispose();
            }

            public static implicit operator IntPtr(WSManNativeApi.WSManShellStartupInfo startupInfo)
            {
                return startupInfo.data.DataPtr;
            }

            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            public struct WSManShellStartupInfoInternal
            {
                internal IntPtr inputStreamSet;
                internal IntPtr outputStreamSet;
                internal int idleTimeoutMs;
                [MarshalAs(UnmanagedType.LPWStr)]
                internal string workingDirectory;
                internal IntPtr environmentVariableSet;
                [MarshalAs(UnmanagedType.LPWStr)]
                internal string name;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WSManStreamIDSet
        {
            private WSManStreamIDSetInternal streamSetInfo;
            private WSManNativeApi.MarshalledObject data;
            internal WSManStreamIDSet(string[] streamIds)
            {
                int num = Marshal.SizeOf(typeof(IntPtr));
                this.streamSetInfo = new WSManStreamIDSetInternal();
                this.streamSetInfo.streamIDsCount = streamIds.Length;
                this.streamSetInfo.streamIDs = Marshal.AllocHGlobal((int) (num * streamIds.Length));
                for (int i = 0; i < streamIds.Length; i++)
                {
                    IntPtr val = Marshal.StringToHGlobalUni(streamIds[i]);
                    Marshal.WriteIntPtr(this.streamSetInfo.streamIDs, i * num, val);
                }
                this.data = WSManNativeApi.MarshalledObject.Create<WSManStreamIDSetInternal>(this.streamSetInfo);
            }

            internal void Dispose()
            {
                if (IntPtr.Zero != this.streamSetInfo.streamIDs)
                {
                    int num = Marshal.SizeOf(typeof(IntPtr));
                    for (int i = 0; i < this.streamSetInfo.streamIDsCount; i++)
                    {
                        IntPtr zero = IntPtr.Zero;
                        zero = Marshal.ReadIntPtr(this.streamSetInfo.streamIDs, i * num);
                        if (IntPtr.Zero != zero)
                        {
                            Marshal.FreeHGlobal(zero);
                            zero = IntPtr.Zero;
                        }
                    }
                    Marshal.FreeHGlobal(this.streamSetInfo.streamIDs);
                    this.streamSetInfo.streamIDs = IntPtr.Zero;
                }
                this.data.Dispose();
            }

			internal static string[] FromPointer (IntPtr ptr)
			{
				WSManStreamIDSet.WSManStreamIDSetInternal streamSet = MarshalledObject.FromPointer<WSManStreamIDSet.WSManStreamIDSetInternal> (ptr);
				int num = Marshal.SizeOf(typeof(IntPtr));
				string[] streamIDs = new string[streamSet.streamIDsCount];
				for (var i = 0; i < streamSet.streamIDsCount; i++) {
					IntPtr streamIdPtr = Marshal.ReadIntPtr (streamSet.streamIDs, i * num);
					streamIDs[i] = Marshal.PtrToStringUni (streamIdPtr);
				}
				return streamIDs;
			}

            public static implicit operator IntPtr(WSManNativeApi.WSManStreamIDSet obj)
            {
                return obj.data.DataPtr;
            }
            [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
            internal struct WSManStreamIDSetInternal
            {
                internal int streamIDsCount;
                internal IntPtr streamIDs;
            }
        }

        internal class WSManUserNameAuthenticationCredentials : WSManNativeApi.BaseWSManAuthenticationCredentials
        {
            private WSManUserNameCredentialStruct cred;
            private WSManNativeApi.MarshalledObject data;

            internal WSManUserNameAuthenticationCredentials()
            {
                this.cred = new WSManUserNameCredentialStruct();
                this.data = WSManNativeApi.MarshalledObject.Create<WSManUserNameCredentialStruct>(this.cred);
            }

            internal WSManUserNameAuthenticationCredentials(string name, SecureString pwd, WSManNativeApi.WSManAuthenticationMechanism authMechanism)
            {
                this.cred = new WSManUserNameCredentialStruct();
                this.cred.authenticationMechanism = authMechanism;
                this.cred.userName = name;
                if (pwd != null)
                {
                    this.cred.password = Marshal.SecureStringToGlobalAllocUnicode(pwd);
                }
                this.data = WSManNativeApi.MarshalledObject.Create<WSManUserNameCredentialStruct>(this.cred);
            }

            protected override void Dispose(bool isDisposing)
            {
                if (this.cred.password != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(this.cred.password);
                    this.cred.password = IntPtr.Zero;
                }
                this.data.Dispose();
            }

            public override WSManNativeApi.MarshalledObject GetMarshalledObject()
            {
                return this.data;
            }

            internal WSManUserNameCredentialStruct CredentialStruct
            {
                get
                {
                    return this.cred;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WSManUserNameCredentialStruct
            {
                internal WSManNativeApi.WSManAuthenticationMechanism authenticationMechanism;
                [MarshalAs(UnmanagedType.LPWStr)]
                internal string userName;
                internal IntPtr password;
            }
        }

		internal class AsyncExecution
		{
			internal Action RunAction
			{
				get;set;
			}

			internal Action Callback
			{
				get;set;
			}

			public void Complete (IAsyncResult result)
			{
				Callback.Invoke ();
			}

			public void Start ()
			{
				RunAction.BeginInvoke (Complete, null);
			}

		}
    }
}

