using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Export", "AzureVM")]
	public class ExportAzureVMCommand : GetAzureVMCommand
	{
		[Parameter(Position=2, Mandatory=true, HelpMessage="The file path in which serialize the persistent VM role state.")]
		[ValidateNotNullOrEmpty]
		public string Path
		{
			get;
			set;
		}

		public ExportAzureVMCommand()
		{
		}

		public ExportAzureVMCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void SaveRoleState(PersistentVM role)
		{
			PersistentVMHelper.SaveStateToFile(role, this.Path);
		}
	}
}