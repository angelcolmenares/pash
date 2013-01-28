using System;
using System.ComponentModel;
using System.Runtime;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	public class ForestTrustDomainInformation
	{
		private string dnsName;

		private string nbName;

		private string sid;

		private ForestTrustDomainStatus status;

		internal LARGE_INTEGER time;

		public string DnsName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.dnsName;
			}
		}

		public string DomainSid
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.sid;
			}
		}

		public string NetBiosName
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.nbName;
			}
		}

		public ForestTrustDomainStatus Status
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.status;
			}
			set
			{
				if (value == ForestTrustDomainStatus.Enabled || value == ForestTrustDomainStatus.SidAdminDisabled || value == ForestTrustDomainStatus.SidConflictDisabled || value == ForestTrustDomainStatus.NetBiosNameAdminDisabled || value == ForestTrustDomainStatus.NetBiosNameConflictDisabled)
				{
					this.status = value;
					return;
				}
				else
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(ForestTrustDomainStatus));
				}
			}
		}

		internal ForestTrustDomainInformation(int flag, LSA_FOREST_TRUST_DOMAIN_INFO domainInfo, LARGE_INTEGER time)
		{
			this.status = (ForestTrustDomainStatus)flag;
			this.dnsName = Marshal.PtrToStringUni(domainInfo.DNSNameBuffer, domainInfo.DNSNameLength / 2);
			this.nbName = Marshal.PtrToStringUni(domainInfo.NetBIOSNameBuffer, domainInfo.NetBIOSNameLength / 2);
			IntPtr intPtr = (IntPtr)0;
			int stringSidW = UnsafeNativeMethods.ConvertSidToStringSidW(domainInfo.sid, ref intPtr);
			if (stringSidW != 0)
			{
				try
				{
					this.sid = Marshal.PtrToStringUni(intPtr);
				}
				finally
				{
					UnsafeNativeMethods.LocalFree(intPtr);
				}
				this.time = time;
				return;
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}
	}
}