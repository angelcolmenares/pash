using System;
using System.Management;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Register", "WmiEvent", DefaultParameterSetName="class", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135245", RemotingCapability=RemotingCapability.OwnedByCommand)]
	public class RegisterWmiEventCommand : ObjectEventRegistrationBase
	{
		private long timeOut;

		private bool timeoutSpecified;

		private string objectQuery;

		private string className;

		private string computerName;

		private string nameSpace;

		private PSCredential credential;

		[Parameter(Position=0, Mandatory=true, ParameterSetName="class")]
		public string Class
		{
			get
			{
				return this.className;
			}
			set
			{
				this.className = value;
			}
		}

		[Alias(new string[] { "Cn" })]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public string ComputerName
		{
			get
			{
				return this.computerName;
			}
			set
			{
				this.computerName = value;
			}
		}

		[Credential]
		[Parameter]
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

		[Alias(new string[] { "NS" })]
		[Parameter]
		public string Namespace
		{
			get
			{
				return this.nameSpace;
			}
			set
			{
				this.nameSpace = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="query")]
		public string Query
		{
			get
			{
				return this.objectQuery;
			}
			set
			{
				this.objectQuery = value;
			}
		}

		[Alias(new string[] { "TimeoutMSec" })]
		[Parameter]
		public long Timeout
		{
			get
			{
				return this.timeOut;
			}
			set
			{
				this.timeOut = value;
				this.timeoutSpecified = true;
			}
		}

		public RegisterWmiEventCommand()
		{
			this.computerName = "localhost";
			this.nameSpace = "root\\cimv2";
		}

		private string BuildEventQuery(string objectName)
		{
			StringBuilder stringBuilder = new StringBuilder("select * from ");
			stringBuilder.Append(objectName);
			return stringBuilder.ToString();
		}

		protected override void EndProcessing()
		{
			base.EndProcessing();
			PSEventSubscriber newSubscriber = base.NewSubscriber;
			if (newSubscriber != null)
			{
				newSubscriber.Unsubscribed += new PSEventUnsubscribedEventHandler(this.newSubscriber_Unsubscribed);
			}
		}

		private string GetScopeString(string computer, string namespaceParameter)
		{
			StringBuilder stringBuilder = new StringBuilder("\\\\");
			stringBuilder.Append(computer);
			stringBuilder.Append("\\");
			stringBuilder.Append(namespaceParameter);
			return stringBuilder.ToString();
		}

		protected override object GetSourceObject()
		{
			string query = this.Query;
			if (this.Class != null)
			{
				for (int i = 0; i < this.Class.Length; i++)
				{
					if (!char.IsLetterOrDigit(this.Class[i]))
					{
						char @class = this.Class[i];
						if (!@class.Equals('\u005F'))
						{
							object[] objArray = new object[1];
							objArray[0] = this.Class;
							ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(string.Format(Thread.CurrentThread.CurrentCulture, "Class", objArray)), "INVALID_QUERY_IDENTIFIER", ErrorCategory.InvalidArgument, null);
							errorRecord.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiInvalidClass", new object[0]);
							base.ThrowTerminatingError(errorRecord);
							return null;
						}
					}
				}
				query = this.BuildEventQuery(this.Class);
			}
			ConnectionOptions connectionOption = new ConnectionOptions();
			if (this.Credential != null)
			{
				NetworkCredential networkCredential = this.Credential.GetNetworkCredential();
				if (!string.IsNullOrEmpty(networkCredential.Domain))
				{
					connectionOption.Username = string.Concat(networkCredential.Domain, "\\", networkCredential.UserName);
				}
				else
				{
					connectionOption.Username = networkCredential.UserName;
				}
				connectionOption.Password = networkCredential.Password;
			}
			ManagementScope managementScope = new ManagementScope(this.GetScopeString(this.computerName, this.Namespace), connectionOption);
			EventWatcherOptions eventWatcherOption = new EventWatcherOptions();
			if (this.timeoutSpecified)
			{
				eventWatcherOption.Timeout = new TimeSpan(this.timeOut * (long)0x2710);
			}
			ManagementEventWatcher managementEventWatcher = new ManagementEventWatcher(managementScope, new EventQuery(query), eventWatcherOption);
			return managementEventWatcher;
		}

		protected override string GetSourceObjectEventName()
		{
			return "EventArrived";
		}

		private void newSubscriber_Unsubscribed(object sender, PSEventUnsubscribedEventArgs e)
		{
			ManagementEventWatcher managementEventWatcher = sender as ManagementEventWatcher;
			if (managementEventWatcher != null)
			{
				managementEventWatcher.Stop();
			}
		}
	}
}