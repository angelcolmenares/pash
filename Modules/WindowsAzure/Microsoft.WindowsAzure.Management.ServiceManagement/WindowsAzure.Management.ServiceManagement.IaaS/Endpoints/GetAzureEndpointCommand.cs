using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.IaaS;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.Endpoints
{
	[Cmdlet("Get", "AzureEndpoint")]
	public class GetAzureEndpointCommand : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=false, HelpMessage="Endpoint name")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public GetAzureEndpointCommand()
		{
		}

		protected Collection<InputEndpointContext> GetInputEndpoints()
		{
			PersistentVM instance = base.VM.GetInstance();
			NetworkConfigurationSet networkConfigurationSet = instance.ConfigurationSets.OfType<NetworkConfigurationSet>().SingleOrDefault<NetworkConfigurationSet>();
			if (networkConfigurationSet == null)
			{
				networkConfigurationSet = new NetworkConfigurationSet();
				instance.ConfigurationSets.Add(networkConfigurationSet);
			}
			if (networkConfigurationSet.InputEndpoints == null)
			{
				networkConfigurationSet.InputEndpoints = new Collection<InputEndpoint>();
			}
			Collection<InputEndpoint> inputEndpoints = networkConfigurationSet.InputEndpoints;
			Collection<InputEndpointContext> inputEndpointContexts = new Collection<InputEndpointContext>();
			foreach (InputEndpoint inputEndpoint in inputEndpoints)
			{
				InputEndpointContext inputEndpointContext = new InputEndpointContext();
				inputEndpointContext.LBSetName = inputEndpoint.LoadBalancedEndpointSetName;
				inputEndpointContext.LocalPort = inputEndpoint.LocalPort;
				inputEndpointContext.Name = inputEndpoint.Name;
				inputEndpointContext.Port = inputEndpoint.Port;
				inputEndpointContext.Protocol = inputEndpoint.Protocol;
				inputEndpointContext.Vip = inputEndpoint.Vip;
				if (inputEndpoint.LoadBalancerProbe != null && !string.IsNullOrEmpty(inputEndpointContext.LBSetName))
				{
					inputEndpointContext.ProbePath = inputEndpoint.LoadBalancerProbe.Path;
					inputEndpointContext.ProbePort = inputEndpoint.LoadBalancerProbe.Port;
					inputEndpointContext.ProbeProtocol = inputEndpoint.LoadBalancerProbe.Protocol;
				}
				inputEndpointContexts.Add(inputEndpointContext);
			}
			return inputEndpointContexts;
		}

		protected override void ProcessRecord()
		{
			Func<InputEndpointContext, bool> func = null;
			Collection<InputEndpointContext> inputEndpoints = this.GetInputEndpoints();
			if (!string.IsNullOrEmpty(this.Name))
			{
				Collection<InputEndpointContext> inputEndpointContexts = inputEndpoints;
				if (func == null)
				{
					func = (InputEndpointContext ep) => string.Compare(ep.Name, this.Name, true) == 0;
				}
				InputEndpointContext inputEndpointContext = inputEndpointContexts.Where<InputEndpointContext>(func).SingleOrDefault<InputEndpointContext>();
				base.WriteObject(inputEndpointContext, true);
				return;
			}
			else
			{
				base.WriteObject(inputEndpoints, true);
				return;
			}
		}
	}
}