using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static string EventLog_ASPNET_SessionTimeout
		{
			get
			{
				return Resources.ResourceManager.GetString("EventLog_ASPNET_SessionTimeout", Resources.resourceCulture);
			}
		}

		internal static string EventLog_BrowserInitiated
		{
			get
			{
				return Resources.ResourceManager.GetString("EventLog_BrowserInitiated", Resources.resourceCulture);
			}
		}

		internal static string EventLog_IdleSessionTimeout
		{
			get
			{
				return Resources.ResourceManager.GetString("EventLog_IdleSessionTimeout", Resources.resourceCulture);
			}
		}

		internal static string EventLog_Logout
		{
			get
			{
				return Resources.ResourceManager.GetString("EventLog_Logout", Resources.resourceCulture);
			}
		}

		internal static string EventLog_UnknownError
		{
			get
			{
				return Resources.ResourceManager.GetString("EventLog_UnknownError", Resources.resourceCulture);
			}
		}

		internal static string HttpsRequired
		{
			get
			{
				return Resources.ResourceManager.GetString("HttpsRequired", Resources.resourceCulture);
			}
		}

		internal static string InternalError_InvalidAuthenticationMechanism
		{
			get
			{
				return Resources.ResourceManager.GetString("InternalError_InvalidAuthenticationMechanism", Resources.resourceCulture);
			}
		}

		internal static string LogonButton_Text
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonButton_Text", Resources.resourceCulture);
			}
		}

		internal static string LogonError_AccessDenied
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_AccessDenied", Resources.resourceCulture);
			}
		}

		internal static string LogonError_ConnectionError
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_ConnectionError", Resources.resourceCulture);
			}
		}

		internal static string LogonError_ConnectionErrorExtended
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_ConnectionErrorExtended", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidAuthenticationTypeBasicOrDigest
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidAuthenticationTypeBasicOrDigest", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidAuthenticationTypeCredSPP
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidAuthenticationTypeCredSPP", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidCharacterFormat
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidCharacterFormat", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidComputer
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidComputer", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidComputerNameUriFormat
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidComputerNameUriFormat", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidConfigurationName
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidConfigurationName", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidCredentials
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidCredentials", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidEndpoint
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidEndpoint", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidPort
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidPort", Resources.resourceCulture);
			}
		}

		internal static string LogonError_InvalidUri
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_InvalidUri", Resources.resourceCulture);
			}
		}

		internal static string LogonError_LogMessage
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_LogMessage", Resources.resourceCulture);
			}
		}

		internal static string LogonError_NoLogonServers
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_NoLogonServers", Resources.resourceCulture);
			}
		}

		internal static string LogonError_RemotingNotEnabled
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_RemotingNotEnabled", Resources.resourceCulture);
			}
		}

		internal static string LogonError_ThreadAborted
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_ThreadAborted", Resources.resourceCulture);
			}
		}

		internal static string LogonError_UnknownError
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_UnknownError", Resources.resourceCulture);
			}
		}

		internal static string LogonError_UnknownErrorAdminMessage
		{
			get
			{
				return Resources.ResourceManager.GetString("LogonError_UnknownErrorAdminMessage", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("Microsoft.Management.PowerShellWebAccess.Primitives.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		internal static string UnknownIpAddress
		{
			get
			{
				return Resources.ResourceManager.GetString("UnknownIpAddress", Resources.resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}