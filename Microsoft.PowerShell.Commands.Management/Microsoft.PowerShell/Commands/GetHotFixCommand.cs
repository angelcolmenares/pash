using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Security.Principal;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "HotFix", DefaultParameterSetName="Default", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135217", RemotingCapability=RemotingCapability.SupportedByCommand)]
	[OutputType(new string[] { "System.Management.ManagementObject#root\\cimv2\\Win32_QuickFixEngineering" })]
	public sealed class GetHotFixCommand : PSCmdlet, IDisposable
	{
		private string[] _id;

		private string[] _description;

		private string[] _computername;

		private PSCredential _credential;

		private ManagementObjectSearcher searchProcess;

		private bool inputContainsWildcard;

		[Alias(new string[] { "CN", "__Server", "IPAddress" })]
		[Parameter(ValueFromPipelineByPropertyName=true)]
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
		[Parameter]
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

		[Parameter(ParameterSetName="Description")]
		[ValidateNotNullOrEmpty]
		public string[] Description
		{
			get
			{
				return this._description;
			}
			set
			{
				this._description = value;
			}
		}

		[Alias(new string[] { "HFID" })]
		[Parameter(Position=0, ParameterSetName="Default")]
		[ValidateNotNullOrEmpty]
		public string[] Id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}

		public GetHotFixCommand()
		{
			string[] strArrays = new string[1];
			strArrays[0] = "localhost";
			this._computername = strArrays;
		}

		protected override void BeginProcessing()
		{
			bool flag = false;
			string[] strArrays = this._computername;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				StringBuilder stringBuilder = new StringBuilder();
				ConnectionOptions connection = ComputerWMIHelper.GetConnection(AuthenticationLevel.Packet, ImpersonationLevel.Impersonate, this.Credential);
				ManagementScope managementScope = new ManagementScope(ComputerWMIHelper.GetScopeString(str, "\\root\\cimv2"), connection);
				managementScope.Connect();
				if (this._id == null)
				{
					stringBuilder.Append("Select * from Win32_QuickFixEngineering");
					flag = true;
				}
				else
				{
					stringBuilder.Append("Select * from Win32_QuickFixEngineering where (");
					for (int j = 0; j <= (int)this._id.Length - 1; j++)
					{
						stringBuilder.Append("HotFixID= '");
						stringBuilder.Append(this._id[j].ToString().Replace("'", "\\'"));
						stringBuilder.Append("'");
						if (j < (int)this._id.Length - 1)
						{
							stringBuilder.Append(" Or ");
						}
					}
					stringBuilder.Append(")");
				}
				this.searchProcess = new ManagementObjectSearcher(managementScope, new ObjectQuery(stringBuilder.ToString()));
				foreach (ManagementObject managementObject in this.searchProcess.Get())
				{
					if (this._description == null)
					{
						this.inputContainsWildcard = true;
					}
					else
					{
						if (!this.FilterMatch(managementObject))
						{
							continue;
						}
					}
					string item = (string)managementObject["InstalledBy"];
					if (!string.IsNullOrEmpty(item))
					{
						try
						{
							SecurityIdentifier securityIdentifier = new SecurityIdentifier(item);
							managementObject["InstalledBy"] = securityIdentifier.Translate(typeof(NTAccount));
						}
						catch (IdentityNotMappedException identityNotMappedException)
						{
						}
						catch (SystemException systemException1)
						{
							SystemException systemException = systemException1;
							CommandsCommon.CheckForSevereException(this, systemException);
						}
					}
					base.WriteObject(managementObject);
					flag = true;
				}
				if (!flag && !this.inputContainsWildcard)
				{
					Exception argumentException = new ArgumentException(StringUtil.Format(HotFixResources.NoEntriesFound, str));
					base.WriteError(new ErrorRecord(argumentException, "GetHotFixNoEntriesFound", ErrorCategory.ObjectNotFound, null));
				}
				if (this.searchProcess != null)
				{
					this.Dispose();
				}
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (disposing && this.searchProcess != null)
			{
				this.searchProcess.Dispose();
			}
		}

		private bool FilterMatch(ManagementObject obj)
		{
			bool flag;
			try
			{
				string[] strArrays = this._description;
				int num = 0;
				while (num < (int)strArrays.Length)
				{
					string str = strArrays[num];
					WildcardPattern wildcardPattern = new WildcardPattern(str, WildcardOptions.IgnoreCase);
					if (!wildcardPattern.IsMatch((string)obj["Description"]))
					{
						if (WildcardPattern.ContainsWildcardCharacters(str))
						{
							this.inputContainsWildcard = true;
						}
						num++;
					}
					else
					{
						flag = true;
						return flag;
					}
				}
				return false;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				CommandsCommon.CheckForSevereException(this, exception);
				flag = false;
			}
			return flag;
		}

		protected override void StopProcessing()
		{
			if (this.searchProcess != null)
			{
				this.searchProcess.Dispose();
			}
		}
	}
}