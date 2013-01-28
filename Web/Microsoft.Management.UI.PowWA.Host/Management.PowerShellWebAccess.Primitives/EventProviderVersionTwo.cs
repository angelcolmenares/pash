using System;
using System.Diagnostics.Eventing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	internal class EventProviderVersionTwo : EventProvider
	{
		internal EventProviderVersionTwo(Guid id) : base(id)
		{

		}
		
		internal bool TemplateEventDescriptor(ref EventDescriptor eventDescriptor)
		{
			if (!base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				return true;
			}
			else
			{
				return base.WriteEvent(ref eventDescriptor, 0, IntPtr.Zero);
			}
		}
		
		internal unsafe bool TemplateT_APPLICATION_SETTING(ref EventDescriptor eventDescriptor, string Name, string Value)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_AUTHENTICATION(ref EventDescriptor eventDescriptor, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_AUTHENTICATION_STOP(ref EventDescriptor eventDescriptor, string UserName, string EndState)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_AUTHORIZATION (ref EventDescriptor eventDescriptor, string UserName, string SourceNode, string FailureMessage, string TargetNode, string TargetNodeUserName, string Port, string ApplicationName, string ConfigurationName)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_DEBUG_CONNECT_USING_COMPUTERNAME(ref EventDescriptor eventDescriptor, string UserName, string TargetNode, int Port, string ApplicationName, string ConfigurationName, string AuthMechanism)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_DEBUG_CONNECT_USING_URI(ref EventDescriptor eventDescriptor, string UserName, string ConnectionURI, string ConfigurationName)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_DEBUG_LOG1(ref EventDescriptor eventDescriptor, string Message, string Key, string Value)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_DEBUG_LOG2(ref EventDescriptor eventDescriptor, string Message, string Key1, string Value1, string Key2, string Value2)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_FAILURE_MESSAGE(ref EventDescriptor eventDescriptor, string SessionId, string Message)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_GATEWAY_AUTHORIZATION(ref EventDescriptor eventDescriptor, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage, string TargetNode, string ConfigurationName)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_GATEWAY_AUTHORIZATION_STOP(ref EventDescriptor eventDescriptor, string UserName, string EndState)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_INVALID_SESSION_KEY(ref EventDescriptor eventDescriptor, string SessionId, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_INVALID_SESSION_USER(ref EventDescriptor eventDescriptor, string SessionId, string RequestUser, string SessionUser, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_LOGON(ref EventDescriptor eventDescriptor, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_MALICIOUS_DATA(ref EventDescriptor eventDescriptor, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string ErrorMessage)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_SESSION_END (ref EventDescriptor eventDescriptor, string SessionId, string EndType)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_SESSION_LIMIT_CHECK(ref EventDescriptor eventDescriptor, string UserName, string EndState)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_SESSION_START(ref EventDescriptor eventDescriptor, string SessionId, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string TargetNode, string TargetNodeUserName, int Port, string ApplicationName, string ConfigurationName)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {
				
			}
			return flag;
		}

		internal unsafe bool TemplateT_TERMINATE_SESSION_ERROR (ref EventDescriptor eventDescriptor, string UserName, string ErrorMessage)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled (eventDescriptor.Level, eventDescriptor.Keywords)) {

			}
			return flag;
		}



		[StructLayout(LayoutKind.Explicit)]
		private struct EventData
		{
			[FieldOffset(0)]
			internal ulong DataPointer;
			
			[FieldOffset(8)]
			internal uint Size;
			
			[FieldOffset(12)]
			internal int Reserved;
			
		}
	}

	/*
	internal class EventProviderVersionTwo : EventProvider
	{
		internal EventProviderVersionTwo(Guid id) : base(id)
		{
		}

		internal bool TemplateEventDescriptor(ref EventDescriptor eventDescriptor)
		{
			if (!base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				return true;
			}
			else
			{
				return base.WriteEvent(ref eventDescriptor, 0, IntPtr.Zero);
			}
		}

		internal unsafe bool TemplateT_APPLICATION_SETTING(ref EventDescriptor eventDescriptor, string Name, string Value)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (Name.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (Value.Length + 1) * 2;
				fixed (string name = Name)
				{
					string* strPointers = &name;
					char* offsetToStringData = (char*)(&name);
					if (&name != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string value = Value)
					{
						string* strPointers1 = &value;
						char* offsetToStringData1 = (char*)(&value);
						if (&value != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
						(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData1;
						flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_AUTHENTICATION(ref EventDescriptor eventDescriptor, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage)
		{
			int num = 4;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressRemoteAddr.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressHttpXForwardedFor.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (FailureMessage.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string originIpAddressRemoteAddr = OriginIpAddressRemoteAddr)
					{
						string* strPointers1 = &originIpAddressRemoteAddr;
						char* offsetToStringData1 = (char*)(&originIpAddressRemoteAddr);
						if (&originIpAddressRemoteAddr != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string originIpAddressHttpXForwardedFor = OriginIpAddressHttpXForwardedFor)
						{
							string* strPointers2 = &originIpAddressHttpXForwardedFor;
							char* offsetToStringData2 = (char*)(&originIpAddressHttpXForwardedFor);
							if (&originIpAddressHttpXForwardedFor != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string failureMessage = FailureMessage)
							{
								string* strPointers3 = &failureMessage;
								char* offsetToStringData3 = (char*)(&failureMessage);
								if (&failureMessage != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
								(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
								(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
								(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData3;
								flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_AUTHENTICATION_STOP(ref EventDescriptor eventDescriptor, string UserName, string EndState)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (EndState.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string endState = EndState)
					{
						string* strPointers1 = &endState;
						char* offsetToStringData1 = (char*)(&endState);
						if (&endState != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
						(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData1;
						flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_AUTHORIZATION(ref EventDescriptor eventDescriptor, string UserName, string SourceNode, string FailureMessage, string TargetNode, string TargetNodeUserName, string Port, string ApplicationName, string ConfigurationName)
		{
			int num = 8;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (SourceNode.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (FailureMessage.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (TargetNode.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 4 * sizeof(EventProviderVersionTwo.EventData)).Size = (TargetNodeUserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 5 * sizeof(EventProviderVersionTwo.EventData)).Size = (Port.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 6 * sizeof(EventProviderVersionTwo.EventData)).Size = (ApplicationName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 7 * sizeof(EventProviderVersionTwo.EventData)).Size = (ConfigurationName.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string sourceNode = SourceNode)
					{
						string* strPointers1 = &sourceNode;
						char* offsetToStringData1 = (char*)(&sourceNode);
						if (&sourceNode != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string failureMessage = FailureMessage)
						{
							string* strPointers2 = &failureMessage;
							char* offsetToStringData2 = (char*)(&failureMessage);
							if (&failureMessage != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string targetNode = TargetNode)
							{
								string* strPointers3 = &targetNode;
								char* offsetToStringData3 = (char*)(&targetNode);
								if (&targetNode != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer3 = offsetToStringData3;
								fixed (string targetNodeUserName = TargetNodeUserName)
								{
									string* strPointers4 = &targetNodeUserName;
									char* offsetToStringData4 = (char*)(&targetNodeUserName);
									if (&targetNodeUserName != null)
									{
										offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer4 = offsetToStringData4;
									fixed (string port = Port)
									{
										string* strPointers5 = &port;
										char* offsetToStringData5 = (char*)(&port);
										if (&port != null)
										{
											offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer5 = offsetToStringData5;
										fixed (string applicationName = ApplicationName)
										{
											string* strPointers6 = &applicationName;
											char* offsetToStringData6 = (char*)(&applicationName);
											if (&applicationName != null)
											{
												offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer6 = offsetToStringData6;
											fixed (string configurationName = ConfigurationName)
											{
												string* strPointers7 = &configurationName;
												char* offsetToStringData7 = (char*)(&configurationName);
												if (&configurationName != null)
												{
													offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
												}
												(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
												(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
												(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
												(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer3;
												(eventDataPointer + 4 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer4;
												(eventDataPointer + 5 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer5;
												(eventDataPointer + 6 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer6;
												(eventDataPointer + 7 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData7;
												flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_DEBUG_CONNECT_USING_COMPUTERNAME(ref EventDescriptor eventDescriptor, string UserName, string TargetNode, int Port, string ApplicationName, string ConfigurationName, string AuthMechanism)
		{
			int num = 6;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (TargetNode.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)(&Port);
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = 4;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (ApplicationName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 4 * sizeof(EventProviderVersionTwo.EventData)).Size = (ConfigurationName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 5 * sizeof(EventProviderVersionTwo.EventData)).Size = (AuthMechanism.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string targetNode = TargetNode)
					{
						string* strPointers1 = &targetNode;
						char* offsetToStringData1 = (char*)(&targetNode);
						if (&targetNode != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string applicationName = ApplicationName)
						{
							string* strPointers2 = &applicationName;
							char* offsetToStringData2 = (char*)(&applicationName);
							if (&applicationName != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string configurationName = ConfigurationName)
							{
								string* strPointers3 = &configurationName;
								char* offsetToStringData3 = (char*)(&configurationName);
								if (&configurationName != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer3 = offsetToStringData3;
								fixed (string authMechanism = AuthMechanism)
								{
									string* strPointers4 = &authMechanism;
									char* offsetToStringData4 = (char*)(&authMechanism);
									if (&authMechanism != null)
									{
										offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
									}
									(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
									(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
									(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
									(eventDataPointer + 4 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer3;
									(eventDataPointer + 5 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData4;
									flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
								}
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_DEBUG_CONNECT_USING_URI(ref EventDescriptor eventDescriptor, string UserName, string ConnectionURI, string ConfigurationName)
		{
			int num = 3;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (ConnectionURI.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (ConfigurationName.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string connectionURI = ConnectionURI)
					{
						string* strPointers1 = &connectionURI;
						char* offsetToStringData1 = (char*)(&connectionURI);
						if (&connectionURI != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string configurationName = ConfigurationName)
						{
							string* strPointers2 = &configurationName;
							char* offsetToStringData2 = (char*)(&configurationName);
							if (&configurationName != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
							(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
							(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData2;
							flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_DEBUG_LOG1(ref EventDescriptor eventDescriptor, string Message, string Key, string Value)
		{
			int num = 3;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (Message.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (Key.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (Value.Length + 1) * 2;
				fixed (string message = Message)
				{
					string* strPointers = &message;
					char* offsetToStringData = (char*)(&message);
					if (&message != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string key = Key)
					{
						string* strPointers1 = &key;
						char* offsetToStringData1 = (char*)(&key);
						if (&key != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string value = Value)
						{
							string* strPointers2 = &value;
							char* offsetToStringData2 = (char*)(&value);
							if (&value != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
							(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
							(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData2;
							flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_DEBUG_LOG2(ref EventDescriptor eventDescriptor, string Message, string Key1, string Value1, string Key2, string Value2)
		{
			int num = 5;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (Message.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (Key1.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (Value1.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (Key2.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 4 * sizeof(EventProviderVersionTwo.EventData)).Size = (Value2.Length + 1) * 2;
				fixed (string message = Message)
				{
					string* strPointers = &message;
					char* offsetToStringData = (char*)(&message);
					if (&message != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string key1 = Key1)
					{
						string* strPointers1 = &key1;
						char* offsetToStringData1 = (char*)(&key1);
						if (&key1 != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string value1 = Value1)
						{
							string* strPointers2 = &value1;
							char* offsetToStringData2 = (char*)(&value1);
							if (&value1 != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string key2 = Key2)
							{
								string* strPointers3 = &key2;
								char* offsetToStringData3 = (char*)(&key2);
								if (&key2 != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer3 = offsetToStringData3;
								fixed (string value2 = Value2)
								{
									string* strPointers4 = &value2;
									char* offsetToStringData4 = (char*)(&value2);
									if (&value2 != null)
									{
										offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
									}
									(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
									(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
									(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
									(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer3;
									(eventDataPointer + 4 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData4;
									flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
								}
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_FAILURE_MESSAGE(ref EventDescriptor eventDescriptor, string SessionId, string Message)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (SessionId.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (Message.Length + 1) * 2;
				fixed (string sessionId = SessionId)
				{
					string* strPointers = &sessionId;
					char* offsetToStringData = (char*)(&sessionId);
					if (&sessionId != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string message = Message)
					{
						string* strPointers1 = &message;
						char* offsetToStringData1 = (char*)(&message);
						if (&message != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
						(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData1;
						flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_GATEWAY_AUTHORIZATION(ref EventDescriptor eventDescriptor, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage, string TargetNode, string ConfigurationName)
		{
			int num = 6;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressRemoteAddr.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressHttpXForwardedFor.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (FailureMessage.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 4 * sizeof(EventProviderVersionTwo.EventData)).Size = (TargetNode.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 5 * sizeof(EventProviderVersionTwo.EventData)).Size = (ConfigurationName.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string originIpAddressRemoteAddr = OriginIpAddressRemoteAddr)
					{
						string* strPointers1 = &originIpAddressRemoteAddr;
						char* offsetToStringData1 = (char*)(&originIpAddressRemoteAddr);
						if (&originIpAddressRemoteAddr != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string originIpAddressHttpXForwardedFor = OriginIpAddressHttpXForwardedFor)
						{
							string* strPointers2 = &originIpAddressHttpXForwardedFor;
							char* offsetToStringData2 = (char*)(&originIpAddressHttpXForwardedFor);
							if (&originIpAddressHttpXForwardedFor != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string failureMessage = FailureMessage)
							{
								string* strPointers3 = &failureMessage;
								char* offsetToStringData3 = (char*)(&failureMessage);
								if (&failureMessage != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer3 = offsetToStringData3;
								fixed (string targetNode = TargetNode)
								{
									string* strPointers4 = &targetNode;
									char* offsetToStringData4 = (char*)(&targetNode);
									if (&targetNode != null)
									{
										offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer4 = offsetToStringData4;
									fixed (string configurationName = ConfigurationName)
									{
										string* strPointers5 = &configurationName;
										char* offsetToStringData5 = (char*)(&configurationName);
										if (&configurationName != null)
										{
											offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
										}
										(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
										(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
										(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
										(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer3;
										(eventDataPointer + 4 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer4;
										(eventDataPointer + 5 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData5;
										flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
									}
								}
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_GATEWAY_AUTHORIZATION_STOP(ref EventDescriptor eventDescriptor, string UserName, string EndState)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (EndState.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string endState = EndState)
					{
						string* strPointers1 = &endState;
						char* offsetToStringData1 = (char*)(&endState);
						if (&endState != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
						(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData1;
						flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_INVALID_SESSION_KEY(ref EventDescriptor eventDescriptor, string SessionId, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor)
		{
			int num = 4;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (SessionId.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressRemoteAddr.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressHttpXForwardedFor.Length + 1) * 2;
				fixed (string sessionId = SessionId)
				{
					string* strPointers = &sessionId;
					char* offsetToStringData = (char*)(&sessionId);
					if (&sessionId != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string userName = UserName)
					{
						string* strPointers1 = &userName;
						char* offsetToStringData1 = (char*)(&userName);
						if (&userName != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string originIpAddressRemoteAddr = OriginIpAddressRemoteAddr)
						{
							string* strPointers2 = &originIpAddressRemoteAddr;
							char* offsetToStringData2 = (char*)(&originIpAddressRemoteAddr);
							if (&originIpAddressRemoteAddr != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string originIpAddressHttpXForwardedFor = OriginIpAddressHttpXForwardedFor)
							{
								string* strPointers3 = &originIpAddressHttpXForwardedFor;
								char* offsetToStringData3 = (char*)(&originIpAddressHttpXForwardedFor);
								if (&originIpAddressHttpXForwardedFor != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
								(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
								(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
								(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData3;
								flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_INVALID_SESSION_USER(ref EventDescriptor eventDescriptor, string SessionId, string RequestUser, string SessionUser, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor)
		{
			int num = 5;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (SessionId.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (RequestUser.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (SessionUser.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressRemoteAddr.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 4 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressHttpXForwardedFor.Length + 1) * 2;
				fixed (string sessionId = SessionId)
				{
					string* strPointers = &sessionId;
					char* offsetToStringData = (char*)(&sessionId);
					if (&sessionId != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string requestUser = RequestUser)
					{
						string* strPointers1 = &requestUser;
						char* offsetToStringData1 = (char*)(&requestUser);
						if (&requestUser != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string sessionUser = SessionUser)
						{
							string* strPointers2 = &sessionUser;
							char* offsetToStringData2 = (char*)(&sessionUser);
							if (&sessionUser != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string originIpAddressRemoteAddr = OriginIpAddressRemoteAddr)
							{
								string* strPointers3 = &originIpAddressRemoteAddr;
								char* offsetToStringData3 = (char*)(&originIpAddressRemoteAddr);
								if (&originIpAddressRemoteAddr != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer3 = offsetToStringData3;
								fixed (string originIpAddressHttpXForwardedFor = OriginIpAddressHttpXForwardedFor)
								{
									string* strPointers4 = &originIpAddressHttpXForwardedFor;
									char* offsetToStringData4 = (char*)(&originIpAddressHttpXForwardedFor);
									if (&originIpAddressHttpXForwardedFor != null)
									{
										offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
									}
									(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
									(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
									(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
									(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer3;
									(eventDataPointer + 4 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData4;
									flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
								}
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_LOGON(ref EventDescriptor eventDescriptor, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string FailureMessage)
		{
			int num = 4;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressRemoteAddr.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressHttpXForwardedFor.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (FailureMessage.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string originIpAddressRemoteAddr = OriginIpAddressRemoteAddr)
					{
						string* strPointers1 = &originIpAddressRemoteAddr;
						char* offsetToStringData1 = (char*)(&originIpAddressRemoteAddr);
						if (&originIpAddressRemoteAddr != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string originIpAddressHttpXForwardedFor = OriginIpAddressHttpXForwardedFor)
						{
							string* strPointers2 = &originIpAddressHttpXForwardedFor;
							char* offsetToStringData2 = (char*)(&originIpAddressHttpXForwardedFor);
							if (&originIpAddressHttpXForwardedFor != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string failureMessage = FailureMessage)
							{
								string* strPointers3 = &failureMessage;
								char* offsetToStringData3 = (char*)(&failureMessage);
								if (&failureMessage != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
								(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
								(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
								(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData3;
								flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_MALICIOUS_DATA(ref EventDescriptor eventDescriptor, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string ErrorMessage)
		{
			int num = 3;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (OriginIpAddressRemoteAddr.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressHttpXForwardedFor.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (ErrorMessage.Length + 1) * 2;
				fixed (string originIpAddressRemoteAddr = OriginIpAddressRemoteAddr)
				{
					string* strPointers = &originIpAddressRemoteAddr;
					char* offsetToStringData = (char*)(&originIpAddressRemoteAddr);
					if (&originIpAddressRemoteAddr != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string originIpAddressHttpXForwardedFor = OriginIpAddressHttpXForwardedFor)
					{
						string* strPointers1 = &originIpAddressHttpXForwardedFor;
						char* offsetToStringData1 = (char*)(&originIpAddressHttpXForwardedFor);
						if (&originIpAddressHttpXForwardedFor != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string errorMessage = ErrorMessage)
						{
							string* strPointers2 = &errorMessage;
							char* offsetToStringData2 = (char*)(&errorMessage);
							if (&errorMessage != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
							(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
							(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData2;
							flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_SESSION_END(ref EventDescriptor eventDescriptor, string SessionId, string EndType)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (SessionId.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (EndType.Length + 1) * 2;
				fixed (string sessionId = SessionId)
				{
					string* strPointers = &sessionId;
					char* offsetToStringData = (char*)(&sessionId);
					if (&sessionId != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string endType = EndType)
					{
						string* strPointers1 = &endType;
						char* offsetToStringData1 = (char*)(&endType);
						if (&endType != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
						(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData1;
						flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_SESSION_LIMIT_CHECK(ref EventDescriptor eventDescriptor, string UserName, string EndState)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (EndState.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string endState = EndState)
					{
						string* strPointers1 = &endState;
						char* offsetToStringData1 = (char*)(&endState);
						if (&endState != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
						(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData1;
						flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_SESSION_START(ref EventDescriptor eventDescriptor, string SessionId, string UserName, string OriginIpAddressRemoteAddr, string OriginIpAddressHttpXForwardedFor, string TargetNode, string TargetNodeUserName, int Port, string ApplicationName, string ConfigurationName)
		{
			int num = 9;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (SessionId.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 2 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressRemoteAddr.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 3 * sizeof(EventProviderVersionTwo.EventData)).Size = (OriginIpAddressHttpXForwardedFor.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 4 * sizeof(EventProviderVersionTwo.EventData)).Size = (TargetNode.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 5 * sizeof(EventProviderVersionTwo.EventData)).Size = (TargetNodeUserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 6 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)(&Port);
				((EventProviderVersionTwo.EventData*)length + 6 * sizeof(EventProviderVersionTwo.EventData)).Size = 4;
				((EventProviderVersionTwo.EventData*)length + 7 * sizeof(EventProviderVersionTwo.EventData)).Size = (ApplicationName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + 8 * sizeof(EventProviderVersionTwo.EventData)).Size = (ConfigurationName.Length + 1) * 2;
				fixed (string sessionId = SessionId)
				{
					string* strPointers = &sessionId;
					char* offsetToStringData = (char*)(&sessionId);
					if (&sessionId != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string userName = UserName)
					{
						string* strPointers1 = &userName;
						char* offsetToStringData1 = (char*)(&userName);
						if (&userName != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer1 = offsetToStringData1;
						fixed (string originIpAddressRemoteAddr = OriginIpAddressRemoteAddr)
						{
							string* strPointers2 = &originIpAddressRemoteAddr;
							char* offsetToStringData2 = (char*)(&originIpAddressRemoteAddr);
							if (&originIpAddressRemoteAddr != null)
							{
								offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer2 = offsetToStringData2;
							fixed (string originIpAddressHttpXForwardedFor = OriginIpAddressHttpXForwardedFor)
							{
								string* strPointers3 = &originIpAddressHttpXForwardedFor;
								char* offsetToStringData3 = (char*)(&originIpAddressHttpXForwardedFor);
								if (&originIpAddressHttpXForwardedFor != null)
								{
									offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer3 = offsetToStringData3;
								fixed (string targetNode = TargetNode)
								{
									string* strPointers4 = &targetNode;
									char* offsetToStringData4 = (char*)(&targetNode);
									if (&targetNode != null)
									{
										offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer4 = offsetToStringData4;
									fixed (string targetNodeUserName = TargetNodeUserName)
									{
										string* strPointers5 = &targetNodeUserName;
										char* offsetToStringData5 = (char*)(&targetNodeUserName);
										if (&targetNodeUserName != null)
										{
											offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer5 = offsetToStringData5;
										fixed (string applicationName = ApplicationName)
										{
											string* strPointers6 = &applicationName;
											char* offsetToStringData6 = (char*)(&applicationName);
											if (&applicationName != null)
											{
												offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer6 = offsetToStringData6;
											fixed (string configurationName = ConfigurationName)
											{
												string* strPointers7 = &configurationName;
												char* offsetToStringData7 = (char*)(&configurationName);
												if (&configurationName != null)
												{
													offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
												}
												(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
												(eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer1;
												(eventDataPointer + 2 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer2;
												(eventDataPointer + 3 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer3;
												(eventDataPointer + 4 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer4;
												(eventDataPointer + 5 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer5;
												(eventDataPointer + 7 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)chrPointer6;
												(eventDataPointer + 8 * sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData7;
												flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return flag;
		}

		internal unsafe bool TemplateT_TERMINATE_SESSION_ERROR(ref EventDescriptor eventDescriptor, string UserName, string ErrorMessage)
		{
			int num = 2;
			bool flag = true;
			if (base.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				byte* length = (byte*)sizeof(EventProviderVersionTwo.EventData) * num;
				EventProviderVersionTwo.EventData* eventDataPointer = (EventProviderVersionTwo.EventData*)length;
				(*((EventProviderVersionTwo.EventData*)length)).Size = (UserName.Length + 1) * 2;
				((EventProviderVersionTwo.EventData*)length + sizeof(EventProviderVersionTwo.EventData)).Size = (ErrorMessage.Length + 1) * 2;
				fixed (string userName = UserName)
				{
					string* strPointers = &userName;
					char* offsetToStringData = (char*)(&userName);
					if (&userName != null)
					{
						offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer = offsetToStringData;
					fixed (string errorMessage = ErrorMessage)
					{
						string* strPointers1 = &errorMessage;
						char* offsetToStringData1 = (char*)(&errorMessage);
						if (&errorMessage != null)
						{
							offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
						}
						(*(eventDataPointer)).DataPointer = (ulong)chrPointer;
						//TODO: FIX: (eventDataPointer + sizeof(EventProviderVersionTwo.EventData)).DataPointer = (ulong)offsetToStringData1;
						flag = base.WriteEvent(ref eventDescriptor, num, (IntPtr)length);
					}
				}
			}
			return flag;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct EventData
		{
			[FieldOffset(0)]
			internal ulong DataPointer;

			[FieldOffset(8)]
			internal uint Size;

			[FieldOffset(12)]
			internal int Reserved;

		}
	}
	*/
}