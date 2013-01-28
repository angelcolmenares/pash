using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using System;
using System.IO;
using System.Management.Automation;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS
{
	[Cmdlet("Remove", "AzureVNetConfig")]
	public class RemoveAzureVNetConfigCommand : ServiceManagementCmdletBase
	{
		private static XNamespace netconfigNamespace;

		private static XNamespace instanceNamespace;

		static RemoveAzureVNetConfigCommand()
		{
			RemoveAzureVNetConfigCommand.netconfigNamespace = "http://schemas.microsoft.com/ServiceHosting/2011/07/NetworkConfiguration";
			RemoveAzureVNetConfigCommand.instanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
		}

		public RemoveAzureVNetConfigCommand()
		{
		}

		public RemoveAzureVNetConfigCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				string str = this.RemoveVirtualNetworkConfigProcess();
				if (!string.IsNullOrEmpty(str))
				{
					ManagementOperationContext managementOperationContext = new ManagementOperationContext();
					managementOperationContext.set_OperationId(str);
					ManagementOperationContext managementOperationContext1 = managementOperationContext;
					base.WriteObject(managementOperationContext1, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		public string RemoveVirtualNetworkConfigProcess()
		{
			Action<string> action = null;
			object[] xAttribute = new object[3];
			xAttribute[0] = new XAttribute("xmlns", RemoveAzureVNetConfigCommand.netconfigNamespace.NamespaceName);
			xAttribute[1] = new XAttribute(XNamespace.Xmlns + "xsi", RemoveAzureVNetConfigCommand.instanceNamespace.NamespaceName);
			xAttribute[2] = new XElement(RemoveAzureVNetConfigCommand.netconfigNamespace + "VirtualNetworkConfiguration");
			XElement xElement = new XElement(RemoveAzureVNetConfigCommand.netconfigNamespace + "NetworkConfiguration", xAttribute);
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter xmlWriter = XmlWriter.Create(memoryStream);
			xElement.WriteTo(xmlWriter);
			xmlWriter.Flush();
			memoryStream.Seek((long)0, SeekOrigin.Begin);
			using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
			{
				try
				{
					RemoveAzureVNetConfigCommand removeAzureVNetConfigCommand = this;
					if (action == null)
					{
						action = (string s) => base.Channel.SetNetworkConfiguration(s, memoryStream);
					}
					((CmdletBase<IServiceManagement>)removeAzureVNetConfigCommand).RetryCall(action);
					Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
					ManagementOperationContext managementOperationContext = new ManagementOperationContext();
					managementOperationContext.set_OperationDescription(base.CommandRuntime.ToString());
					managementOperationContext.set_OperationId(operation.OperationTrackingId);
					managementOperationContext.set_OperationStatus(operation.Status);
					ManagementOperationContext managementOperationContext1 = managementOperationContext;
					base.WriteObject(managementOperationContext1, true);
				}
				catch (CommunicationException communicationException1)
				{
					CommunicationException communicationException = communicationException1;
					this.WriteErrorDetails(communicationException);
				}
			}
			return null;
		}
	}
}