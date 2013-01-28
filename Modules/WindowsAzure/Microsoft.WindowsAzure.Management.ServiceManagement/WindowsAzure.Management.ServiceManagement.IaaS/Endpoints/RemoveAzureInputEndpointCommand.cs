using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.IaaS;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.Endpoints
{
	[Cmdlet("Remove", "AzureEndpoint")]
	public class RemoveAzureInputEndpointCommand : VirtualMachineConfigurationCmdletBase
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Endpoint Name")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public RemoveAzureInputEndpointCommand()
		{
		}

		protected Collection<InputEndpoint> GetInputEndpoints()
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
			return inputEndpoints;
		}

		protected override void ProcessRecord()
		{
			Func<InputEndpoint, bool> func = null;
			try
			{
				base.ProcessRecord();
				Collection<InputEndpoint> inputEndpoints = this.GetInputEndpoints();
				Collection<InputEndpoint> inputEndpoints1 = inputEndpoints;
				if (func == null)
				{
					func = (InputEndpoint ep) => string.Compare(ep.Name, this.Name, true) == 0;
				}
				InputEndpoint inputEndpoint = inputEndpoints1.Where<InputEndpoint>(func).SingleOrDefault<InputEndpoint>();
				if (inputEndpoint == null)
				{
					object[] name = new object[1];
					name[0] = this.Name;
					base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "An endpoint named '{0}' cannot be found in the configuration of this VM.", name)), string.Empty, ErrorCategory.InvalidData, null));
				}
				inputEndpoints.Remove(inputEndpoint);
				base.WriteObject(base.VM, true);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}