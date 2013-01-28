using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace System.Runtime.Interop
{
	[SuppressUnmanagedCodeSecurity]
	internal static class UnsafeNativeMethods
	{
		public const string KERNEL32 = "kernel32.dll";

		public const string ADVAPI32 = "advapi32.dll";

		public const int ERROR_INVALID_HANDLE = 6;

		public const int ERROR_MORE_DATA = 234;

		public const int ERROR_ARITHMETIC_OVERFLOW = 0x216;

		public const int ERROR_NOT_ENOUGH_MEMORY = 8;

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		[SecurityCritical]
		public static extern SafeWaitHandle CreateWaitableTimer(IntPtr mustBeZero, bool manualReset, string timerName);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		[SecurityCritical]
		internal static extern void DebugBreak();

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern uint EventActivityIdControl(int ControlCode, out Guid ActivityId);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern unsafe uint EventRegister(ref Guid providerId, UnsafeNativeMethods.EtwEnableCallback enableCallback, void* callbackContext, out long registrationHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern uint EventUnregister(long registrationHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern unsafe uint EventWrite(long registrationHandle, ref System.Runtime.Diagnostics.EventDescriptor eventDescriptor, uint userDataCount, UnsafeNativeMethods.EventData* userData);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern unsafe uint EventWriteString(long registrationHandle, byte level, long keywords, char* message);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern unsafe uint EventWriteTransfer(long registrationHandle, ref System.Runtime.Diagnostics.EventDescriptor eventDescriptor, ref Guid activityId, ref Guid relatedActivityId, uint userDataCount, UnsafeNativeMethods.EventData* userData);

		[SecurityCritical]
		internal static string GetComputerName(ComputerNameFormat nameType)
		{
			int num = 0;
			if (!UnsafeNativeMethods.GetComputerNameEx(nameType, null, out num))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 234)
				{
					throw Fx.Exception.AsError(new Win32Exception(lastWin32Error));
				}
			}
			if (num < 0)
			{
				Fx.AssertAndThrow(string.Concat("GetComputerName returned an invalid length: ", num));
			}
			StringBuilder stringBuilder = new StringBuilder(num);
			if (UnsafeNativeMethods.GetComputerNameEx(nameType, stringBuilder, out num))
			{
				return stringBuilder.ToString();
			}
			else
			{
				int lastWin32Error1 = Marshal.GetLastWin32Error();
				throw Fx.Exception.AsError(new Win32Exception(lastWin32Error1));
			}
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Auto)]
		[SecurityCritical]
		private static extern bool GetComputerNameEx(ComputerNameFormat nameType, StringBuilder lpBuffer, out int size);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		[SecurityCritical]
		public static extern uint GetSystemTimeAdjustment(out int adjustment, out uint increment, out uint adjustmentDisabled);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		[SecurityCritical]
		public static extern void GetSystemTimeAsFileTime(out long time);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		[SecurityCritical]
		internal static extern bool IsDebuggerPresent();

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern void OutputDebugString(string lpOutputString);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		[SecurityCritical]
		public static extern int QueryPerformanceCounter(out long time);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		[SecurityCritical]
		internal static extern bool ReportEvent(SafeHandle hEventLog, ushort type, ushort category, uint eventID, byte[] userSID, ushort numStrings, uint dataLen, HandleRef strings, byte[] rawData);

		[DllImport("kernel32.dll", CharSet=CharSet.None)]
		[SecurityCritical]
		public static extern bool SetWaitableTimer(SafeWaitHandle handle, ref long dueTime, int period, IntPtr mustBeZero, IntPtr mustBeZeroAlso, bool resume);

		[SecurityCritical]
		internal unsafe delegate void EtwEnableCallback(ref Guid sourceId, int isEnabled, byte level, long matchAnyKeywords, long matchAllKeywords, void* filterData, void* callbackContext);

		[StructLayout(LayoutKind.Explicit)]
		public struct EventData
		{
			[FieldOffset(0)]
			internal ulong DataPointer;

			[FieldOffset(8)]
			internal uint Size;

			[FieldOffset(12)]
			internal int Reserved;

		}
	}
}