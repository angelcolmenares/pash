using System;
using System.Globalization;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.Interop;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime.Diagnostics
{

	[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
	internal abstract class DiagnosticsEventProvider : IDisposable
	{
		[SecurityCritical]
		private UnsafeNativeMethods.EtwEnableCallback etwCallback;

		private long traceRegistrationHandle;

		private byte currentTraceLevel;

		private long anyKeywordMask;

		private long allKeywordMask;

		private bool isProviderEnabled;

		private Guid providerId;

		private int isDisposed;

		[ThreadStatic]
		private static DiagnosticsEventProvider.WriteEventErrorCode errorCode;

		private const int basicTypeAllocationBufferSize = 16;

		private const int etwMaxNumberArguments = 32;

		private const int etwAPIMaxStringCount = 8;

		private const int maxEventDataDescriptors = 128;

		private const int traceEventMaximumSize = 0xffca;

		private const int traceEventMaximumStringSize = 0x7fd4;

		private const int WindowsVistaMajorNumber = 6;

		[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
		[SecurityCritical]
		protected DiagnosticsEventProvider(Guid providerGuid)
		{
			this.providerId = providerGuid;
			this.EtwRegister();
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public virtual void Close()
		{
			this.Dispose();
		}

		[SecurityCritical]
		private void Deregister()
		{
			if (this.traceRegistrationHandle != (long)0)
			{
				UnsafeNativeMethods.EventUnregister(this.traceRegistrationHandle);
				this.traceRegistrationHandle = (long)0;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[SecuritySafeCritical]
		protected virtual void Dispose(bool disposing)
		{
			if (this.isDisposed != 1 && Interlocked.Exchange(ref this.isDisposed, 1) == 0)
			{
				this.isProviderEnabled = false;
				this.Deregister();
			}
		}

		[SecurityCritical]
		private static unsafe string EncodeObject(ref object data, UnsafeNativeMethods.EventData* dataDescriptor, byte* dataBuffer)
		{
			(*(dataDescriptor)).Reserved = 0;
			string str = data as string;
			if (str == null)
			{
				if (data as IntPtr == 0)
				{
					if (data as int == 0)
					{
						if (data as long == 0)
						{
							if (data as uint == 0)
							{
								if (data as ulong == 0)
								{
									if (data as char == 0)
									{
										if (data as byte == 0)
										{
											if (data as short == 0)
											{
												if (data as sbyte == 0)
												{
													if (data as ushort == 0)
													{
														if (data as float == 0)
														{
															if (data as double == 0)
															{
																if (!data as bool)
																{
																	if (data as Guid == null)
																	{
																		if (data as decimal == null)
																		{
																			if (!data as bool)
																			{
																				str = data.ToString();
																				(*(dataDescriptor)).Size = (str.Length + 1) * 2;
																				return str;
																			}
																			else
																			{
																				(*(dataDescriptor)).Size = 1;
																				*((bool*)dataBuffer) = (bool)data;
																				(*(dataDescriptor)).DataPointer = (ulong)((bool*)dataBuffer);
																			}
																		}
																		else
																		{
																			(*(dataDescriptor)).Size = 16;
																			*((decimal*)dataBuffer) = (decimal)data;
																			(*(dataDescriptor)).DataPointer = (ulong)((decimal*)dataBuffer);
																		}
																	}
																	else
																	{
																		(*(dataDescriptor)).Size = sizeof(Guid);
																		*((Guid*)dataBuffer) = (Guid)data;
																		(*(dataDescriptor)).DataPointer = (ulong)((Guid*)dataBuffer);
																	}
																}
																else
																{
																	(*(dataDescriptor)).Size = 1;
																	*((bool*)dataBuffer) = (bool)data;
																	(*(dataDescriptor)).DataPointer = (ulong)((bool*)dataBuffer);
																}
															}
															else
															{
																(*(dataDescriptor)).Size = 8;
																*((double*)dataBuffer) = (double)data;
																(*(dataDescriptor)).DataPointer = (ulong)((double*)dataBuffer);
															}
														}
														else
														{
															(*(dataDescriptor)).Size = 4;
															*((float*)dataBuffer) = (float)data;
															(*(dataDescriptor)).DataPointer = (ulong)((float*)dataBuffer);
														}
													}
													else
													{
														(*(dataDescriptor)).Size = 2;
														*((ushort*)dataBuffer) = (ushort)data;
														(*(dataDescriptor)).DataPointer = (ulong)((ushort*)dataBuffer);
													}
												}
												else
												{
													(*(dataDescriptor)).Size = 1;
													*((sbyte*)dataBuffer) = (sbyte)data;
													(*(dataDescriptor)).DataPointer = (ulong)((sbyte*)dataBuffer);
												}
											}
											else
											{
												(*(dataDescriptor)).Size = 2;
												*((short*)dataBuffer) = (short)data;
												(*(dataDescriptor)).DataPointer = (ulong)((short*)dataBuffer);
											}
										}
										else
										{
											(*(dataDescriptor)).Size = 1;
											*(dataBuffer) = (byte)data;
											(*(dataDescriptor)).DataPointer = (ulong)dataBuffer;
										}
									}
									else
									{
										(*(dataDescriptor)).Size = 2;
										*((char*)dataBuffer) = (char)data;
										(*(dataDescriptor)).DataPointer = (ulong)((char*)dataBuffer);
									}
								}
								else
								{
									(*(dataDescriptor)).Size = 8;
									*((ulong*)dataBuffer) = (ulong)data;
									(*(dataDescriptor)).DataPointer = (ulong)((ulong*)dataBuffer);
								}
							}
							else
							{
								(*(dataDescriptor)).Size = 4;
								*((uint*)dataBuffer) = (uint)data;
								(*(dataDescriptor)).DataPointer = (ulong)((uint*)dataBuffer);
							}
						}
						else
						{
							(*(dataDescriptor)).Size = 8;
							*((long*)dataBuffer) = (long)data;
							(*(dataDescriptor)).DataPointer = (ulong)((long*)dataBuffer);
						}
					}
					else
					{
						(*(dataDescriptor)).Size = 4;
						*((int*)dataBuffer) = (int)data;
						(*(dataDescriptor)).DataPointer = (ulong)((int*)dataBuffer);
					}
				}
				else
				{
					(*(dataDescriptor)).Size = sizeof(IntPtr);
					*((IntPtr*)dataBuffer) = (IntPtr)data;
					(*(dataDescriptor)).DataPointer = (ulong)((IntPtr*)dataBuffer);
				}
				return null;
			}
			else
			{
				(*(dataDescriptor)).Size = (str.Length + 1) * 2;
				return str;
			}
		}

		[SecurityCritical]
		private unsafe void EtwEnableCallBack(ref Guid sourceId, int isEnabled, byte setLevel, long anyKeyword, long allKeyword, void* filterData, void* callbackContext)
		{
			this.isProviderEnabled = isEnabled != 0;
			this.currentTraceLevel = setLevel;
			this.anyKeywordMask = anyKeyword;
			this.allKeywordMask = allKeyword;
			this.OnControllerCommand();
		}

		[SecurityCritical]
		private void EtwRegister()
		{
			unsafe
			{
				this.etwCallback = new UnsafeNativeMethods.EtwEnableCallback(this.EtwEnableCallBack);
				uint num = UnsafeNativeMethods.EventRegister(ref this.providerId, this.etwCallback, 0, out this.traceRegistrationHandle);
				if (num == 0)
				{
					return;
				}
				else
				{
					throw new InvalidOperationException(InternalSR.EtwRegistrationFailed(num.ToString("x", CultureInfo.CurrentCulture)));
				}
			}
		}

		~DiagnosticsEventProvider()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				//this.Finalize();
			}
		}

		public static DiagnosticsEventProvider.WriteEventErrorCode GetLastWriteEventError()
		{
			return DiagnosticsEventProvider.errorCode;
		}

		public bool IsEnabled()
		{
			return this.isProviderEnabled;
		}

		public bool IsEnabled(byte level, long keywords)
		{
			if (!this.isProviderEnabled || level > this.currentTraceLevel && this.currentTraceLevel != 0 || keywords != (long)0 && ((keywords & this.anyKeywordMask) == (long)0 || (keywords & this.allKeywordMask) != this.allKeywordMask))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		protected abstract void OnControllerCommand();

		[SecurityCritical]
		public static void SetActivityId(ref Guid id)
		{
			UnsafeNativeMethods.EventActivityIdControl(2, out id);
		}

		private static void SetLastError(int error)
		{
			int num = error;
			if (num == 8)
			{
				DiagnosticsEventProvider.errorCode = DiagnosticsEventProvider.WriteEventErrorCode.NoFreeBuffers;
				return;
			}
			else
			{
				if (num == 234 || num == 0x216)
				{
					DiagnosticsEventProvider.errorCode = DiagnosticsEventProvider.WriteEventErrorCode.EventTooBig;
					return;
				}
				else
				{
					return;
				}
			}
		}

		[SecurityCritical]
		public unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, object[] eventPayload)
		{
			uint num = 0;
			if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				int length = 0;
				if (eventTraceActivity != null)
				{
					DiagnosticsEventProvider.SetActivityId(ref eventTraceActivity.ActivityId);
				}
				if (eventPayload == null || (int)eventPayload.Length == 0 || (int)eventPayload.Length == 1)
				{
					string str = null;
					byte numPointer = (byte)16;
					eventDatum.Size = 0;
					if (eventPayload != null && (int)eventPayload.Length != 0)
					{
						str = DiagnosticsEventProvider.EncodeObject(ref eventPayload[0], ref eventDatum, numPointer);
						length = 1;
					}
					if (eventDatum.Size <= 0xffca)
					{
						if (str == null)
						{
							if (length != 0)
							{
								num = 0; //TODO: num = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, length, ref eventDatum);
							}
							else
							{
								num = 0; //TODO: num = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, 0, 0);
							}
						}
						else
						{
							fixed (string str1 = str)
							{
								string* strPointers = &str1;
								char* offsetToStringData = (char*)(&str1);
								if (&str1 != null)
								{
									offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
								}
								eventDatum.DataPointer = (ulong)offsetToStringData;
								num = 0; //TODO: num = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, length, ref eventDatum);
							}
						}
					}
					else
					{
						DiagnosticsEventProvider.errorCode = DiagnosticsEventProvider.WriteEventErrorCode.EventTooBig;
						return false;
					}
				}
				else
				{
					length = (int)eventPayload.Length;
					if (length <= 32)
					{
						uint size = 0;
						int num1 = 0;
						int[] numArray = new int[8];
						string[] strArrays = new string[8];
						UnsafeNativeMethods.EventData* eventDataPointer = (UnsafeNativeMethods.EventData*)length * sizeof(UnsafeNativeMethods.EventData);
						UnsafeNativeMethods.EventData* eventDataPointer1 = eventDataPointer;
						byte* numPointer1 = (byte*)16 * length;
						byte* numPointer2 = numPointer1;
						for (int i = 0; i < (int)eventPayload.Length; i++)
						{
							if (eventPayload[i] != null)
							{
								string str2 = DiagnosticsEventProvider.EncodeObject(ref eventPayload[i], eventDataPointer1, numPointer2);
								numPointer2 = numPointer2 + 16;
								size = size + (*(eventDataPointer1)).Size;
								eventDataPointer1 = (UnsafeNativeMethods.EventData*)(eventDataPointer1 + sizeof(UnsafeNativeMethods.EventData));
								if (str2 != null)
								{
									if (num1 >= 8)
									{
										throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload", InternalSR.EtwAPIMaxStringCountExceeded(8)));
									}
									else
									{
										strArrays[num1] = str2;
										numArray[num1] = i;
										num1++;
									}
								}
							}
						}
						if (size <= 0xffca)
						{
							fixed (string str3 = strArrays[0])
							{
								string* strPointers1 = &str3;
								char* chrPointer = (char*)(&str3);
								if (&str3 != null)
								{
									chrPointer = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer1 = chrPointer;
								fixed (string str4 = strArrays[1])
								{
									string* strPointers2 = &str4;
									char* offsetToStringData1 = (char*)(&str4);
									if (&str4 != null)
									{
										offsetToStringData1 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer2 = offsetToStringData1;
									fixed (string str5 = strArrays[2])
									{
										string* strPointers3 = &str5;
										char* offsetToStringData2 = (char*)(&str5);
										if (&str5 != null)
										{
											offsetToStringData2 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer3 = offsetToStringData2;
										fixed (string str6 = strArrays[3])
										{
											string* strPointers4 = &str6;
											char* offsetToStringData3 = (char*)(&str6);
											if (&str6 != null)
											{
												offsetToStringData3 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer4 = offsetToStringData3;
											fixed (string str7 = strArrays[4])
											{
												string* strPointers5 = &str7;
												char* offsetToStringData4 = (char*)(&str7);
												if (&str7 != null)
												{
													offsetToStringData4 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer5 = offsetToStringData4;
												fixed (string str8 = strArrays[5])
												{
													string* strPointers6 = &str8;
													char* offsetToStringData5 = (char*)(&str8);
													if (&str8 != null)
													{
														offsetToStringData5 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
													}
													char* chrPointer6 = offsetToStringData5;
													fixed (string str9 = strArrays[6])
													{
														string* strPointers7 = &str9;
														char* offsetToStringData6 = (char*)(&str9);
														if (&str9 != null)
														{
															offsetToStringData6 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
														}
														char* chrPointer7 = offsetToStringData6;
														fixed (string str10 = strArrays[7])
														{
															string* strPointers8 = &str10;
															char* offsetToStringData7 = (char*)(&str10);
															if (&str10 != null)
															{
																offsetToStringData7 = (char*)(strPointers8 + RuntimeHelpers.OffsetToStringData);
															}
															char* chrPointer8 = offsetToStringData7;
															eventDataPointer1 = eventDataPointer;
															if (strArrays[0] != null)
															{
																(eventDataPointer1 + numArray[0] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
															}
															if (strArrays[1] != null)
															{
																(eventDataPointer1 + numArray[1] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
															}
															if (strArrays[2] != null)
															{
																(eventDataPointer1 + numArray[2] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
															}
															if (strArrays[3] != null)
															{
																(eventDataPointer1 + numArray[3] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
															}
															if (strArrays[4] != null)
															{
																(eventDataPointer1 + numArray[4] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
															}
															if (strArrays[5] != null)
															{
																(eventDataPointer1 + numArray[5] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
															}
															if (strArrays[6] != null)
															{
																(eventDataPointer1 + numArray[6] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
															}
															if (strArrays[7] != null)
															{
																(eventDataPointer1 + numArray[7] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer8;
															}
															num = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, length, eventDataPointer);
														}
													}
												}
											}
										}
									}
								}
							}
						}
						else
						{
							DiagnosticsEventProvider.errorCode = DiagnosticsEventProvider.WriteEventErrorCode.EventTooBig;
							return false;
						}
					}
					else
					{
						throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload", InternalSR.EtwMaxNumberArgumentsExceeded(32)));
					}
				}
			}
			if (num == 0)
			{
				return true;
			}
			else
			{
				DiagnosticsEventProvider.SetLastError(num);
				return false;
			}
		}

		[SecurityCritical]
		public unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, string data)
		{
			uint num = 0;
			string str = data;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			data = empty;
			if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				if (data.Length <= 0x7fd4)
				{
					if (eventTraceActivity != null)
					{
						DiagnosticsEventProvider.SetActivityId(ref eventTraceActivity.ActivityId);
					}
					UnsafeNativeMethods.EventData length;
					length.Size = (data.Length + 1) * 2;
					length.Reserved = 0;
					fixed (string str1 = data)
					{
						string* strPointers = &str1;
						char* offsetToStringData = (char*)(&str1);
						if (&str1 != null)
						{
							offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
						}
						length.DataPointer = (ulong)offsetToStringData;
						num = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, 1, ref length);
					}
				}
				else
				{
					DiagnosticsEventProvider.errorCode = DiagnosticsEventProvider.WriteEventErrorCode.EventTooBig;
					return false;
				}
			}
			if (num == 0)
			{
				return true;
			}
			else
			{
				DiagnosticsEventProvider.SetLastError(num);
				return false;
			}
		}

		[SecurityCritical]
		protected internal bool WriteEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, int dataCount, IntPtr data)
		{
			if (eventTraceActivity != null)
			{
				DiagnosticsEventProvider.SetActivityId(ref eventTraceActivity.ActivityId);
			}
			uint num = UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, dataCount, (void*)data);
			if (num == 0)
			{
				return true;
			}
			else
			{
				DiagnosticsEventProvider.SetLastError(num);
				return false;
			}
		}

		[SecurityCritical]
		public unsafe bool WriteMessageEvent(EventTraceActivity eventTraceActivity, string eventMessage, byte eventLevel, long eventKeywords)
		{
			if (eventMessage != null)
			{
				if (eventTraceActivity != null)
				{
					DiagnosticsEventProvider.SetActivityId(ref eventTraceActivity.ActivityId);
				}
				if (this.IsEnabled(eventLevel, eventKeywords))
				{
					if (eventMessage.Length <= 0x7fd4)
					{
						fixed (string str = eventMessage)
						{
							string* strPointers = &str;
							char* offsetToStringData = (char*)(&str);
							if (&str != null)
							{
								offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
							}
							int num = UnsafeNativeMethods.EventWriteString(this.traceRegistrationHandle, eventLevel, eventKeywords, offsetToStringData);
						}
					}
					else
					{
						DiagnosticsEventProvider.errorCode = DiagnosticsEventProvider.WriteEventErrorCode.EventTooBig;
						return false;
					}
				}
				return true;
			}
			else
			{
				throw Fx.Exception.AsError(new ArgumentNullException("eventMessage"));
			}
		}

		[SecurityCritical]
		public bool WriteMessageEvent(EventTraceActivity eventTraceActivity, string eventMessage)
		{
			return this.WriteMessageEvent(eventTraceActivity, eventMessage, 0, (long)0);
		}

		[SecurityCritical]
		public unsafe bool WriteTransferEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid relatedActivityId, object[] eventPayload)
		{
			if (eventTraceActivity == null)
			{
				eventTraceActivity = EventTraceActivity.Empty;
			}
			uint num = 0;
			if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
			{
				if (eventPayload == null || (int)eventPayload.Length == 0)
				{
					num = UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle, ref eventDescriptor, ref eventTraceActivity.ActivityId, ref relatedActivityId, 0, 0);
				}
				else
				{
					int length = (int)eventPayload.Length;
					if (length <= 32)
					{
						uint size = 0;
						int num1 = 0;
						int[] numArray = new int[8];
						string[] strArrays = new string[8];
						UnsafeNativeMethods.EventData* eventDataPointer = (UnsafeNativeMethods.EventData*)length * sizeof(UnsafeNativeMethods.EventData);
						UnsafeNativeMethods.EventData* eventDataPointer1 = eventDataPointer;
						byte* numPointer = (byte*)16 * length;
						byte* numPointer1 = numPointer;
						for (int i = 0; i < (int)eventPayload.Length; i++)
						{
							if (eventPayload[i] != null)
							{
								string str = DiagnosticsEventProvider.EncodeObject(ref eventPayload[i], eventDataPointer1, numPointer1);
								numPointer1 = numPointer1 + 16;
								size = size + (*(eventDataPointer1)).Size;
								eventDataPointer1 = (UnsafeNativeMethods.EventData*)(eventDataPointer1 + sizeof(UnsafeNativeMethods.EventData));
								if (str != null)
								{
									if (num1 >= 8)
									{
										throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload", InternalSR.EtwAPIMaxStringCountExceeded(8)));
									}
									else
									{
										strArrays[num1] = str;
										numArray[num1] = i;
										num1++;
									}
								}
							}
						}
						if (size <= 0xffca)
						{
							fixed (string str1 = strArrays[0])
							{
								string* strPointers = &str1;
								char* offsetToStringData = (char*)(&str1);
								if (&str1 != null)
								{
									offsetToStringData = (char*)(strPointers + RuntimeHelpers.OffsetToStringData);
								}
								char* chrPointer = offsetToStringData;
								fixed (string str2 = strArrays[1])
								{
									string* strPointers1 = &str2;
									char* offsetToStringData1 = (char*)(&str2);
									if (&str2 != null)
									{
										offsetToStringData1 = (char*)(strPointers1 + RuntimeHelpers.OffsetToStringData);
									}
									char* chrPointer1 = offsetToStringData1;
									fixed (string str3 = strArrays[2])
									{
										string* strPointers2 = &str3;
										char* offsetToStringData2 = (char*)(&str3);
										if (&str3 != null)
										{
											offsetToStringData2 = (char*)(strPointers2 + RuntimeHelpers.OffsetToStringData);
										}
										char* chrPointer2 = offsetToStringData2;
										fixed (string str4 = strArrays[3])
										{
											string* strPointers3 = &str4;
											char* offsetToStringData3 = (char*)(&str4);
											if (&str4 != null)
											{
												offsetToStringData3 = (char*)(strPointers3 + RuntimeHelpers.OffsetToStringData);
											}
											char* chrPointer3 = offsetToStringData3;
											fixed (string str5 = strArrays[4])
											{
												string* strPointers4 = &str5;
												char* offsetToStringData4 = (char*)(&str5);
												if (&str5 != null)
												{
													offsetToStringData4 = (char*)(strPointers4 + RuntimeHelpers.OffsetToStringData);
												}
												char* chrPointer4 = offsetToStringData4;
												fixed (string str6 = strArrays[5])
												{
													string* strPointers5 = &str6;
													char* offsetToStringData5 = (char*)(&str6);
													if (&str6 != null)
													{
														offsetToStringData5 = (char*)(strPointers5 + RuntimeHelpers.OffsetToStringData);
													}
													char* chrPointer5 = offsetToStringData5;
													fixed (string str7 = strArrays[6])
													{
														string* strPointers6 = &str7;
														char* offsetToStringData6 = (char*)(&str7);
														if (&str7 != null)
														{
															offsetToStringData6 = (char*)(strPointers6 + RuntimeHelpers.OffsetToStringData);
														}
														char* chrPointer6 = offsetToStringData6;
														fixed (string str8 = strArrays[7])
														{
															string* strPointers7 = &str8;
															char* offsetToStringData7 = (char*)(&str8);
															if (&str8 != null)
															{
																offsetToStringData7 = (char*)(strPointers7 + RuntimeHelpers.OffsetToStringData);
															}
															char* chrPointer7 = offsetToStringData7;
															eventDataPointer1 = eventDataPointer;
															if (strArrays[0] != null)
															{
																(eventDataPointer1 + numArray[0] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer;
															}
															if (strArrays[1] != null)
															{
																(eventDataPointer1 + numArray[1] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer1;
															}
															if (strArrays[2] != null)
															{
																(eventDataPointer1 + numArray[2] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer2;
															}
															if (strArrays[3] != null)
															{
																(eventDataPointer1 + numArray[3] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer3;
															}
															if (strArrays[4] != null)
															{
																(eventDataPointer1 + numArray[4] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer4;
															}
															if (strArrays[5] != null)
															{
																(eventDataPointer1 + numArray[5] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer5;
															}
															if (strArrays[6] != null)
															{
																(eventDataPointer1 + numArray[6] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer6;
															}
															if (strArrays[7] != null)
															{
																(eventDataPointer1 + numArray[7] * sizeof(UnsafeNativeMethods.EventData)).DataPointer = (ulong)chrPointer7;
															}
															num = UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle, ref eventDescriptor, ref eventTraceActivity.ActivityId, ref relatedActivityId, length, eventDataPointer);
														}
													}
												}
											}
										}
									}
								}
							}
						}
						else
						{
							DiagnosticsEventProvider.errorCode = DiagnosticsEventProvider.WriteEventErrorCode.EventTooBig;
							return false;
						}
					}
					else
					{
						throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload", InternalSR.EtwMaxNumberArgumentsExceeded(32)));
					}
				}
			}
			if (num == 0)
			{
				return true;
			}
			else
			{
				DiagnosticsEventProvider.SetLastError(num);
				return false;
			}
		}

		[SecurityCritical]
		protected bool WriteTransferEvent(ref EventDescriptor eventDescriptor, EventTraceActivity eventTraceActivity, Guid relatedActivityId, int dataCount, IntPtr data)
		{
			if (eventTraceActivity != null)
			{
				uint num = UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle, ref eventDescriptor, ref eventTraceActivity.ActivityId, ref relatedActivityId, dataCount, (void*)data);
				if (num == 0)
				{
					return true;
				}
				else
				{
					DiagnosticsEventProvider.SetLastError(num);
					return false;
				}
			}
			else
			{
				throw Fx.Exception.ArgumentNull("eventTraceActivity");
			}
		}

		public enum WriteEventErrorCode
		{
			NoError,
			NoFreeBuffers,
			EventTooBig
		}
	}
}