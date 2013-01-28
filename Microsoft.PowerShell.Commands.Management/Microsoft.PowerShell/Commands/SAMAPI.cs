using System;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	internal static class SAMAPI
	{
		internal const int WorkrGroupMachine = 0xa84;

		internal const int MaxMachineNameLength = 15;

		internal static void FreeLsaString(ref SAMAPI.LSA_UNICODE_STRING s)
		{
			if (s.Buffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(s.Buffer);
				s.Buffer = IntPtr.Zero;
				return;
			}
			else
			{
				return;
			}
		}

		[DllImport("netapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern int I_NetLogonControl2(string lpServerName, uint lpFunctionCode, uint lpQueryLevel, ref IntPtr lpInputData, out IntPtr queryInformation);

		internal static void InitLsaString(string s, ref SAMAPI.LSA_UNICODE_STRING lus)
		{
			ushort num = 0x7ffe;
			if (s.Length <= num)
			{
				lus.Buffer = Marshal.StringToHGlobalUni(s);
				lus.Length = (ushort)(s.Length * 2);
				lus.MaximumLength = (ushort)((s.Length + 1) * 2);
				return;
			}
			else
			{
				throw new ArgumentException("String too long");
			}
		}

		[DllImport("advapi32", CharSet=CharSet.None)]
		internal static extern int LsaClose(IntPtr policyHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern uint LsaCreateSecret(IntPtr policyHandle, ref SAMAPI.LSA_UNICODE_STRING secretName, uint desiredAccess, out IntPtr secretHandle);

		[DllImport("advapi32", CharSet=CharSet.None)]
		internal static extern int LsaFreeMemory(IntPtr buffer);

		[DllImport("advapi32", CharSet=CharSet.None)]
		internal static extern int LsaNtStatusToWinError(uint ntStatus);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern uint LsaOpenPolicy(ref SAMAPI.LSA_UNICODE_STRING systemName, ref SAMAPI.LSA_OBJECT_ATTRIBUTES objectAttributes, uint desiredAccess, out IntPtr policyHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern uint LsaOpenSecret(IntPtr policyHandle, ref SAMAPI.LSA_UNICODE_STRING secretName, uint accessMask, out IntPtr secretHandle);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern uint LsaQuerySecret(IntPtr secretHandle, out IntPtr currentValue, IntPtr currentValueSetTime, IntPtr oldValue, IntPtr oldValueSetTime);

		[DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
		internal static extern uint LsaSetSecret(IntPtr secretHandle, ref SAMAPI.LSA_UNICODE_STRING currentValue, ref SAMAPI.LSA_UNICODE_STRING oldValue);

		[DllImport("Netapi32.dll", CharSet=CharSet.None)]
		internal static extern int NetApiBufferFree(IntPtr Buffer);

		internal enum LSA_ACCESS
		{
			Read = 131078,
			Write = 133112,
			Execute = 133121,
			AllAccess = 987135
		}

		internal struct LSA_OBJECT_ATTRIBUTES
		{
			internal int Length;

			internal IntPtr RootDirectory;

			internal IntPtr ObjectName;

			internal int Attributes;

			internal IntPtr SecurityDescriptor;

			internal IntPtr SecurityQualityOfService;

		}

		internal struct LSA_UNICODE_STRING
		{
			internal ushort Length;

			internal ushort MaximumLength;

			internal IntPtr Buffer;

		}

		internal struct NetLogonInfo2
		{
			internal uint Flags;

			internal uint PdcConnectionStatus;

			internal string TrustedDcName;

			internal uint TdcConnectionStatus;

		}
	}
}