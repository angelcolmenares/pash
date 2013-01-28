using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Test", "Connection", DefaultParameterSetName="Default", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135266", RemotingCapability=RemotingCapability.OwnedByCommand)]
	[OutputType(new string[] { "System.Management.ManagementObject#root\\cimv2\\Win32_PingStatus" })]
	[OutputType(new Type[] { typeof(bool) })]
	public class TestConnectionCommand : PSCmdlet, IDisposable
	{
		private const string RegularParameterSet = "Default";

		private const string QuietParameterSet = "Quiet";

		private const string SourceParameterSet = "Source";

		private SwitchParameter asjob;

		private AuthenticationLevel authentication;

		private int buffersize;

		private string[] destination;

		private int count;

		private PSCredential credential;

		private string[] source;

		private ImpersonationLevel impersonation;

		private int throttlelimit;

		private int timetolive;

		private int delay;

		private bool quiet;

		private ManagementObjectSearcher searcher;

		private Dictionary<string, bool> quietResults;

		[Parameter(ParameterSetName="Default")]
		[Parameter(ParameterSetName="Source")]
		public SwitchParameter AsJob
		{
			get
			{
				return this.asjob;
			}
			set
			{
				this.asjob = value;
			}
		}

		[Parameter]
		public AuthenticationLevel Authentication
		{
			get
			{
				return this.authentication;
			}
			set
			{
				this.authentication = value;
			}
		}

		[Alias(new string[] { "Size", "Bytes", "BS" })]
		[Parameter]
		[ValidateRange(0, 0xffdc)]
		public int BufferSize
		{
			get
			{
				return this.buffersize;
			}
			set
			{
				this.buffersize = value;
			}
		}

		[Alias(new string[] { "CN", "IPAddress", "__SERVER", "Server", "Destination" })]
		[Parameter(Mandatory=true, Position=0, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return this.destination;
			}
			set
			{
				this.destination = value;
			}
		}

		[Parameter]
		[ValidateRange(1, 0xffffffff)]
		public int Count
		{
			get
			{
				return this.count;
			}
			set
			{
				this.count = value;
			}
		}

		[Credential]
		[Parameter(ParameterSetName="Source", Mandatory=false)]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get
			{
				return this.credential;
			}
			set
			{
				this.credential = value;
			}
		}

		[Parameter]
		[ValidateRange(1, 60)]
		public int Delay
		{
			get
			{
				return this.delay;
			}
			set
			{
				this.delay = value;
			}
		}

		[Parameter]
		public ImpersonationLevel Impersonation
		{
			get
			{
				return this.impersonation;
			}
			set
			{
				this.impersonation = value;
			}
		}

		[Parameter(ParameterSetName="Quiet")]
		public SwitchParameter Quiet
		{
			get
			{
				return this.quiet;
			}
			set
			{
				this.quiet = value;
			}
		}

		[Alias(new string[] { "FCN", "SRC" })]
		[Parameter(Position=1, ParameterSetName="Source", Mandatory=true)]
		[ValidateNotNullOrEmpty]
		public string[] Source
		{
			get
			{
				return this.source;
			}
			set
			{
				this.source = value;
			}
		}

		[Parameter(ParameterSetName="Source")]
		[Parameter(ParameterSetName="Default")]
		[ValidateRange(-2147483648, 0x3e8)]
		public int ThrottleLimit
		{
			get
			{
				return this.throttlelimit;
			}
			set
			{
				this.throttlelimit = value;
				if (this.throttlelimit <= 0)
				{
					this.throttlelimit = 32;
				}
			}
		}

		[Alias(new string[] { "TTL" })]
		[Parameter]
		[ValidateRange(1, 0xff)]
		public int TimeToLive
		{
			get
			{
				return this.timetolive;
			}
			set
			{
				this.timetolive = value;
			}
		}

		public TestConnectionCommand()
		{
			this.asjob = false;
			this.authentication = AuthenticationLevel.Packet;
			this.buffersize = 32;
			this.count = 4;
			string[] strArrays = new string[1];
			strArrays[0] = ".";
			this.source = strArrays;
			this.impersonation = ImpersonationLevel.Impersonate;
			this.throttlelimit = 32;
			this.timetolive = 80;
			this.delay = 1;
			this.quietResults = new Dictionary<string, bool>();
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

		private void ProcessPingStatus(ManagementBaseObject pingStatus)
		{
			string str = (string)LanguagePrimitives.ConvertTo(pingStatus.GetPropertyValue("Address"), typeof(string), CultureInfo.InvariantCulture);
			int num = (int)LanguagePrimitives.ConvertTo(pingStatus.GetPropertyValue("PrimaryAddressResolutionStatus"), typeof(int), CultureInfo.InvariantCulture);
			if (num == 0)
			{
				int num1 = (int)LanguagePrimitives.ConvertTo(pingStatus.GetPropertyValue("StatusCode"), typeof(int), CultureInfo.InvariantCulture);
				if (num1 == 0)
				{
					this.quietResults[str] = true;
					if (!this.quiet)
					{
						base.WriteObject(pingStatus);
					}
				}
				else
				{
					if (!this.quiet)
					{
						Win32Exception win32Exception = new Win32Exception(num1);
						string str1 = StringUtil.Format(ComputerResources.NoPingResult, str, win32Exception.Message);
						Exception pingException = new PingException(str1, win32Exception);
						ErrorRecord errorRecord = new ErrorRecord(pingException, "TestConnectionException", ErrorCategory.ResourceUnavailable, str);
						base.WriteError(errorRecord);
						return;
					}
				}
			}
			else
			{
				if (!this.quiet)
				{
					Win32Exception win32Exception1 = new Win32Exception(num);
					string str2 = StringUtil.Format(ComputerResources.NoPingResult, str, win32Exception1.Message);
					Exception exception = new PingException(str2, win32Exception1);
					ErrorRecord errorRecord1 = new ErrorRecord(exception, "TestConnectionException", ErrorCategory.ResourceUnavailable, str);
					base.WriteError(errorRecord1);
					return;
				}
			}
		}

		protected override void ProcessRecord()
		{
			ConnectionOptions connection = ComputerWMIHelper.GetConnection(this.Authentication, this.Impersonation, this.Credential);
			if (!this.asjob)
			{
				int num = 0;
				string[] strArrays = this.source;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					try
					{
						num++;
						string str1 = this.QueryString(this.destination, true, true);
						ObjectQuery objectQuery = new ObjectQuery(str1);
						ManagementScope managementScope = new ManagementScope(ComputerWMIHelper.GetScopeString(str, "\\root\\cimv2"), connection);
						managementScope.Options.EnablePrivileges = true;
						managementScope.Connect();
						EnumerationOptions enumerationOption = new EnumerationOptions();
						enumerationOption.UseAmendedQualifiers = true;
						enumerationOption.DirectRead = true;
						this.searcher = new ManagementObjectSearcher(managementScope, objectQuery, enumerationOption);
						for (int j = 0; j <= this.count - 1; j++)
						{
							ManagementObjectCollection managementObjectCollections = this.searcher.Get();
							int num1 = 0;
							foreach (ManagementBaseObject managementBaseObject in managementObjectCollections)
							{
								num1++;
								this.ProcessPingStatus(managementBaseObject);
								if (num1 >= managementObjectCollections.Count && j >= this.count - 1 && num >= (int)this.Source.Length)
								{
									continue;
								}
								Thread.Sleep(this.delay * 0x3e8);
							}
						}
					}
					catch (ManagementException managementException1)
					{
						ManagementException managementException = managementException1;
						ErrorRecord errorRecord = new ErrorRecord(managementException, "TestConnectionException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord);
					}
					catch (COMException cOMException1)
					{
						COMException cOMException = cOMException1;
						ErrorRecord errorRecord1 = new ErrorRecord(cOMException, "TestConnectionException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord1);
					}
				}
			}
			else
			{
				string str2 = this.QueryString(this.destination, true, false);
				GetWmiObjectCommand getWmiObjectCommand = new GetWmiObjectCommand();
				getWmiObjectCommand.Filter = str2.ToString();
				getWmiObjectCommand.Class = "Win32_PingStatus";
				getWmiObjectCommand.ComputerName = this.source;
				getWmiObjectCommand.Authentication = this.Authentication;
				getWmiObjectCommand.Impersonation = this.Impersonation;
				getWmiObjectCommand.ThrottleLimit = this.throttlelimit;
				PSWmiJob pSWmiJob = new PSWmiJob(getWmiObjectCommand, this.source, this.throttlelimit, base.MyInvocation.MyCommand.Name, this.count);
				base.JobRepository.Add(pSWmiJob);
				base.WriteObject(pSWmiJob);
			}
			if (this.quiet)
			{
				string[] strArrays1 = this.destination;
				for (int k = 0; k < (int)strArrays1.Length; k++)
				{
					string str3 = strArrays1[k];
					bool flag = false;
					this.quietResults.TryGetValue(str3, out flag);
					base.WriteObject(flag);
				}
			}
		}

		private string QueryString(string[] machinenames, bool escaperequired, bool selectrequired)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (selectrequired)
			{
				stringBuilder.Append("Select * from ");
				stringBuilder.Append("Win32_PingStatus");
				stringBuilder.Append(" where ");
			}
			stringBuilder.Append("((");
			for (int i = 0; i <= (int)machinenames.Length - 1; i++)
			{
				stringBuilder.Append("Address='");
				string str = machinenames[i].ToString();
				if (str.Equals(".", StringComparison.CurrentCultureIgnoreCase))
				{
					str = "localhost";
				}
				if (escaperequired)
				{
					str = str.Replace("\\", "\\\\'").ToString();
					str = str.Replace("'", "\\'").ToString();
				}
				stringBuilder.Append(str.ToString());
				stringBuilder.Append("'");
				if (i < (int)machinenames.Length - 1)
				{
					stringBuilder.Append(" Or ");
				}
			}
			stringBuilder.Append(")");
			stringBuilder.Append(" And ");
			stringBuilder.Append("TimeToLive=");
			stringBuilder.Append(this.timetolive);
			stringBuilder.Append(" And ");
			stringBuilder.Append("BufferSize=");
			stringBuilder.Append(this.buffersize);
			stringBuilder.Append(")");
			return stringBuilder.ToString();
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