using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Interop;
using System.Security;
using System.Security.Permissions;

namespace System.Runtime.Diagnostics
{
	internal sealed class EtwProvider : DiagnosticsEventProvider
	{
		private Action invokeControllerCallback;

		private bool end2EndActivityTracingEnabled;

		internal Action ControllerCallBack
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.invokeControllerCallback;
			}
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			set
			{
				this.invokeControllerCallback = value;
			}
		}

		internal bool IsEnd2EndActivityTracingEnabled
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.end2EndActivityTracingEnabled;
			}
		}

		[PermissionSet(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal EtwProvider(Guid id) : base(id)
		{
		}

		protected override void OnControllerCommand()
		{
			this.end2EndActivityTracingEnabled = false;
			if (this.invokeControllerCallback != null)
			{
				this.invokeControllerCallback();
			}
		}

		internal void SetEnd2EndActivityTracingEnabled(bool isEnd2EndActivityTracingEnabled)
		{
			this.end2EndActivityTracingEnabled = isEnd2EndActivityTracingEnabled;
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, string value2, string value3)
		{
			string str = value2;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value2 = empty;
			string str1 = value3;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value3 = empty1;
			fixed (string str2 = value2)
			{
				string* strPointers = &str2;
				char* offsetToStringData = (char*)(&str2);
				if (&str2 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str3 = value3)
				{
					string* strPointers1 = &str3;
					char* offsetToStringData1 = (char*)(&str3);
					if (&str3 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 3;
					(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)(&value1);
					(*((UnsafeNativeMethods.EventData*)length)).Size = sizeof(Guid);
					((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
					((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
					((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData1;
					((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
					bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 3, (IntPtr)length);
					return flag;
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			fixed (string str2 = value1)
			{
				string* strPointers = &str2;
				char* offsetToStringData = (char*)(&str2);
				if (&str2 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str3 = value2)
				{
					string* strPointers1 = &str3;
					char* offsetToStringData1 = (char*)(&str3);
					if (&str3 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 2;
					(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
					(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
					((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData1;
					((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
					bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 2, (IntPtr)length);
					return flag;
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			fixed (string str3 = value1)
			{
				string* strPointers = &str3;
				char* offsetToStringData = (char*)(&str3);
				if (&str3 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str4 = value2)
				{
					string* strPointers1 = &str4;
					char* offsetToStringData1 = (char*)(&str4);
					if (&str4 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str5 = value3)
					{
						string* strPointers2 = &str5;
						char* offsetToStringData2 = (char*)(&str5);
						if (&str5 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 3;
						(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
						(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
						((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
						((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
						((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData2;
						((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
						bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 3, (IntPtr)length);
						return flag;
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			fixed (string str4 = value1)
			{
				string* strPointers = &str4;
				char* offsetToStringData = (char*)(&str4);
				if (&str4 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str5 = value2)
				{
					string* strPointers1 = &str5;
					char* offsetToStringData1 = (char*)(&str5);
					if (&str5 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str6 = value3)
					{
						string* strPointers2 = &str6;
						char* offsetToStringData2 = (char*)(&str6);
						if (&str6 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str7 = value4)
						{
							string* strPointers3 = &str7;
							char* offsetToStringData3 = (char*)(&str7);
							if (&str7 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 4;
							(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
							(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
							((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
							((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
							((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
							((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
							((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData3;
							((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
							bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 4, (IntPtr)length);
							return flag;
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			fixed (string str5 = value1)
			{
				string* strPointers = &str5;
				char* offsetToStringData = (char*)(&str5);
				if (&str5 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str6 = value2)
				{
					string* strPointers1 = &str6;
					char* offsetToStringData1 = (char*)(&str6);
					if (&str6 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str7 = value3)
					{
						string* strPointers2 = &str7;
						char* offsetToStringData2 = (char*)(&str7);
						if (&str7 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str8 = value4)
						{
							string* strPointers3 = &str8;
							char* offsetToStringData3 = (char*)(&str8);
							if (&str8 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str9 = value5)
							{
								string* strPointers4 = &str9;
								char* offsetToStringData4 = (char*)(&str9);
								if (&str9 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 5;
								(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
								(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
								((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
								((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
								((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
								((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
								((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
								((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
								((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData4;
								((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
								bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 5, (IntPtr)length);
								return flag;
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5, string value6)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			string str5 = value6;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value6 = empty5;
			fixed (string str6 = value1)
			{
				string* strPointers = &str6;
				char* offsetToStringData = (char*)(&str6);
				if (&str6 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str7 = value2)
				{
					string* strPointers1 = &str7;
					char* offsetToStringData1 = (char*)(&str7);
					if (&str7 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str8 = value3)
					{
						string* strPointers2 = &str8;
						char* offsetToStringData2 = (char*)(&str8);
						if (&str8 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str9 = value4)
						{
							string* strPointers3 = &str9;
							char* offsetToStringData3 = (char*)(&str9);
							if (&str9 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str10 = value5)
							{
								string* strPointers4 = &str10;
								char* offsetToStringData4 = (char*)(&str10);
								if (&str10 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str11 = value6)
								{
									string* strPointers5 = &str11;
									char* offsetToStringData5 = (char*)(&str11);
									if (&str11 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 6;
									(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
									(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
									((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
									((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
									((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
									((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData5;
									((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
									bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 6, (IntPtr)length);
									return flag;
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5, string value6, string value7)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			string str5 = value6;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value6 = empty5;
			string str6 = value7;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value7 = empty6;
			fixed (string str7 = value1)
			{
				string* strPointers = &str7;
				char* offsetToStringData = (char*)(&str7);
				if (&str7 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str8 = value2)
				{
					string* strPointers1 = &str8;
					char* offsetToStringData1 = (char*)(&str8);
					if (&str8 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str9 = value3)
					{
						string* strPointers2 = &str9;
						char* offsetToStringData2 = (char*)(&str9);
						if (&str9 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str10 = value4)
						{
							string* strPointers3 = &str10;
							char* offsetToStringData3 = (char*)(&str10);
							if (&str10 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str11 = value5)
							{
								string* strPointers4 = &str11;
								char* offsetToStringData4 = (char*)(&str11);
								if (&str11 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str12 = value6)
								{
									string* strPointers5 = &str12;
									char* offsetToStringData5 = (char*)(&str12);
									if (&str12 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str13 = value7)
									{
										string* strPointers6 = &str13;
										char* offsetToStringData6 = (char*)(&str13);
										if (&str13 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 7;
										(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
										(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
										((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
										((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
										((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
										((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
										((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
										((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
										((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
										((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
										((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
										((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
										((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData6;
										((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
										bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 7, (IntPtr)length);
										return flag;
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			string str5 = value6;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value6 = empty5;
			string str6 = value7;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value7 = empty6;
			string str7 = value8;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value8 = empty7;
			fixed (string str8 = value1)
			{
				string* strPointers = &str8;
				char* offsetToStringData = (char*)(&str8);
				if (&str8 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str9 = value2)
				{
					string* strPointers1 = &str9;
					char* offsetToStringData1 = (char*)(&str9);
					if (&str9 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str10 = value3)
					{
						string* strPointers2 = &str10;
						char* offsetToStringData2 = (char*)(&str10);
						if (&str10 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str11 = value4)
						{
							string* strPointers3 = &str11;
							char* offsetToStringData3 = (char*)(&str11);
							if (&str11 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str12 = value5)
							{
								string* strPointers4 = &str12;
								char* offsetToStringData4 = (char*)(&str12);
								if (&str12 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str13 = value6)
								{
									string* strPointers5 = &str13;
									char* offsetToStringData5 = (char*)(&str13);
									if (&str13 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str14 = value7)
									{
										string* strPointers6 = &str14;
										char* offsetToStringData6 = (char*)(&str14);
										if (&str14 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str15 = value8)
										{
											string* strPointers7 = &str15;
											char* offsetToStringData7 = (char*)(&str15);
											if (&str15 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 8;
											(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
											(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
											((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
											((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
											((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
											((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
											((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
											((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData7;
											((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
											bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 8, (IntPtr)length);
											return flag;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			string str5 = value6;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value6 = empty5;
			string str6 = value7;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value7 = empty6;
			string str7 = value8;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value8 = empty7;
			string str8 = value9;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value9 = empty8;
			fixed (string str9 = value1)
			{
				string* strPointers = &str9;
				char* offsetToStringData = (char*)(&str9);
				if (&str9 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str10 = value2)
				{
					string* strPointers1 = &str10;
					char* offsetToStringData1 = (char*)(&str10);
					if (&str10 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str11 = value3)
					{
						string* strPointers2 = &str11;
						char* offsetToStringData2 = (char*)(&str11);
						if (&str11 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str12 = value4)
						{
							string* strPointers3 = &str12;
							char* offsetToStringData3 = (char*)(&str12);
							if (&str12 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str13 = value5)
							{
								string* strPointers4 = &str13;
								char* offsetToStringData4 = (char*)(&str13);
								if (&str13 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str14 = value6)
								{
									string* strPointers5 = &str14;
									char* offsetToStringData5 = (char*)(&str14);
									if (&str14 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str15 = value7)
									{
										string* strPointers6 = &str15;
										char* offsetToStringData6 = (char*)(&str15);
										if (&str15 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str16 = value8)
										{
											string* strPointers7 = &str16;
											char* offsetToStringData7 = (char*)(&str16);
											if (&str16 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str17 = value9)
											{
												string* strPointers8 = &str17;
												char* offsetToStringData8 = (char*)(&str17);
												if (&str17 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 9;
												(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
												(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
												((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
												((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
												((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
												((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
												((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
												((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
												((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData8;
												((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
												bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 9, (IntPtr)length);
												return flag;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			string str5 = value6;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value6 = empty5;
			string str6 = value7;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value7 = empty6;
			string str7 = value8;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value8 = empty7;
			string str8 = value9;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value9 = empty8;
			string str9 = value10;
			string empty9 = str9;
			if (str9 == null)
			{
				empty9 = string.Empty;
			}
			value10 = empty9;
			fixed (string str10 = value1)
			{
				string* strPointers = &str10;
				char* offsetToStringData = (char*)(&str10);
				if (&str10 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str11 = value2)
				{
					string* strPointers1 = &str11;
					char* offsetToStringData1 = (char*)(&str11);
					if (&str11 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str12 = value3)
					{
						string* strPointers2 = &str12;
						char* offsetToStringData2 = (char*)(&str12);
						if (&str12 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str13 = value4)
						{
							string* strPointers3 = &str13;
							char* offsetToStringData3 = (char*)(&str13);
							if (&str13 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str14 = value5)
							{
								string* strPointers4 = &str14;
								char* offsetToStringData4 = (char*)(&str14);
								if (&str14 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str15 = value6)
								{
									string* strPointers5 = &str15;
									char* offsetToStringData5 = (char*)(&str15);
									if (&str15 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str16 = value7)
									{
										string* strPointers6 = &str16;
										char* offsetToStringData6 = (char*)(&str16);
										if (&str16 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str17 = value8)
										{
											string* strPointers7 = &str17;
											char* offsetToStringData7 = (char*)(&str17);
											if (&str17 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str18 = value9)
											{
												string* strPointers8 = &str18;
												char* offsetToStringData8 = (char*)(&str18);
												if (&str18 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer8 = offsetToStringData8;
												fixed (string str19 = value10)
												{
													string* strPointers9 = &str19;
													char* offsetToStringData9 = (char*)(&str19);
													if (&str19 != null)
													{
														offsetToStringData9 = (char*)(strPointers9 + RuntimeHelpers.OffsetToStringData);
													}
													byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 10;
													(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
													(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
													((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
													((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
													((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
													((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
													((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
													((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
													((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
													((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData9;
													((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
													bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 10, (IntPtr)length);
													return flag;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			string str5 = value6;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value6 = empty5;
			string str6 = value7;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value7 = empty6;
			string str7 = value8;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value8 = empty7;
			string str8 = value9;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value9 = empty8;
			string str9 = value10;
			string empty9 = str9;
			if (str9 == null)
			{
				empty9 = string.Empty;
			}
			value10 = empty9;
			string str10 = value11;
			string empty10 = str10;
			if (str10 == null)
			{
				empty10 = string.Empty;
			}
			value11 = empty10;
			fixed (string str11 = value1)
			{
				string* strPointers = &str11;
				char* offsetToStringData = (char*)(&str11);
				if (&str11 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str12 = value2)
				{
					string* strPointers1 = &str12;
					char* offsetToStringData1 = (char*)(&str12);
					if (&str12 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str13 = value3)
					{
						string* strPointers2 = &str13;
						char* offsetToStringData2 = (char*)(&str13);
						if (&str13 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str14 = value4)
						{
							string* strPointers3 = &str14;
							char* offsetToStringData3 = (char*)(&str14);
							if (&str14 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str15 = value5)
							{
								string* strPointers4 = &str15;
								char* offsetToStringData4 = (char*)(&str15);
								if (&str15 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str16 = value6)
								{
									string* strPointers5 = &str16;
									char* offsetToStringData5 = (char*)(&str16);
									if (&str16 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str17 = value7)
									{
										string* strPointers6 = &str17;
										char* offsetToStringData6 = (char*)(&str17);
										if (&str17 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str18 = value8)
										{
											string* strPointers7 = &str18;
											char* offsetToStringData7 = (char*)(&str18);
											if (&str18 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str19 = value9)
											{
												string* strPointers8 = &str19;
												char* offsetToStringData8 = (char*)(&str19);
												if (&str19 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer8 = offsetToStringData8;
												fixed (string str20 = value10)
												{
													string* strPointers9 = &str20;
													char* offsetToStringData9 = (char*)(&str20);
													if (&str20 != null)
													{
														offsetToStringData9 = (char*)(strPointers9 + RuntimeHelpers.OffsetToStringData);
													}
													char* chrPointer9 = offsetToStringData9;
													fixed (string str21 = value11)
													{
														string* strPointers10 = &str21;
														char* offsetToStringData10 = (char*)(&str21);
														if (&str21 != null)
														{
															offsetToStringData10 = (char*)(strPointers10 + RuntimeHelpers.OffsetToStringData);
														}
														byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 11;
														(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
														(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
														((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
														((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
														((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
														((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
														((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
														((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
														((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
														((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer9;
														((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData10;
														((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
														bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 11, (IntPtr)length);
														return flag;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			string str5 = value6;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value6 = empty5;
			string str6 = value7;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value7 = empty6;
			string str7 = value8;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value8 = empty7;
			string str8 = value9;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value9 = empty8;
			string str9 = value10;
			string empty9 = str9;
			if (str9 == null)
			{
				empty9 = string.Empty;
			}
			value10 = empty9;
			string str10 = value11;
			string empty10 = str10;
			if (str10 == null)
			{
				empty10 = string.Empty;
			}
			value11 = empty10;
			string str11 = value12;
			string empty11 = str11;
			if (str11 == null)
			{
				empty11 = string.Empty;
			}
			value12 = empty11;
			fixed (string str12 = value1)
			{
				string* strPointers = &str12;
				char* offsetToStringData = (char*)(&str12);
				if (&str12 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str13 = value2)
				{
					string* strPointers1 = &str13;
					char* offsetToStringData1 = (char*)(&str13);
					if (&str13 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str14 = value3)
					{
						string* strPointers2 = &str14;
						char* offsetToStringData2 = (char*)(&str14);
						if (&str14 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str15 = value4)
						{
							string* strPointers3 = &str15;
							char* offsetToStringData3 = (char*)(&str15);
							if (&str15 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str16 = value5)
							{
								string* strPointers4 = &str16;
								char* offsetToStringData4 = (char*)(&str16);
								if (&str16 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str17 = value6)
								{
									string* strPointers5 = &str17;
									char* offsetToStringData5 = (char*)(&str17);
									if (&str17 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str18 = value7)
									{
										string* strPointers6 = &str18;
										char* offsetToStringData6 = (char*)(&str18);
										if (&str18 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str19 = value8)
										{
											string* strPointers7 = &str19;
											char* offsetToStringData7 = (char*)(&str19);
											if (&str19 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str20 = value9)
											{
												string* strPointers8 = &str20;
												char* offsetToStringData8 = (char*)(&str20);
												if (&str20 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer8 = offsetToStringData8;
												fixed (string str21 = value10)
												{
													string* strPointers9 = &str21;
													char* offsetToStringData9 = (char*)(&str21);
													if (&str21 != null)
													{
														offsetToStringData9 = (char*)(strPointers9 + RuntimeHelpers.OffsetToStringData);
													}
													char* chrPointer9 = offsetToStringData9;
													fixed (string str22 = value11)
													{
														string* strPointers10 = &str22;
														char* offsetToStringData10 = (char*)(&str22);
														if (&str22 != null)
														{
															offsetToStringData10 = (char*)(strPointers10 + RuntimeHelpers.OffsetToStringData);
														}
														char* chrPointer10 = offsetToStringData10;
														fixed (string str23 = value12)
														{
															string* strPointers11 = &str23;
															char* offsetToStringData11 = (char*)(&str23);
															if (&str23 != null)
															{
																offsetToStringData11 = (char*)(strPointers11 + RuntimeHelpers.OffsetToStringData);
															}
															byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 12;
															(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
															(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
															((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
															((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
															((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
															((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
															((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
															((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
															((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
															((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer9;
															((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer10;
															((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData11;
															((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).Size = (value12.Length + 1) * 2;
															bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 12, (IntPtr)length);
															return flag;
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			string str2 = value3;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value3 = empty2;
			string str3 = value4;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value4 = empty3;
			string str4 = value5;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value5 = empty4;
			string str5 = value6;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value6 = empty5;
			string str6 = value7;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value7 = empty6;
			string str7 = value8;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value8 = empty7;
			string str8 = value9;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value9 = empty8;
			string str9 = value10;
			string empty9 = str9;
			if (str9 == null)
			{
				empty9 = string.Empty;
			}
			value10 = empty9;
			string str10 = value11;
			string empty10 = str10;
			if (str10 == null)
			{
				empty10 = string.Empty;
			}
			value11 = empty10;
			string str11 = value12;
			string empty11 = str11;
			if (str11 == null)
			{
				empty11 = string.Empty;
			}
			value12 = empty11;
			string str12 = value13;
			string empty12 = str12;
			if (str12 == null)
			{
				empty12 = string.Empty;
			}
			value13 = empty12;
			fixed (string str13 = value1)
			{
				string* strPointers = &str13;
				char* offsetToStringData = (char*)(&str13);
				if (&str13 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str14 = value2)
				{
					string* strPointers1 = &str14;
					char* offsetToStringData1 = (char*)(&str14);
					if (&str14 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str15 = value3)
					{
						string* strPointers2 = &str15;
						char* offsetToStringData2 = (char*)(&str15);
						if (&str15 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str16 = value4)
						{
							string* strPointers3 = &str16;
							char* offsetToStringData3 = (char*)(&str16);
							if (&str16 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str17 = value5)
							{
								string* strPointers4 = &str17;
								char* offsetToStringData4 = (char*)(&str17);
								if (&str17 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str18 = value6)
								{
									string* strPointers5 = &str18;
									char* offsetToStringData5 = (char*)(&str18);
									if (&str18 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str19 = value7)
									{
										string* strPointers6 = &str19;
										char* offsetToStringData6 = (char*)(&str19);
										if (&str19 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str20 = value8)
										{
											string* strPointers7 = &str20;
											char* offsetToStringData7 = (char*)(&str20);
											if (&str20 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str21 = value9)
											{
												string* strPointers8 = &str21;
												char* offsetToStringData8 = (char*)(&str21);
												if (&str21 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer8 = offsetToStringData8;
												fixed (string str22 = value10)
												{
													string* strPointers9 = &str22;
													char* offsetToStringData9 = (char*)(&str22);
													if (&str22 != null)
													{
														offsetToStringData9 = (char*)(strPointers9 + RuntimeHelpers.OffsetToStringData);
													}
													char* chrPointer9 = offsetToStringData9;
													fixed (string str23 = value11)
													{
														string* strPointers10 = &str23;
														char* offsetToStringData10 = (char*)(&str23);
														if (&str23 != null)
														{
															offsetToStringData10 = (char*)(strPointers10 + RuntimeHelpers.OffsetToStringData);
														}
														char* chrPointer10 = offsetToStringData10;
														fixed (string str24 = value12)
														{
															string* strPointers11 = &str24;
															char* offsetToStringData11 = (char*)(&str24);
															if (&str24 != null)
															{
																offsetToStringData11 = (char*)(strPointers11 + RuntimeHelpers.OffsetToStringData);
															}
															char* chrPointer11 = offsetToStringData11;
															fixed (string str25 = value13)
															{
																string* strPointers12 = &str25;
																char* offsetToStringData12 = (char*)(&str25);
																if (&str25 != null)
																{
																	offsetToStringData12 = (char*)(strPointers12 + RuntimeHelpers.OffsetToStringData);
																}
																byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 13;
																(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
																(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
																((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
																((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
																((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
																((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
																((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
																((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
																((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
																((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer9;
																((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer10;
																((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer11;
																((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).Size = (value12.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData12;
																((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).Size = (value13.Length + 1) * 2;
																bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 13, (IntPtr)length);
																return flag;
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, int value1)
		{
			byte* numPointer = (byte*)sizeof(UnsafeNativeMethods.EventData);
			(*((UnsafeNativeMethods.EventData*)numPointer)).DataPointer = (ulong)(&value1);
			(*((UnsafeNativeMethods.EventData*)numPointer)).Size = 4;
			bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 1, (IntPtr)numPointer);
			return flag;
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, int value1, int value2)
		{
			byte* numPointer = (byte*)sizeof(UnsafeNativeMethods.EventData) * 2;
			(*((UnsafeNativeMethods.EventData*)numPointer)).DataPointer = (ulong)(&value1);
			(*((UnsafeNativeMethods.EventData*)numPointer)).Size = 4;
			((UnsafeNativeMethods.EventData*)numPointer + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
			((UnsafeNativeMethods.EventData*)numPointer + sizeof(UnsafeNativeMethods.EventData)).Size = 4;
			bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 2, (IntPtr)numPointer);
			return flag;
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, int value1, int value2, int value3)
		{
			byte* numPointer = (byte*)sizeof(UnsafeNativeMethods.EventData) * 3;
			(*((UnsafeNativeMethods.EventData*)numPointer)).DataPointer = (ulong)(&value1);
			(*((UnsafeNativeMethods.EventData*)numPointer)).Size = 4;
			((UnsafeNativeMethods.EventData*)numPointer + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
			((UnsafeNativeMethods.EventData*)numPointer + sizeof(UnsafeNativeMethods.EventData)).Size = 4;
			((UnsafeNativeMethods.EventData*)numPointer + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
			((UnsafeNativeMethods.EventData*)numPointer + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 4;
			bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 3, (IntPtr)numPointer);
			return flag;
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, long value1)
		{
			byte* numPointer = (byte*)sizeof(UnsafeNativeMethods.EventData);
			(*((UnsafeNativeMethods.EventData*)numPointer)).DataPointer = (ulong)(&value1);
			(*((UnsafeNativeMethods.EventData*)numPointer)).Size = 8;
			bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 1, (IntPtr)numPointer);
			return flag;
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, long value1, long value2)
		{
			byte* numPointer = (byte*)sizeof(UnsafeNativeMethods.EventData) * 2;
			(*((UnsafeNativeMethods.EventData*)numPointer)).DataPointer = (ulong)(&value1);
			(*((UnsafeNativeMethods.EventData*)numPointer)).Size = 8;
			((UnsafeNativeMethods.EventData*)numPointer + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
			((UnsafeNativeMethods.EventData*)numPointer + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
			bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 2, (IntPtr)numPointer);
			return flag;
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, long value1, long value2, long value3)
		{
			byte* numPointer = (byte*)sizeof(UnsafeNativeMethods.EventData) * 3;
			(*((UnsafeNativeMethods.EventData*)numPointer)).DataPointer = (ulong)(&value1);
			(*((UnsafeNativeMethods.EventData*)numPointer)).Size = 8;
			((UnsafeNativeMethods.EventData*)numPointer + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
			((UnsafeNativeMethods.EventData*)numPointer + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
			((UnsafeNativeMethods.EventData*)numPointer + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
			((UnsafeNativeMethods.EventData*)numPointer + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 8;
			bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 3, (IntPtr)numPointer);
			return flag;
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13, string value14, string value15)
		{
			string str = value4;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value4 = empty;
			string str1 = value5;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value5 = empty1;
			string str2 = value6;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value6 = empty2;
			string str3 = value7;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value7 = empty3;
			string str4 = value8;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value8 = empty4;
			string str5 = value9;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value9 = empty5;
			string str6 = value10;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value10 = empty6;
			string str7 = value11;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value11 = empty7;
			string str8 = value12;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value12 = empty8;
			string str9 = value13;
			string empty9 = str9;
			if (str9 == null)
			{
				empty9 = string.Empty;
			}
			value13 = empty9;
			string str10 = value14;
			string empty10 = str10;
			if (str10 == null)
			{
				empty10 = string.Empty;
			}
			value14 = empty10;
			string str11 = value15;
			string empty11 = str11;
			if (str11 == null)
			{
				empty11 = string.Empty;
			}
			value15 = empty11;
			fixed (string str12 = value4)
			{
				string* strPointers = &str12;
				char* offsetToStringData = (char*)(&str12);
				if (&str12 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str13 = value5)
				{
					string* strPointers1 = &str13;
					char* offsetToStringData1 = (char*)(&str13);
					if (&str13 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str14 = value6)
					{
						string* strPointers2 = &str14;
						char* offsetToStringData2 = (char*)(&str14);
						if (&str14 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str15 = value7)
						{
							string* strPointers3 = &str15;
							char* offsetToStringData3 = (char*)(&str15);
							if (&str15 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str16 = value8)
							{
								string* strPointers4 = &str16;
								char* offsetToStringData4 = (char*)(&str16);
								if (&str16 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str17 = value9)
								{
									string* strPointers5 = &str17;
									char* offsetToStringData5 = (char*)(&str17);
									if (&str17 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str18 = value10)
									{
										string* strPointers6 = &str18;
										char* offsetToStringData6 = (char*)(&str18);
										if (&str18 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str19 = value11)
										{
											string* strPointers7 = &str19;
											char* offsetToStringData7 = (char*)(&str19);
											if (&str19 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str20 = value12)
											{
												string* strPointers8 = &str20;
												char* offsetToStringData8 = (char*)(&str20);
												if (&str20 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer8 = offsetToStringData8;
												fixed (string str21 = value13)
												{
													string* strPointers9 = &str21;
													char* offsetToStringData9 = (char*)(&str21);
													if (&str21 != null)
													{
														offsetToStringData9 = (char*)(strPointers9 + RuntimeHelpers.OffsetToStringData);
													}
													char* chrPointer9 = offsetToStringData9;
													fixed (string str22 = value14)
													{
														string* strPointers10 = &str22;
														char* offsetToStringData10 = (char*)(&str22);
														if (&str22 != null)
														{
															offsetToStringData10 = (char*)(strPointers10 + RuntimeHelpers.OffsetToStringData);
														}
														char* chrPointer10 = offsetToStringData10;
														fixed (string str23 = value15)
														{
															string* strPointers11 = &str23;
															char* offsetToStringData11 = (char*)(&str23);
															if (&str23 != null)
															{
																offsetToStringData11 = (char*)(strPointers11 + RuntimeHelpers.OffsetToStringData);
															}
															byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 15;
															(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)(&value1);
															(*((UnsafeNativeMethods.EventData*)length)).Size = sizeof(Guid);
															((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
															((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
															((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
															((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 8;
															((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
															((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
															((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
															((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
															((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
															((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
															((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
															((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
															((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
															((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).Size = (value12.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer9;
															((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).Size = (value13.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 13 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer10;
															((UnsafeNativeMethods.EventData*)length + 13 * sizeof(UnsafeNativeMethods.EventData)).Size = (value14.Length + 1) * 2;
															((UnsafeNativeMethods.EventData*)length + 14 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData11;
															((UnsafeNativeMethods.EventData*)length + 14 * sizeof(UnsafeNativeMethods.EventData)).Size = (value15.Length + 1) * 2;
															bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 15, (IntPtr)length);
															return flag;
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, bool value13, string value14, string value15, string value16, string value17)
		{
			string str = value4;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value4 = empty;
			string str1 = value5;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value5 = empty1;
			string str2 = value6;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value6 = empty2;
			string str3 = value7;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value7 = empty3;
			string str4 = value8;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value8 = empty4;
			string str5 = value9;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value9 = empty5;
			string str6 = value10;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value10 = empty6;
			string str7 = value11;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value11 = empty7;
			string str8 = value12;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value12 = empty8;
			string str9 = value14;
			string empty9 = str9;
			if (str9 == null)
			{
				empty9 = string.Empty;
			}
			value14 = empty9;
			string str10 = value15;
			string empty10 = str10;
			if (str10 == null)
			{
				empty10 = string.Empty;
			}
			value15 = empty10;
			string str11 = value16;
			string empty11 = str11;
			if (str11 == null)
			{
				empty11 = string.Empty;
			}
			value16 = empty11;
			string str12 = value17;
			string empty12 = str12;
			if (str12 == null)
			{
				empty12 = string.Empty;
			}
			value17 = empty12;
			fixed (string str13 = value4)
			{
				string* strPointers = &str13;
				char* offsetToStringData = (char*)(&str13);
				if (&str13 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str14 = value5)
				{
					string* strPointers1 = &str14;
					char* offsetToStringData1 = (char*)(&str14);
					if (&str14 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str15 = value6)
					{
						string* strPointers2 = &str15;
						char* offsetToStringData2 = (char*)(&str15);
						if (&str15 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str16 = value7)
						{
							string* strPointers3 = &str16;
							char* offsetToStringData3 = (char*)(&str16);
							if (&str16 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str17 = value8)
							{
								string* strPointers4 = &str17;
								char* offsetToStringData4 = (char*)(&str17);
								if (&str17 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str18 = value9)
								{
									string* strPointers5 = &str18;
									char* offsetToStringData5 = (char*)(&str18);
									if (&str18 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str19 = value10)
									{
										string* strPointers6 = &str19;
										char* offsetToStringData6 = (char*)(&str19);
										if (&str19 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str20 = value11)
										{
											string* strPointers7 = &str20;
											char* offsetToStringData7 = (char*)(&str20);
											if (&str20 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str21 = value12)
											{
												string* strPointers8 = &str21;
												char* offsetToStringData8 = (char*)(&str21);
												if (&str21 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer8 = offsetToStringData8;
												fixed (string str22 = value14)
												{
													string* strPointers9 = &str22;
													char* offsetToStringData9 = (char*)(&str22);
													if (&str22 != null)
													{
														offsetToStringData9 = (char*)(strPointers9 + RuntimeHelpers.OffsetToStringData);
													}
													char* chrPointer9 = offsetToStringData9;
													fixed (string str23 = value15)
													{
														string* strPointers10 = &str23;
														char* offsetToStringData10 = (char*)(&str23);
														if (&str23 != null)
														{
															offsetToStringData10 = (char*)(strPointers10 + RuntimeHelpers.OffsetToStringData);
														}
														char* chrPointer10 = offsetToStringData10;
														fixed (string str24 = value16)
														{
															string* strPointers11 = &str24;
															char* offsetToStringData11 = (char*)(&str24);
															if (&str24 != null)
															{
																offsetToStringData11 = (char*)(strPointers11 + RuntimeHelpers.OffsetToStringData);
															}
															char* chrPointer11 = offsetToStringData11;
															fixed (string str25 = value17)
															{
																string* strPointers12 = &str25;
																char* offsetToStringData12 = (char*)(&str25);
																if (&str25 != null)
																{
																	offsetToStringData12 = (char*)(strPointers12 + RuntimeHelpers.OffsetToStringData);
																}
																byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 17;
																(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)(&value1);
																(*((UnsafeNativeMethods.EventData*)length)).Size = sizeof(Guid);
																((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
																((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
																((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
																((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 8;
																((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
																((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
																((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
																((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
																((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
																((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
																((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
																((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
																((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
																((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).Size = (value12.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value13);
																((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).Size = 1;
																((UnsafeNativeMethods.EventData*)length + 13 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer9;
																((UnsafeNativeMethods.EventData*)length + 13 * sizeof(UnsafeNativeMethods.EventData)).Size = (value14.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 14 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer10;
																((UnsafeNativeMethods.EventData*)length + 14 * sizeof(UnsafeNativeMethods.EventData)).Size = (value15.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 15 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer11;
																((UnsafeNativeMethods.EventData*)length + 15 * sizeof(UnsafeNativeMethods.EventData)).Size = (value16.Length + 1) * 2;
																((UnsafeNativeMethods.EventData*)length + 16 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData12;
																((UnsafeNativeMethods.EventData*)length + 16 * sizeof(UnsafeNativeMethods.EventData)).Size = (value17.Length + 1) * 2;
																bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 17, (IntPtr)length);
																return flag;
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9)
		{
			string str = value4;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value4 = empty;
			string str1 = value5;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value5 = empty1;
			string str2 = value6;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value6 = empty2;
			string str3 = value7;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value7 = empty3;
			string str4 = value8;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value8 = empty4;
			string str5 = value9;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value9 = empty5;
			fixed (string str6 = value4)
			{
				string* strPointers = &str6;
				char* offsetToStringData = (char*)(&str6);
				if (&str6 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str7 = value5)
				{
					string* strPointers1 = &str7;
					char* offsetToStringData1 = (char*)(&str7);
					if (&str7 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str8 = value6)
					{
						string* strPointers2 = &str8;
						char* offsetToStringData2 = (char*)(&str8);
						if (&str8 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str9 = value7)
						{
							string* strPointers3 = &str9;
							char* offsetToStringData3 = (char*)(&str9);
							if (&str9 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str10 = value8)
							{
								string* strPointers4 = &str10;
								char* offsetToStringData4 = (char*)(&str10);
								if (&str10 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str11 = value9)
								{
									string* strPointers5 = &str11;
									char* offsetToStringData5 = (char*)(&str11);
									if (&str11 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 9;
									(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)(&value1);
									(*((UnsafeNativeMethods.EventData*)length)).Size = sizeof(Guid);
									((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
									((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
									((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
									((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 8;
									((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
									((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
									((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
									((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
									((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
									((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
									((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData5;
									((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
									bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 9, (IntPtr)length);
									return flag;
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11)
		{
			string str = value4;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value4 = empty;
			string str1 = value5;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value5 = empty1;
			string str2 = value6;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value6 = empty2;
			string str3 = value7;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value7 = empty3;
			string str4 = value8;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value8 = empty4;
			string str5 = value9;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value9 = empty5;
			string str6 = value10;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value10 = empty6;
			string str7 = value11;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value11 = empty7;
			fixed (string str8 = value4)
			{
				string* strPointers = &str8;
				char* offsetToStringData = (char*)(&str8);
				if (&str8 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str9 = value5)
				{
					string* strPointers1 = &str9;
					char* offsetToStringData1 = (char*)(&str9);
					if (&str9 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str10 = value6)
					{
						string* strPointers2 = &str10;
						char* offsetToStringData2 = (char*)(&str10);
						if (&str10 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str11 = value7)
						{
							string* strPointers3 = &str11;
							char* offsetToStringData3 = (char*)(&str11);
							if (&str11 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str12 = value8)
							{
								string* strPointers4 = &str12;
								char* offsetToStringData4 = (char*)(&str12);
								if (&str12 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str13 = value9)
								{
									string* strPointers5 = &str13;
									char* offsetToStringData5 = (char*)(&str13);
									if (&str13 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str14 = value10)
									{
										string* strPointers6 = &str14;
										char* offsetToStringData6 = (char*)(&str14);
										if (&str14 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str15 = value11)
										{
											string* strPointers7 = &str15;
											char* offsetToStringData7 = (char*)(&str15);
											if (&str15 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 11;
											(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)(&value1);
											(*((UnsafeNativeMethods.EventData*)length)).Size = sizeof(Guid);
											((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
											((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
											((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
											((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 8;
											((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
											((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
											((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
											((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
											((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
											((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
											((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
											((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
											((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData7;
											((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
											bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 11, (IntPtr)length);
											return flag;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
		{
			string str = value4;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value4 = empty;
			string str1 = value5;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value5 = empty1;
			string str2 = value6;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value6 = empty2;
			string str3 = value7;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value7 = empty3;
			string str4 = value8;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value8 = empty4;
			string str5 = value9;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value9 = empty5;
			string str6 = value10;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value10 = empty6;
			string str7 = value11;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value11 = empty7;
			string str8 = value12;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value12 = empty8;
			string str9 = value13;
			string empty9 = str9;
			if (str9 == null)
			{
				empty9 = string.Empty;
			}
			value13 = empty9;
			fixed (string str10 = value4)
			{
				string* strPointers = &str10;
				char* offsetToStringData = (char*)(&str10);
				if (&str10 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str11 = value5)
				{
					string* strPointers1 = &str11;
					char* offsetToStringData1 = (char*)(&str11);
					if (&str11 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str12 = value6)
					{
						string* strPointers2 = &str12;
						char* offsetToStringData2 = (char*)(&str12);
						if (&str12 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str13 = value7)
						{
							string* strPointers3 = &str13;
							char* offsetToStringData3 = (char*)(&str13);
							if (&str13 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str14 = value8)
							{
								string* strPointers4 = &str14;
								char* offsetToStringData4 = (char*)(&str14);
								if (&str14 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str15 = value9)
								{
									string* strPointers5 = &str15;
									char* offsetToStringData5 = (char*)(&str15);
									if (&str15 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str16 = value10)
									{
										string* strPointers6 = &str16;
										char* offsetToStringData6 = (char*)(&str16);
										if (&str16 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str17 = value11)
										{
											string* strPointers7 = &str17;
											char* offsetToStringData7 = (char*)(&str17);
											if (&str17 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str18 = value12)
											{
												string* strPointers8 = &str18;
												char* offsetToStringData8 = (char*)(&str18);
												if (&str18 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer8 = offsetToStringData8;
												fixed (string str19 = value13)
												{
													string* strPointers9 = &str19;
													char* offsetToStringData9 = (char*)(&str19);
													if (&str19 != null)
													{
														offsetToStringData9 = (char*)(strPointers9 + RuntimeHelpers.OffsetToStringData);
													}
													byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 13;
													(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)(&value1);
													(*((UnsafeNativeMethods.EventData*)length)).Size = sizeof(Guid);
													((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
													((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
													((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
													((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 8;
													((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
													((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
													((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
													((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
													((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
													((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
													((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
													((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
													((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
													((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).Size = (value12.Length + 1) * 2;
													((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData9;
													((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).Size = (value13.Length + 1) * 2;
													bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 13, (IntPtr)length);
													return flag;
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13, string value14)
		{
			string str = value4;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value4 = empty;
			string str1 = value5;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value5 = empty1;
			string str2 = value6;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value6 = empty2;
			string str3 = value7;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value7 = empty3;
			string str4 = value8;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value8 = empty4;
			string str5 = value9;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value9 = empty5;
			string str6 = value10;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value10 = empty6;
			string str7 = value11;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value11 = empty7;
			string str8 = value12;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value12 = empty8;
			string str9 = value13;
			string empty9 = str9;
			if (str9 == null)
			{
				empty9 = string.Empty;
			}
			value13 = empty9;
			string str10 = value14;
			string empty10 = str10;
			if (str10 == null)
			{
				empty10 = string.Empty;
			}
			value14 = empty10;
			fixed (string str11 = value4)
			{
				string* strPointers = &str11;
				char* offsetToStringData = (char*)(&str11);
				if (&str11 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str12 = value5)
				{
					string* strPointers1 = &str12;
					char* offsetToStringData1 = (char*)(&str12);
					if (&str12 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str13 = value6)
					{
						string* strPointers2 = &str13;
						char* offsetToStringData2 = (char*)(&str13);
						if (&str13 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str14 = value7)
						{
							string* strPointers3 = &str14;
							char* offsetToStringData3 = (char*)(&str14);
							if (&str14 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str15 = value8)
							{
								string* strPointers4 = &str15;
								char* offsetToStringData4 = (char*)(&str15);
								if (&str15 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str16 = value9)
								{
									string* strPointers5 = &str16;
									char* offsetToStringData5 = (char*)(&str16);
									if (&str16 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str17 = value10)
									{
										string* strPointers6 = &str17;
										char* offsetToStringData6 = (char*)(&str17);
										if (&str17 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str18 = value11)
										{
											string* strPointers7 = &str18;
											char* offsetToStringData7 = (char*)(&str18);
											if (&str18 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str19 = value12)
											{
												string* strPointers8 = &str19;
												char* offsetToStringData8 = (char*)(&str19);
												if (&str19 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer8 = offsetToStringData8;
												fixed (string str20 = value13)
												{
													string* strPointers9 = &str20;
													char* offsetToStringData9 = (char*)(&str20);
													if (&str20 != null)
													{
														offsetToStringData9 = (char*)(strPointers9 + RuntimeHelpers.OffsetToStringData);
													}
													char* chrPointer9 = offsetToStringData9;
													fixed (string str21 = value14)
													{
														string* strPointers10 = &str21;
														char* offsetToStringData10 = (char*)(&str21);
														if (&str21 != null)
														{
															offsetToStringData10 = (char*)(strPointers10 + RuntimeHelpers.OffsetToStringData);
														}
														byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 14;
														(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)(&value1);
														(*((UnsafeNativeMethods.EventData*)length)).Size = sizeof(Guid);
														((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
														((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
														((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
														((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 8;
														((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
														((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
														((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = (value5.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
														((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
														((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
														((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
														((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
														((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
														((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
														((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).Size = (value12.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer9;
														((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).Size = (value13.Length + 1) * 2;
														((UnsafeNativeMethods.EventData*)length + 13 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData10;
														((UnsafeNativeMethods.EventData*)length + 13 * sizeof(UnsafeNativeMethods.EventData)).Size = (value14.Length + 1) * 2;
														bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 14, (IntPtr)length);
														return flag;
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid value1, long value2, long value3, string value4, Guid value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
		{
			string str = value4;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value4 = empty;
			string str1 = value6;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value6 = empty1;
			string str2 = value7;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value7 = empty2;
			string str3 = value8;
			string empty3 = str3;
			if (str3 == null)
			{
				empty3 = string.Empty;
			}
			value8 = empty3;
			string str4 = value9;
			string empty4 = str4;
			if (str4 == null)
			{
				empty4 = string.Empty;
			}
			value9 = empty4;
			string str5 = value10;
			string empty5 = str5;
			if (str5 == null)
			{
				empty5 = string.Empty;
			}
			value10 = empty5;
			string str6 = value11;
			string empty6 = str6;
			if (str6 == null)
			{
				empty6 = string.Empty;
			}
			value11 = empty6;
			string str7 = value12;
			string empty7 = str7;
			if (str7 == null)
			{
				empty7 = string.Empty;
			}
			value12 = empty7;
			string str8 = value13;
			string empty8 = str8;
			if (str8 == null)
			{
				empty8 = string.Empty;
			}
			value13 = empty8;
			fixed (string str9 = value4)
			{
				string* strPointers = &str9;
				char* offsetToStringData = (char*)(&str9);
				if (&str9 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str10 = value6)
				{
					string* strPointers1 = &str10;
					char* offsetToStringData1 = (char*)(&str10);
					if (&str10 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str11 = value7)
					{
						string* strPointers2 = &str11;
						char* offsetToStringData2 = (char*)(&str11);
						if (&str11 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						char* chrPointer2 = offsetToStringData2;
						fixed (string str12 = value8)
						{
							string* strPointers3 = &str12;
							char* offsetToStringData3 = (char*)(&str12);
							if (&str12 != null)
							{
								offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
							}
							char* chrPointer3 = offsetToStringData3;
							fixed (string str13 = value9)
							{
								string* strPointers4 = &str13;
								char* offsetToStringData4 = (char*)(&str13);
								if (&str13 != null)
								{
									offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer4 = offsetToStringData4;
								fixed (string str14 = value10)
								{
									string* strPointers5 = &str14;
									char* offsetToStringData5 = (char*)(&str14);
									if (&str14 != null)
									{
										offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer5 = offsetToStringData5;
									fixed (string str15 = value11)
									{
										string* strPointers6 = &str15;
										char* offsetToStringData6 = (char*)(&str15);
										if (&str15 != null)
										{
											offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer6 = offsetToStringData6;
										fixed (string str16 = value12)
										{
											string* strPointers7 = &str16;
											char* offsetToStringData7 = (char*)(&str16);
											if (&str16 != null)
											{
												offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer7 = offsetToStringData7;
											fixed (string str17 = value13)
											{
												string* strPointers8 = &str17;
												char* offsetToStringData8 = (char*)(&str17);
												if (&str17 != null)
												{
													offsetToStringData8 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
												}
												byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 13;
												(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)(&value1);
												(*((UnsafeNativeMethods.EventData*)length)).Size = sizeof(Guid);
												((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
												((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
												((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value3);
												((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = 8;
												((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
												((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value5);
												((UnsafeNativeMethods.EventData*)length + 4 * sizeof(UnsafeNativeMethods.EventData)).Size = sizeof(Guid);
												((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
												((UnsafeNativeMethods.EventData*)length + 5 * sizeof(UnsafeNativeMethods.EventData)).Size = (value6.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
												((UnsafeNativeMethods.EventData*)length + 6 * sizeof(UnsafeNativeMethods.EventData)).Size = (value7.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
												((UnsafeNativeMethods.EventData*)length + 7 * sizeof(UnsafeNativeMethods.EventData)).Size = (value8.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
												((UnsafeNativeMethods.EventData*)length + 8 * sizeof(UnsafeNativeMethods.EventData)).Size = (value9.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
												((UnsafeNativeMethods.EventData*)length + 9 * sizeof(UnsafeNativeMethods.EventData)).Size = (value10.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
												((UnsafeNativeMethods.EventData*)length + 10 * sizeof(UnsafeNativeMethods.EventData)).Size = (value11.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
												((UnsafeNativeMethods.EventData*)length + 11 * sizeof(UnsafeNativeMethods.EventData)).Size = (value12.Length + 1) * 2;
												((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData8;
												((UnsafeNativeMethods.EventData*)length + 12 * sizeof(UnsafeNativeMethods.EventData)).Size = (value13.Length + 1) * 2;
												bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 13, (IntPtr)length);
												return flag;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string value1, long value2, string value3, string value4)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value3;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value3 = empty1;
			string str2 = value4;
			string empty2 = str2;
			if (str2 == null)
			{
				empty2 = string.Empty;
			}
			value4 = empty2;
			fixed (string str3 = value1)
			{
				string* strPointers = &str3;
				char* offsetToStringData = (char*)(&str3);
				if (&str3 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str4 = value3)
				{
					string* strPointers1 = &str4;
					char* offsetToStringData1 = (char*)(&str4);
					if (&str4 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					char* chrPointer1 = offsetToStringData1;
					fixed (string str5 = value4)
					{
						string* strPointers2 = &str5;
						char* offsetToStringData2 = (char*)(&str5);
						if (&str5 != null)
						{
							offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
						}
						byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 4;
						(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
						(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
						((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)(&value2);
						((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = 8;
						((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
						((UnsafeNativeMethods.EventData*)length + 2 * sizeof(UnsafeNativeMethods.EventData)).Size = (value3.Length + 1) * 2;
						((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData2;
						((UnsafeNativeMethods.EventData*)length + 3 * sizeof(UnsafeNativeMethods.EventData)).Size = (value4.Length + 1) * 2;
						bool flag = base.WriteEvent(ref eventDescriptor, eventTraceActivity, 4, (IntPtr)length);
						return flag;
					}
				}
			}
		}

		[SecurityCritical]
		internal unsafe bool WriteTransferEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid relatedActivityId, string value1, string value2)
		{
			string str = value1;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			value1 = empty;
			string str1 = value2;
			string empty1 = str1;
			if (str1 == null)
			{
				empty1 = string.Empty;
			}
			value2 = empty1;
			fixed (string str2 = value1)
			{
				string* strPointers = &str2;
				char* offsetToStringData = (char*)(&str2);
				if (&str2 != null)
				{
					offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
				}
				char* chrPointer = offsetToStringData;
				fixed (string str3 = value2)
				{
					string* strPointers1 = &str3;
					char* offsetToStringData1 = (char*)(&str3);
					if (&str3 != null)
					{
						offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
					}
					byte* length = (byte*)sizeof(UnsafeNativeMethods.EventData) * 2;
					(*((UnsafeNativeMethods.EventData*)length)).DataPointer = (ulong)chrPointer;
					(*((UnsafeNativeMethods.EventData*)length)).Size = (value1.Length + 1) * 2;
					((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)offsetToStringData1;
					((UnsafeNativeMethods.EventData*)length + sizeof(UnsafeNativeMethods.EventData)).Size = (value2.Length + 1) * 2;
					bool flag = base.WriteTransferEvent(ref eventDescriptor, eventTraceActivity, relatedActivityId, 2, (IntPtr)length);
					return flag;
				}
			}
		}
	}
}