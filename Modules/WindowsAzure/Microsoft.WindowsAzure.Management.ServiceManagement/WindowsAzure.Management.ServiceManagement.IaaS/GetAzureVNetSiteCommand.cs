using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Get", "AzureVNetSite")]
	public class GetAzureVNetSiteCommand : ServiceManagementCmdletBase
	{
		[Parameter(Position=0, Mandatory=false, HelpMessage="The virtual network name.")]
		[ValidateNotNullOrEmpty]
		public string VNetName
		{
			get;
			set;
		}

		public GetAzureVNetSiteCommand()
		{
		}

		public GetAzureVNetSiteCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		public IEnumerable<VirtualNetworkSiteContext> GetVirtualNetworkSiteProcess()
		{
			IEnumerable<VirtualNetworkSiteContext> list;
			Func<string, VirtualNetworkSiteList> func = null;
			Func<VirtualNetworkSite, bool> func1 = null;
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					GetAzureVNetSiteCommand getAzureVNetSiteCommand = this;
					if (func == null)
					{
						func = (string s) => base.Channel.ListVirtualNetworkSites(s);
					}
					List<VirtualNetworkSite> virtualNetworkSites = ((CmdletBase<IServiceManagement>)getAzureVNetSiteCommand).RetryCall<VirtualNetworkSiteList>(func).ToList<VirtualNetworkSite>();
					if (!string.IsNullOrEmpty(this.VNetName))
					{
						List<VirtualNetworkSite> virtualNetworkSites1 = virtualNetworkSites;
						if (func1 == null)
						{
							func1 = (VirtualNetworkSite s) => string.Equals(s.Name, this.VNetName, StringComparison.InvariantCultureIgnoreCase);
						}
						virtualNetworkSites = virtualNetworkSites1.Where<VirtualNetworkSite>(func1).ToList<VirtualNetworkSite>();
						if (virtualNetworkSites.Count<VirtualNetworkSite>() == 0)
						{
							object[] vNetName = new object[1];
							vNetName[0] = this.VNetName;
							throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The specified virtual network name was not found: {0}", vNetName), "VirtualNetworkName");
						}
					}
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					list = virtualNetworkSites.Select<VirtualNetworkSite, VirtualNetworkSiteContext>((VirtualNetworkSite s) => {
						IEnumerable<string> addressPrefixes;
						IEnumerable<DnsServer> dnsServers;
						string profile;
						LocalNetworkSiteList sites;
						VirtualNetworkSiteContext virtualNetworkSiteContext = new VirtualNetworkSiteContext();
						virtualNetworkSiteContext.set_OperationId(operation.OperationTrackingId);
						virtualNetworkSiteContext.set_OperationDescription(this.CommandRuntime.ToString());
						virtualNetworkSiteContext.set_OperationStatus(operation.Status);
						VirtualNetworkSiteContext virtualNetworkSiteContext1 = virtualNetworkSiteContext;
						if (s.AddressSpace != null)
						{
							addressPrefixes = s.AddressSpace.AddressPrefixes;
						}
						else
						{
							addressPrefixes = null;
						}
						virtualNetworkSiteContext1.AddressSpacePrefixes = addressPrefixes;
						virtualNetworkSiteContext.AffinityGroup = s.AffinityGroup;
						VirtualNetworkSiteContext virtualNetworkSiteContext2 = virtualNetworkSiteContext;
						if (s.Dns != null)
						{
							dnsServers = s.Dns.DnsServers;
						}
						else
						{
							dnsServers = null;
						}
						virtualNetworkSiteContext2.DnsServers = dnsServers;
						VirtualNetworkSiteContext virtualNetworkSiteContext3 = virtualNetworkSiteContext;
						if (s.Gateway != null)
						{
							profile = s.Gateway.Profile;
						}
						else
						{
							profile = null;
						}
						virtualNetworkSiteContext3.GatewayProfile = profile;
						VirtualNetworkSiteContext virtualNetworkSiteContext4 = virtualNetworkSiteContext;
						if (s.Gateway != null)
						{
							sites = s.Gateway.Sites;
						}
						else
						{
							sites = null;
						}
						virtualNetworkSiteContext4.GatewaySites = sites;
						virtualNetworkSiteContext.Id = s.Id;
						virtualNetworkSiteContext.InUse = s.InUse;
						virtualNetworkSiteContext.Label = s.Label;
						virtualNetworkSiteContext.Name = s.Name;
						virtualNetworkSiteContext.State = s.State;
						virtualNetworkSiteContext.Subnets = s.Subnets;
						return virtualNetworkSiteContext;
					}
					).ToList<VirtualNetworkSiteContext>();
					return list;
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					if (communicationException as EndpointNotFoundException == null || base.IsVerbose())
					{
						this.WriteErrorDetails(communicationException);
					}
					else
					{
						list = null;
						return list;
					}
				}
				list = null;
			}
			return list;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				IEnumerable<VirtualNetworkSiteContext> virtualNetworkSiteProcess = this.GetVirtualNetworkSiteProcess();
				if (virtualNetworkSiteProcess != null)
				{
					base.WriteObject(virtualNetworkSiteProcess, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}