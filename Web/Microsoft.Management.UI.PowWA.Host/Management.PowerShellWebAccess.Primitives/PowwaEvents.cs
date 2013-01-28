using System;
using System.Diagnostics.Eventing;
using System.Threading;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public static class PowwaEvents
	{
		internal static EventProviderVersionTwo m_provider;

		private static EventDescriptor EVENT_AUTHENTICATION_FAILURE;

		private static EventDescriptor EVENT_AUTHORIZATION_FAILURE;

		private static EventDescriptor EVENT_GATEWAY_AUTHORIZATION_FAILURE;

		private static EventDescriptor EVENT_SESSION_LIMIT_REACHED;

		private static EventDescriptor EVENT_LOGON_FAILURE;

		private static EventDescriptor EVENT_AUTHORIZATION_FAILURE_INVALID_RULES;

		private static EventDescriptor EVENT_PSRCONNECTION_FAILURE;

		private static EventDescriptor EVENT_PSREXECUTION_FAILURE;

		private static EventDescriptor EVENT_SESSION_START;

		private static EventDescriptor EVENT_SESSION_END;

		private static EventDescriptor EVENT_TERMINATE_SESSION_ERROR;

		private static EventDescriptor EVENT_GENERIC_FAILURE;

		private static EventDescriptor EVENT_ACTIVITYID_TRANSFER;

		private static EventDescriptor EVENT_INVALID_APPLICATION_SETTING;

		private static EventDescriptor EVENT_INVALID_SESSION_KEY;

		private static EventDescriptor EVENT_INVALID_SESSION_USER;

		private static EventDescriptor EVENT_MALICIOUS_DATA;

		private static EventDescriptor EVENT_AUTHENTICATION_START;

		private static EventDescriptor EVENT_AUTHENTICATION_STOP;

		private static EventDescriptor EVENT_GATEWAY_AUTHORIZATION_START;

		private static EventDescriptor EVENT_GATEWAY_AUTHORIZATION_STOP;

		private static EventDescriptor EVENT_SESSION_LIMIT_CHECK;

		private static EventDescriptor EVENT_DEBUG_CONNECT_USING_COMPUTERNAME;

		private static EventDescriptor EVENT_DEBUG_CONNECT_USING_URI;

		private static EventDescriptor EVENT_DEBUG_LOG0;

		private static EventDescriptor EVENT_DEBUG_LOG1;

		private static EventDescriptor EVENT_DEBUG_LOG2;

		private static IEtwEventCorrelator _etwEventCorrelator;

		public static IEtwEventCorrelator EventCorrelator
		{
			get
			{
				if (PowwaEvents._etwEventCorrelator == null)
				{
					EtwEventCorrelator etwEventCorrelator = new EtwEventCorrelator(PowwaEvents.m_provider, PowwaEvents.EVENT_ACTIVITYID_TRANSFER);
					Interlocked.CompareExchange<IEtwEventCorrelator>(ref PowwaEvents._etwEventCorrelator, etwEventCorrelator, null);
				}
				return PowwaEvents._etwEventCorrelator;
			}
		}

		static PowwaEvents()
		{
			PowwaEvents.m_provider = new EventProviderVersionTwo(new Guid("ead6595b-5613-414c-a5ee-069bb1eca485"));
			PowwaEvents.EVENT_AUTHENTICATION_FAILURE = new EventDescriptor(0x101, 0, 16, 4, 0, 1, -9223372036854775800L);
			PowwaEvents.EVENT_AUTHORIZATION_FAILURE = new EventDescriptor(0x102, 0, 16, 4, 0, 2, -9223372036854775792L);
			PowwaEvents.EVENT_GATEWAY_AUTHORIZATION_FAILURE = new EventDescriptor(0x103, 0, 16, 4, 0, 2, -9223372036854775792L);
			PowwaEvents.EVENT_SESSION_LIMIT_REACHED = new EventDescriptor(0x104, 0, 16, 4, 0, 12, -9223372036854775792L);
			PowwaEvents.EVENT_LOGON_FAILURE = new EventDescriptor(0x105, 0, 16, 4, 0, 13, -9223372036854775800L);
			PowwaEvents.EVENT_AUTHORIZATION_FAILURE_INVALID_RULES = new EventDescriptor(0x10b, 0, 16, 3, 0, 2, -9223372036854775792L);
			PowwaEvents.EVENT_PSRCONNECTION_FAILURE = new EventDescriptor(0x201, 0, 16, 4, 0, 13, -9223372036854775806L);
			PowwaEvents.EVENT_PSREXECUTION_FAILURE = new EventDescriptor(0x202, 0, 16, 4, 0, 5, -9223372036854775806L);
			PowwaEvents.EVENT_SESSION_START = new EventDescriptor(0x301, 0, 16, 4, 1, 3, -9223372036854775806L);
			PowwaEvents.EVENT_SESSION_END = new EventDescriptor(0x302, 0, 16, 4, 2, 3, -9223372036854775806L);
			PowwaEvents.EVENT_TERMINATE_SESSION_ERROR = new EventDescriptor(0x307, 0, 16, 2, 0, 3, -9223372036854775806L);
			PowwaEvents.EVENT_GENERIC_FAILURE = new EventDescriptor(0x401, 0, 16, 4, 0, 6, -9223372036854775808L);
			PowwaEvents.EVENT_ACTIVITYID_TRANSFER = new EventDescriptor(0x402, 0, 16, 4, 0, 7, -9223372036854775808L);
			PowwaEvents.EVENT_INVALID_APPLICATION_SETTING = new EventDescriptor(0x501, 0, 16, 2, 0, 14, -9223372036854775808L);
			PowwaEvents.EVENT_INVALID_SESSION_KEY = new EventDescriptor(0x601, 0, 16, 2, 0, 3, -9223372036854775807L);
			PowwaEvents.EVENT_INVALID_SESSION_USER = new EventDescriptor(0x602, 0, 16, 2, 0, 3, -9223372036854775807L);
			PowwaEvents.EVENT_MALICIOUS_DATA = new EventDescriptor(0x603, 0, 16, 2, 0, 3, -9223372036854775807L);
			PowwaEvents.EVENT_AUTHENTICATION_START = new EventDescriptor(0x106, 0, 18, 4, 1, 1, 0x2000000000000008L);
			PowwaEvents.EVENT_AUTHENTICATION_STOP = new EventDescriptor(0x107, 0, 18, 4, 2, 1, 0x2000000000000008L);
			PowwaEvents.EVENT_GATEWAY_AUTHORIZATION_START = new EventDescriptor(0x108, 0, 18, 4, 1, 2, 0x2000000000000010L);
			PowwaEvents.EVENT_GATEWAY_AUTHORIZATION_STOP = new EventDescriptor(0x109, 0, 18, 4, 2, 2, 0x2000000000000010L);
			PowwaEvents.EVENT_SESSION_LIMIT_CHECK = new EventDescriptor(0x10a, 0, 18, 4, 0, 12, 0x2000000000000008L);
			PowwaEvents.EVENT_DEBUG_CONNECT_USING_COMPUTERNAME = new EventDescriptor(0x303, 0, 17, 4, 0, 3, 0x4000000000000002L);
			PowwaEvents.EVENT_DEBUG_CONNECT_USING_URI = new EventDescriptor(0x304, 0, 17, 4, 0, 3, 0x4000000000000002L);
			PowwaEvents.EVENT_DEBUG_LOG0 = new EventDescriptor(0x403, 0, 17, 4, 0, 8, 0x4000000000000000L);
			PowwaEvents.EVENT_DEBUG_LOG1 = new EventDescriptor(0x404, 0, 17, 4, 0, 8, 0x4000000000000000L);
			PowwaEvents.EVENT_DEBUG_LOG2 = new EventDescriptor(0x405, 0, 17, 4, 0, 8, 0x4000000000000000L);
		}

		public static bool PowwaEVENT_ACTIVITYID_TRANSFER()
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateEventDescriptor(ref PowwaEvents.EVENT_ACTIVITYID_TRANSFER);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_AUTHENTICATION_FAILURE(string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_AUTHENTICATION(ref PowwaEvents.EVENT_AUTHENTICATION_FAILURE, UserName, OriginIpAddressRemoteAddr, OriginIpAddressHttpXForwardedFor, FailureMessage);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_AUTHENTICATION_START(string UserName)
		{
			return PowwaEvents.m_provider.WriteEvent(ref PowwaEvents.EVENT_AUTHENTICATION_START, UserName);
		}

		public static bool PowwaEVENT_AUTHENTICATION_STOP(string UserName, string EndState)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_AUTHENTICATION_STOP(ref PowwaEvents.EVENT_AUTHENTICATION_STOP, UserName, EndState);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_AUTHORIZATION_FAILURE(string UserName, string SourceNode, string FailureMessage, string TargetNode, string TargetNodeUserName, string Port, string ApplicationName, string ConfigurationName)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_AUTHORIZATION(ref PowwaEvents.EVENT_AUTHORIZATION_FAILURE, UserName, SourceNode, FailureMessage, TargetNode, TargetNodeUserName, Port, ApplicationName, ConfigurationName);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_AUTHORIZATION_FAILURE_INVALID_RULES(string Command)
		{
			return PowwaEvents.m_provider.WriteEvent(ref PowwaEvents.EVENT_AUTHORIZATION_FAILURE_INVALID_RULES, Command);
		}

		public static bool PowwaEVENT_DEBUG_CONNECT_USING_COMPUTERNAME(string UserName, string TargetNode, int Port, string ApplicationName, string ConfigurationName, string AuthMechanism)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_DEBUG_CONNECT_USING_COMPUTERNAME(ref PowwaEvents.EVENT_DEBUG_CONNECT_USING_COMPUTERNAME, UserName, TargetNode, Port, ApplicationName, ConfigurationName, AuthMechanism);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_DEBUG_CONNECT_USING_URI(string UserName, string ConnectionURI, string ConfigurationName)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_DEBUG_CONNECT_USING_URI(ref PowwaEvents.EVENT_DEBUG_CONNECT_USING_URI, UserName, ConnectionURI, ConfigurationName);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_DEBUG_LOG0(string Message)
		{
			return PowwaEvents.m_provider.WriteEvent(ref PowwaEvents.EVENT_DEBUG_LOG0, Message);
		}

		public static bool PowwaEVENT_DEBUG_LOG1(string Message, string Key, string Value)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_DEBUG_LOG1(ref PowwaEvents.EVENT_DEBUG_LOG1, Message, Key, Value);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_DEBUG_LOG2(string Message, string Key1, string Value1, string Key2, string Value2)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_DEBUG_LOG2(ref PowwaEvents.EVENT_DEBUG_LOG2, Message, Key1, Value1, Key2, Value2);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_GATEWAY_AUTHORIZATION_FAILURE(string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage, string TargetNode, string ConfigurationName)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_GATEWAY_AUTHORIZATION(ref PowwaEvents.EVENT_GATEWAY_AUTHORIZATION_FAILURE, UserName, OriginIpAddressRemoteAddr, OriginIpAddressHttpXForwardedFor, FailureMessage, TargetNode, ConfigurationName);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_GATEWAY_AUTHORIZATION_START(string UserName)
		{
			return PowwaEvents.m_provider.WriteEvent(ref PowwaEvents.EVENT_GATEWAY_AUTHORIZATION_START, UserName);
		}

		public static bool PowwaEVENT_GATEWAY_AUTHORIZATION_STOP(string UserName, string EndState)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_GATEWAY_AUTHORIZATION_STOP(ref PowwaEvents.EVENT_GATEWAY_AUTHORIZATION_STOP, UserName, EndState);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_GENERIC_FAILURE(string SessionId, string Message)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_FAILURE_MESSAGE(ref PowwaEvents.EVENT_GENERIC_FAILURE, SessionId, Message);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_INVALID_APPLICATION_SETTING(string Name, string Value)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_APPLICATION_SETTING(ref PowwaEvents.EVENT_INVALID_APPLICATION_SETTING, Name, Value);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_INVALID_SESSION_KEY(string SessionId, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_INVALID_SESSION_KEY(ref PowwaEvents.EVENT_INVALID_SESSION_KEY, SessionId, UserName, OriginIpAddressRemoteAddr, OriginIpAddressHttpXForwardedFor);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_INVALID_SESSION_USER(string SessionId, string RequestUser, string SessionUser, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_INVALID_SESSION_USER(ref PowwaEvents.EVENT_INVALID_SESSION_USER, SessionId, RequestUser, SessionUser, OriginIpAddressRemoteAddr, OriginIpAddressHttpXForwardedFor);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_LOGON_FAILURE(string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_LOGON(ref PowwaEvents.EVENT_LOGON_FAILURE, UserName, OriginIpAddressRemoteAddr, OriginIpAddressHttpXForwardedFor, FailureMessage);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_MALICIOUS_DATA(string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string ErrorMessage)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_MALICIOUS_DATA(ref PowwaEvents.EVENT_MALICIOUS_DATA, OriginIpAddressRemoteAddr, OriginIpAddressHttpXForwardedFor, ErrorMessage);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_PSRCONNECTION_FAILURE(string SessionId, string Message)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_FAILURE_MESSAGE(ref PowwaEvents.EVENT_PSRCONNECTION_FAILURE, SessionId, Message);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_PSREXECUTION_FAILURE(string SessionId, string Message)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_FAILURE_MESSAGE(ref PowwaEvents.EVENT_PSREXECUTION_FAILURE, SessionId, Message);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_SESSION_END(string SessionId, string EndType)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_SESSION_END(ref PowwaEvents.EVENT_SESSION_END, SessionId, EndType);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_SESSION_LIMIT_CHECK(string UserName, string EndState)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_SESSION_LIMIT_CHECK(ref PowwaEvents.EVENT_SESSION_LIMIT_CHECK, UserName, EndState);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_SESSION_LIMIT_REACHED(string UserName)
		{
			return PowwaEvents.m_provider.WriteEvent(ref PowwaEvents.EVENT_SESSION_LIMIT_REACHED, UserName);
		}

		public static bool PowwaEVENT_SESSION_START(string SessionId, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string TargetNode, string TargetNodeUserName, int Port, string ApplicationName, string ConfigurationName)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_SESSION_START(ref PowwaEvents.EVENT_SESSION_START, SessionId, UserName, OriginIpAddressRemoteAddr, OriginIpAddressHttpXForwardedFor, TargetNode, TargetNodeUserName, Port, ApplicationName, ConfigurationName);
			}
			else
			{
				return true;
			}
		}

		public static bool PowwaEVENT_TERMINATE_SESSION_ERROR(string UserName, string ErrorMessage)
		{
			if (PowwaEvents.m_provider.IsEnabled())
			{
				return PowwaEvents.m_provider.TemplateT_TERMINATE_SESSION_ERROR(ref PowwaEvents.EVENT_TERMINATE_SESSION_ERROR, UserName, ErrorMessage);
			}
			else
			{
				return true;
			}
		}
	}
}