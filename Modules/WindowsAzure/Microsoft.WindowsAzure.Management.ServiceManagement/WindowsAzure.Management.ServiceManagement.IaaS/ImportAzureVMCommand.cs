using Microsoft.WindowsAzure.Management.ServiceManagement.Helpers;
using Microsoft.WindowsAzure.Management.ServiceManagement.Model;
using System;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Import", "AzureVM")]
	public class ImportAzureVMCommand : Cmdlet
	{
		[Parameter(Position=0, Mandatory=true, HelpMessage="Path to the file with the persistent VM role state previously serialized.")]
		[ValidateNotNullOrEmpty]
		public string Path
		{
			get;
			set;
		}

		public ImportAzureVMCommand()
		{
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				PersistentVM persistentVM = PersistentVMHelper.LoadStateFromFile(this.Path);
				base.WriteObject(persistentVM, true);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}
	}
}