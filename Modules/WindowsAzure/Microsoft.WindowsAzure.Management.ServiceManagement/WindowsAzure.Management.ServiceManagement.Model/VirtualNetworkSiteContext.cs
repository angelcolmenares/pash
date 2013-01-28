using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.Collections.Generic;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Model
{
	public class VirtualNetworkSiteContext : ManagementOperationContext
	{
		public IEnumerable<string> AddressSpacePrefixes
		{
			get;
			set;
		}

		public string AffinityGroup
		{
			get;
			set;
		}

		public IEnumerable<DnsServer> DnsServers
		{
			get;
			set;
		}

		public string GatewayProfile
		{
			get;
			set;
		}

		public LocalNetworkSiteList GatewaySites
		{
			get;
			set;
		}

		public string Id
		{
			get;
			set;
		}

		public bool InUse
		{
			get;
			set;
		}

		public string Label
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public string State
		{
			get;
			set;
		}

		public IEnumerable<Subnet> Subnets
		{
			get;
			set;
		}

		public VirtualNetworkSiteContext()
		{
		}
	}
}