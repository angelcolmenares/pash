using Microsoft.Samples.WindowsAzure.ServiceManagement;
using Microsoft.WindowsAzure.Management.Cmdlets.Common;
using Microsoft.WindowsAzure.Management.Model;
using Microsoft.WindowsAzure.Management.ServiceManagement.IaaS;
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Security.Permissions;
using System.ServiceModel;

namespace Microsoft.WindowsAzure.Management.ServiceManagement.IaaS.PersistentVMs
{
	[Cmdlet("Get", "AzureRemoteDesktopFile", DefaultParameterSetName="Download")]
	public class GetAzureRemoteDesktopFileCommand : IaaSDeploymentManagementCmdletBase
	{
		[Parameter(Position=3, Mandatory=true, HelpMessage="Start a remote desktop session to the specified role instance.", ParameterSetName="Launch")]
		public SwitchParameter Launch
		{
			get;
			set;
		}

		[Parameter(Position=2, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Path and name of the output RDP file.", ParameterSetName="Download")]
		[Parameter(Position=2, Mandatory=false, ValueFromPipelineByPropertyName=true, HelpMessage="Path and name of the output RDP file.", ParameterSetName="Launch")]
		[ValidateNotNullOrEmpty]
		public string LocalPath
		{
			get;
			set;
		}

		[Alias(new string[] { "InstanceName" })]
		[Parameter(Position=1, Mandatory=true, ValueFromPipelineByPropertyName=true, HelpMessage="Name of the Role Instance or Virtual Machine Name to create/connect via RDP")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get;
			set;
		}

		public GetAzureRemoteDesktopFileCommand()
		{
		}

		public GetAzureRemoteDesktopFileCommand(IServiceManagement channel)
		{
			base.Channel = channel;
		}

		[SecurityPermission(SecurityAction.LinkDemand)]
		protected override void ProcessRecord()
		{
			try
			{
				base.ProcessRecord();
				this.ValidateParameters();
				ManagementOperationContext managementOperationContext = this.ReadRDPFileCommandProcess();
				if (managementOperationContext != null)
				{
					base.WriteObject(managementOperationContext, true);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, string.Empty, ErrorCategory.CloseError, null));
			}
		}

		[SecurityPermission(SecurityAction.LinkDemand)]
		public ManagementOperationContext ReadRDPFileCommandProcess()
		{
			Func<string, Stream> func = null;
			if (base.CurrentDeployment != null)
			{
				ManagementOperationContext managementOperationContext = null;
				using (OperationContextScope operationContextScope = new OperationContextScope((IContextChannel)base.Channel))
				{
					string localPath = this.LocalPath;
					string tempFileName = localPath;
					if (localPath == null)
					{
						tempFileName = Path.GetTempFileName();
					}
					string str = tempFileName;
					try
					{
						GetAzureRemoteDesktopFileCommand getAzureRemoteDesktopFileCommand = this;
						if (func == null)
						{
							func = (string s) => base.Channel.DownloadRDPFile(s, this.ServiceName, base.CurrentDeployment.Name, string.Concat(this.Name, "_IN_0"));
						}
						Stream stream = ((CmdletBase<IServiceManagement>)getAzureRemoteDesktopFileCommand).RetryCall<Stream>(func);
						using (stream)
						{
							FileStream fileStream = File.Create(str);
							using (fileStream)
							{
								byte[] numArray = new byte[0x3e8];
								while (true)
								{
									int num = stream.Read(numArray, 0, (int)numArray.Length);
									int num1 = num;
									if (num <= 0)
									{
										break;
									}
									fileStream.Write(numArray, 0, num1);
								}
							}
							Operation operation = base.WaitForOperation(base.CommandRuntime.ToString());
							ManagementOperationContext managementOperationContext1 = new ManagementOperationContext();
							managementOperationContext1.set_OperationDescription(base.CommandRuntime.ToString());
							managementOperationContext1.set_OperationStatus(operation.Status);
							managementOperationContext1.set_OperationId(operation.OperationTrackingId);
							managementOperationContext = managementOperationContext1;
						}
						SwitchParameter launch = this.Launch;
						if (launch.IsPresent)
						{
							ProcessStartInfo processStartInfo = new ProcessStartInfo();
							processStartInfo.CreateNoWindow = true;
							processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
							ProcessStartInfo processStartInfo1 = processStartInfo;
							ProcessStartInfo processStartInfo2 = new ProcessStartInfo();
							processStartInfo2.CreateNoWindow = true;
							processStartInfo2.WindowStyle = ProcessWindowStyle.Hidden;
							if (this.LocalPath != null)
							{
								processStartInfo1.FileName = "mstsc.exe";
								processStartInfo1.Arguments = str;
							}
							else
							{
								Guid guid = Guid.NewGuid();
								string str1 = guid.ToString();
								string str2 = string.Concat(Path.GetTempPath(), str1, ".bat");
								FileStream fileStream1 = File.OpenWrite(str2);
								using (fileStream1)
								{
									StreamWriter streamWriter = new StreamWriter(fileStream1);
									streamWriter.WriteLine(string.Concat("start /wait mstsc.exe ", str));
									streamWriter.WriteLine(string.Concat("del ", str));
									streamWriter.WriteLine(string.Concat("del ", str2));
									streamWriter.Flush();
								}
								processStartInfo1.FileName = str2;
							}
							Process.Start(processStartInfo1);
						}
					}
					catch (CommunicationException communicationException1)
					{
						CommunicationException communicationException = communicationException1;
						this.WriteErrorDetails(communicationException);
					}
				}
				return managementOperationContext;
			}
			else
			{
				throw new ArgumentException("Cloud Service is not present or there is no virtual machine deployment.");
			}
		}
	}
}