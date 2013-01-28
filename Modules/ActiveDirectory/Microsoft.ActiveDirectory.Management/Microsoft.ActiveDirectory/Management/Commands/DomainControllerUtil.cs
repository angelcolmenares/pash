using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class DomainControllerUtil
	{
		internal const string DiscoveryInternalPropertyDCAddress = "DiscoveryInternalPropertyDCAddress";

		public DomainControllerUtil()
		{
		}

		internal static ADEntity DiscoverDomainController(string siteName, string domainName, ADDiscoverableService[] servicesToFind, ADDiscoverDomainControllerOptions discoverOptions, ADMinimumDirectoryServiceVersion? minDSVersion)
		{
			DOMAIN_CONTROLLER_INFO structure;
			string domainControllerName;
			IntPtr zero = IntPtr.Zero;
			uint num = 0;
			if (servicesToFind.Length != 0)
			{
				ADDiscoverableService[] aDDiscoverableServiceArray = servicesToFind;
				for (int i = 0; i < (int)aDDiscoverableServiceArray.Length; i++)
				{
					ADDiscoverableService aDDiscoverableService = aDDiscoverableServiceArray[i];
					ADDiscoverableService aDDiscoverableService1 = aDDiscoverableService;
					if (aDDiscoverableService1 == ADDiscoverableService.PrimaryDC)
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.PdcRequired);
					}
					else if (aDDiscoverableService1 == ADDiscoverableService.GlobalCatalog)
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.GCRequired);
					}
					else if (aDDiscoverableService1 == ADDiscoverableService.KDC)
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.KdcRequired);
					}
					else if (aDDiscoverableService1 == ADDiscoverableService.TimeService)
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.TimeServerRequired);
					}
					else if (aDDiscoverableService1 == ADDiscoverableService.ReliableTimeService)
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.GoodTimeServerPreferred);
						num = num | Convert.ToUInt32(ADLocatorFlags.TimeServerRequired);
					}
					else if (aDDiscoverableService1 == ADDiscoverableService.ADWS)
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.WebServiceRequired);
					}
					else
					{
						object[] str = new object[1];
						str[0] = (object)aDDiscoverableService.ToString();
						throw new ADException(string.Format(CultureInfo.CurrentCulture, StringResources.UnsupportedOptionSpecified, str));
					}
				}
			}
			ADMinimumDirectoryServiceVersion valueOrDefault = minDSVersion.GetValueOrDefault();
			if (minDSVersion.HasValue)
			{
				switch (valueOrDefault)
				{
					case ADMinimumDirectoryServiceVersion.Windows2000:
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.DirectoryServicesRequired);
						break;
					}
					case ADMinimumDirectoryServiceVersion.Windows2008:
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.DirectoryServices6Required);
						break;
					}
					case ADMinimumDirectoryServiceVersion.Windows2012:
					{
						num = num | Convert.ToUInt32(ADLocatorFlags.DirectoryServices8Required);
						break;
					}
				}
			}
			if ((discoverOptions & ADDiscoverDomainControllerOptions.Writable) == ADDiscoverDomainControllerOptions.Writable)
			{
				num = num | Convert.ToUInt32(ADLocatorFlags.WriteableRequired);
			}
			if ((discoverOptions & ADDiscoverDomainControllerOptions.AvoidSelf) == ADDiscoverDomainControllerOptions.AvoidSelf)
			{
				num = num | Convert.ToUInt32(ADLocatorFlags.AvoidSelf);
			}
			if ((discoverOptions & ADDiscoverDomainControllerOptions.ForceDiscover) == ADDiscoverDomainControllerOptions.ForceDiscover)
			{
				num = num | Convert.ToUInt32(ADLocatorFlags.ForceRediscovery);
			}
			if ((discoverOptions & ADDiscoverDomainControllerOptions.TryNextClosestSite) == ADDiscoverDomainControllerOptions.TryNextClosestSite)
			{
				num = num | Convert.ToUInt32(ADLocatorFlags.TryNextClosestSite);
			}
			if ((discoverOptions & ADDiscoverDomainControllerOptions.ReturnDnsName) == ADDiscoverDomainControllerOptions.ReturnDnsName)
			{
				num = num | Convert.ToUInt32(ADLocatorFlags.ReturnDnsName);
			}
			if ((discoverOptions & ADDiscoverDomainControllerOptions.ReturnFlatName) == ADDiscoverDomainControllerOptions.ReturnFlatName)
			{
				num = num | Convert.ToUInt32(-2147483648);
			}
			try
			{
				int num1 = UnsafeNativeMethods.DsGetDcName(null, domainName, 0, siteName, num, out zero);
				if (num1 != 0)
				{
					Win32Exception win32Exception = new Win32Exception(num1);
					throw new ADException(win32Exception.Message, num1);
				}
				else
				{
					structure = (DOMAIN_CONTROLLER_INFO)Marshal.PtrToStructure(zero, typeof(DOMAIN_CONTROLLER_INFO));
				}
			}
			finally
			{
				UnsafeNativeMethods.NetApiBufferFree(zero);
			}
			ADEntity aDEntity = new ADEntity();
			if (!structure.DomainControllerName.StartsWith("\\\\"))
			{
				domainControllerName = structure.DomainControllerName;
			}
			else
			{
				domainControllerName = structure.DomainControllerName.Substring(2);
			}
			if ((structure.Flags & 0x20000000) != 0x20000000)
			{
				aDEntity.Add("Name", domainControllerName);
			}
			else
			{
				aDEntity.Add("HostName", domainControllerName);
			}
			aDEntity.Add("Domain", structure.DomainName);
			aDEntity.Add("Forest", structure.DnsForestName);
			aDEntity.Add("Site", structure.DcSiteName);
			aDEntity.InternalProperties.Add("DiscoveryInternalPropertyDCAddress", structure.DomainControllerAddress);
			return aDEntity;
		}
	}
}