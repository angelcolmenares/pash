using Microsoft.PowerShell.Commands.Management;
using System;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Stop", "Computer", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135263", RemotingCapability=RemotingCapability.SupportedByCommand)]
	public sealed class StopComputerCommand : PSCmdlet, IDisposable
	{
		private SwitchParameter _asjob;

		private AuthenticationLevel _authentication;

		private string[] _computername;

		private PSCredential _credential;

		private ImpersonationLevel _impersonation;

		private int _throttlelimit;

		private SwitchParameter _force;

		private ManagementObjectSearcher searcher;

		[Parameter]
		public SwitchParameter AsJob
		{
			get
			{
				return this._asjob;
			}
			set
			{
				this._asjob = value;
			}
		}

		[Parameter]
		public AuthenticationLevel Authentication
		{
			get
			{
				return this._authentication;
			}
			set
			{
				this._authentication = value;
			}
		}

		[Alias(new string[] { "CN", "__SERVER", "Server", "IPAddress" })]
		[Parameter(Position=0, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return this._computername;
			}
			set
			{
				this._computername = value;
			}
		}

		[Credential]
		[Parameter(Position=1)]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get
			{
				return this._credential;
			}
			set
			{
				this._credential = value;
			}
		}

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this._force;
			}
			set
			{
				this._force = value;
			}
		}

		[Parameter]
		public ImpersonationLevel Impersonation
		{
			get
			{
				return this._impersonation;
			}
			set
			{
				this._impersonation = value;
			}
		}

		[Parameter]
		[ValidateRange(-2147483648, 0x3e8)]
		public int ThrottleLimit
		{
			get
			{
				return this._throttlelimit;
			}
			set
			{
				this._throttlelimit = value;
				if (this._throttlelimit <= 0)
				{
					this._throttlelimit = 32;
				}
			}
		}

		public StopComputerCommand()
		{
			this._asjob = false;
			this._authentication = AuthenticationLevel.Packet;
			string[] strArrays = new string[1];
			strArrays[0] = ".";
			this._computername = strArrays;
			this._impersonation = ImpersonationLevel.Impersonate;
			this._throttlelimit = 32;
			this._force = false;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (disposing && this.searcher != null)
			{
				this.searcher.Dispose();
			}
		}

		protected override void ProcessRecord()
		{
			string hostName;
			ConnectionOptions connection = ComputerWMIHelper.GetConnection(this.Authentication, this.Impersonation, this.Credential);
			object[] objArray = new object[2];
			objArray[0] = 1;
			objArray[1] = 0;
			object[] objArray1 = objArray;
			if (this._force.IsPresent)
			{
				objArray1[0] = 5;
			}
			if (!this._asjob.IsPresent)
			{
				string empty = string.Empty;
				string[] strArrays = this._computername;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					if (str.Equals("localhost", StringComparison.CurrentCultureIgnoreCase) || str.Equals(".", StringComparison.OrdinalIgnoreCase))
					{
						hostName = Dns.GetHostName();
						empty = "localhost";
					}
					else
					{
						hostName = str;
					}
					if (base.ShouldProcess(StringUtil.Format(ComputerResources.DoubleComputerName, empty, hostName)))
					{
						try
						{
							ManagementScope managementScope = new ManagementScope(ComputerWMIHelper.GetScopeString(str, "\\root\\cimv2"), connection);
							EnumerationOptions enumerationOption = new EnumerationOptions();
							enumerationOption.UseAmendedQualifiers = true;
							enumerationOption.DirectRead = true;
							ObjectQuery objectQuery = new ObjectQuery("select * from Win32_OperatingSystem");
							this.searcher = new ManagementObjectSearcher(managementScope, objectQuery, enumerationOption);
							foreach (ManagementObject managementObject in this.searcher.Get())
							{
								object obj = managementObject.InvokeMethod("Win32shutdown", objArray1);
								int num = Convert.ToInt32(obj.ToString(), CultureInfo.CurrentCulture);
								if (num == 0)
								{
									continue;
								}
								ComputerWMIHelper.WriteNonTerminatingError(num, this, hostName);
							}
						}
						catch (ManagementException managementException1)
						{
							ManagementException managementException = managementException1;
							ErrorRecord errorRecord = new ErrorRecord(managementException, "StopComputerException", ErrorCategory.InvalidOperation, hostName);
							base.WriteError(errorRecord);
						}
						catch (COMException cOMException1)
						{
							COMException cOMException = cOMException1;
							ErrorRecord errorRecord1 = new ErrorRecord(cOMException, "StopComputerException", ErrorCategory.InvalidOperation, hostName);
							base.WriteError(errorRecord1);
						}
					}
				}
				return;
			}
			else
			{
				string machineNames = ComputerWMIHelper.GetMachineNames(this.ComputerName);
				if (base.ShouldProcess(machineNames))
				{
					InvokeWmiMethod invokeWmiMethod = new InvokeWmiMethod();
					invokeWmiMethod.Path = "Win32_OperatingSystem=@";
					invokeWmiMethod.ComputerName = this._computername;
					invokeWmiMethod.Authentication = this._authentication;
					invokeWmiMethod.Impersonation = this._impersonation;
					invokeWmiMethod.Credential = this._credential;
					invokeWmiMethod.ThrottleLimit = this._throttlelimit;
					invokeWmiMethod.Name = "Win32Shutdown";
					invokeWmiMethod.EnableAllPrivileges = SwitchParameter.Present;
					invokeWmiMethod.ArgumentList = objArray1;
					PSWmiJob pSWmiJob = new PSWmiJob(invokeWmiMethod, this._computername, this._throttlelimit, Job.GetCommandTextFromInvocationInfo(base.MyInvocation));
					base.JobRepository.Add(pSWmiJob);
					base.WriteObject(pSWmiJob);
					return;
				}
				else
				{
					return;
				}
			}
		}

		protected override void StopProcessing()
		{
			if (this.searcher != null)
			{
				this.searcher.Dispose();
			}
		}
	}
}