using Microsoft.PowerShell.Commands.Management;
using System;
using System.Diagnostics;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Debug", "Process", DefaultParameterSetName="Name", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135206")]
	public sealed class DebugProcessCommand : ProcessBaseCommand
	{
		[Alias(new string[] { "PID", "ProcessId" })]
		[Parameter(ParameterSetName="Id", Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public int[] Id
		{
			get
			{
				return this.processIds;
			}
			set
			{
				this.myMode = ProcessBaseCommand.MatchMode.ById;
				this.processIds = value;
			}
		}

		[Alias(new string[] { "ProcessName" })]
		[Parameter(ParameterSetName="Name", Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true)]
		public string[] Name
		{
			get
			{
				return this.processNames;
			}
			set
			{
				this.myMode = ProcessBaseCommand.MatchMode.ByName;
				this.processNames = value;
			}
		}

		public DebugProcessCommand()
		{
		}

		protected override void ProcessRecord()
		{
			foreach (Process process in base.MatchingProcesses())
			{
				if (!base.ShouldProcess(StringUtil.Format(ProcessResources.ProcessNameForConfirmation, ProcessBaseCommand.SafeGetProcessName(process), ProcessBaseCommand.SafeGetProcessId(process))))
				{
					continue;
				}
				if (process.Id != 0)
				{
					string str = string.Concat("Select * From Win32_Process Where ProcessId=", ProcessBaseCommand.SafeGetProcessId(process));
					ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(str);
					ManagementObjectCollection managementObjectCollections = managementObjectSearcher.Get();
					foreach (ManagementObject managementObject in managementObjectCollections)
					{
						try
						{
							managementObject.InvokeMethod("AttachDebugger", null);
						}
						catch (ManagementException managementException1)
						{
							ManagementException managementException = managementException1;
							string message = managementException.Message;
							if (!string.IsNullOrEmpty(message))
							{
								message = message.Trim();
							}
							ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(StringUtil.Format(ProcessResources.DebuggerError, message, null)), "GetWMIManagementException", ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
						}
					}
				}
				else
				{
					base.WriteNonTerminatingError(process, null, ProcessResources.NoDebuggerFound, "NoDebuggerFound", ErrorCategory.ObjectNotFound);
				}
			}
		}
	}
}