using System;
using System.Collections;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.Commands
{
	public class WmiBaseCmdlet : Cmdlet
	{
		private string[] computerName;

		private string nameSpace;

		internal bool namespaceSpecified;

		internal bool serverNameSpecified;

		private PSCredential credential;

		private ImpersonationLevel impersonationLevel;

		private AuthenticationLevel authenticationLevel;

		private string locale;

		private SwitchParameter enableAllPrivileges;

		private string authority;

		private SwitchParameter async;

		private int throttleLimit;

		private static int DEFAULT_THROTTLE_LIMIT;

		private ExecutionContext _context;

		[Parameter]
		public SwitchParameter AsJob
		{
			get
			{
				return this.async;
			}
			set
			{
				this.async = value;
			}
		}

		[Parameter(ParameterSetName="list")]
		[Parameter(ParameterSetName="class")]
		[Parameter(ParameterSetName="query")]
		[Parameter(ParameterSetName="path")]
		[Parameter(ParameterSetName="WQLQuery")]
		public AuthenticationLevel Authentication
		{
			get
			{
				return this.authenticationLevel;
			}
			set
			{
				this.authenticationLevel = value;
			}
		}

		[Parameter(ParameterSetName="WQLQuery")]
		[Parameter(ParameterSetName="list")]
		[Parameter(ParameterSetName="path")]
		[Parameter(ParameterSetName="class")]
		[Parameter(ParameterSetName="query")]
		public string Authority
		{
			get
			{
				return this.authority;
			}
			set
			{
				this.authority = value;
			}
		}

		[Alias(new string[] { "Cn" })]
		[Parameter(ParameterSetName="path")]
		[Parameter(ParameterSetName="query")]
		[Parameter(ParameterSetName="list")]
		[Parameter(ParameterSetName="class")]
		[Parameter(ParameterSetName="WQLQuery")]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return this.computerName;
			}
			set
			{
				this.computerName = value;
				this.serverNameSpecified = true;
			}
		}

		[Credential]
		[Parameter(ParameterSetName="class")]
		[Parameter(ParameterSetName="path")]
		[Parameter(ParameterSetName="query")]
		[Parameter(ParameterSetName="WQLQuery")]
		[Parameter(ParameterSetName="list")]
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

		[Parameter(ParameterSetName="class")]
		[Parameter(ParameterSetName="WQLQuery")]
		[Parameter(ParameterSetName="query")]
		[Parameter(ParameterSetName="path")]
		[Parameter(ParameterSetName="list")]
		public SwitchParameter EnableAllPrivileges
		{
			get
			{
				return this.enableAllPrivileges;
			}
			set
			{
				this.enableAllPrivileges = value;
			}
		}

		[Parameter(ParameterSetName="list")]
		[Parameter(ParameterSetName="path")]
		[Parameter(ParameterSetName="class")]
		[Parameter(ParameterSetName="WQLQuery")]
		[Parameter(ParameterSetName="query")]
		public ImpersonationLevel Impersonation
		{
			get
			{
				return this.impersonationLevel;
			}
			set
			{
				this.impersonationLevel = value;
			}
		}

		[Parameter(ParameterSetName="class")]
		[Parameter(ParameterSetName="query")]
		[Parameter(ParameterSetName="path")]
		[Parameter(ParameterSetName="WQLQuery")]
		[Parameter(ParameterSetName="list")]
		public string Locale
		{
			get
			{
				return this.locale;
			}
			set
			{
				this.locale = value;
			}
		}

		[Alias(new string[] { "NS" })]
		[Parameter(ParameterSetName="query")]
		[Parameter(ParameterSetName="path")]
		[Parameter(ParameterSetName="class")]
		[Parameter(ParameterSetName="WQLQuery")]
		[Parameter(ParameterSetName="list")]
		public string Namespace
		{
			get
			{
				return this.nameSpace;
			}
			set
			{
				this.nameSpace = value;
				this.namespaceSpecified = true;
			}
		}

		[Parameter]
		public int ThrottleLimit
		{
			get
			{
				return this.throttleLimit;
			}
			set
			{
				this.throttleLimit = value;
			}
		}

		static WmiBaseCmdlet()
		{
			WmiBaseCmdlet.DEFAULT_THROTTLE_LIMIT = 32;
		}

		public WmiBaseCmdlet()
		{
			string[] strArrays = new string[1];
			strArrays[0] = "localhost";
			this.computerName = strArrays;
			this.nameSpace = "root\\cimv2";
			this.impersonationLevel = ImpersonationLevel.Impersonate;
			this.authenticationLevel = AuthenticationLevel.Packet;
			this.async = false;
			this.throttleLimit = WmiBaseCmdlet.DEFAULT_THROTTLE_LIMIT;
			this._context = LocalPipeline.GetExecutionContextFromTLS();
		}

		internal ConnectionOptions GetConnectionOption ()
		{
			ConnectionOptions connectionOption = null;
			try {
				connectionOption = new ConnectionOptions ();
				connectionOption.Authentication = this.Authentication;
				connectionOption.Locale = this.Locale;
				connectionOption.Authority = this.Authority;
				connectionOption.EnablePrivileges = this.EnableAllPrivileges;
				connectionOption.Impersonation = this.Impersonation;
				if (this.Credential != null && (this.Credential.UserName != null || this.Credential.Password != null)) {
					connectionOption.Username = this.Credential.UserName;
					//connectionOption.SecurePassword = this.Credential.Password;
				}
			} catch (Exception ex) {
				Console.Write (ex.Message);
			}
			return connectionOption;
		}

		internal void RunAsJob(string cmdletName)
		{
			PSWmiJob pSWmiJob = new PSWmiJob(this, this.ComputerName, this.ThrottleLimit, Job.GetCommandTextFromInvocationInfo(base.MyInvocation));
			if (this._context != null)
			{
				((LocalRunspace)this._context.CurrentRunspace).JobRepository.Add(pSWmiJob);
			}
			base.WriteObject(pSWmiJob);
		}

		internal ManagementPath SetWmiInstanceBuildManagementPath()
		{
			ManagementPath managementPath = null;
			if (base.GetType() == typeof(SetWmiInstance))
			{
				SetWmiInstance setWmiInstance = (SetWmiInstance)this;
				if (setWmiInstance.Class == null)
				{
					managementPath = new ManagementPath(setWmiInstance.Path);
					if (!string.IsNullOrEmpty(managementPath.NamespacePath))
					{
						if (setWmiInstance.namespaceSpecified)
						{
							base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NamespaceSpecifiedWithPath", ErrorCategory.InvalidOperation, setWmiInstance.Namespace));
						}
					}
					else
					{
						managementPath.NamespacePath = setWmiInstance.Namespace;
					}
					if (managementPath.Server != "." && setWmiInstance.serverNameSpecified)
					{
						base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "ComputerNameSpecifiedWithPath", ErrorCategory.InvalidOperation, setWmiInstance.ComputerName));
					}
					if (!managementPath.IsClass)
					{
						if (!setWmiInstance.flagSpecified)
						{
							setWmiInstance.PutType = PutType.UpdateOrCreate;
						}
						else
						{
							if (setWmiInstance.PutType != PutType.UpdateOnly && setWmiInstance.PutType != PutType.UpdateOrCreate)
							{
								base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NonUpdateFlagSpecifiedWithInstancePath", ErrorCategory.InvalidOperation, (object)setWmiInstance.PutType));
							}
						}
					}
					else
					{
						if (setWmiInstance.flagSpecified && setWmiInstance.PutType != PutType.CreateOnly)
						{
							base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "CreateOnlyFlagNotSpecifiedWithClassPath", ErrorCategory.InvalidOperation, (object)setWmiInstance.PutType));
						}
						setWmiInstance.PutType = PutType.CreateOnly;
					}
				}
				else
				{
					if (setWmiInstance.flagSpecified && setWmiInstance.PutType != PutType.CreateOnly)
					{
						base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "CreateOnlyFlagNotSpecifiedWithClassPath", ErrorCategory.InvalidOperation, (object)setWmiInstance.PutType));
					}
					setWmiInstance.PutType = PutType.CreateOnly;
				}
			}
			return managementPath;
		}

		internal ManagementObject SetWmiInstanceGetObject(ManagementPath mPath, string serverName)
		{
			ConnectionOptions connectionOption = this.GetConnectionOption();
			ManagementObject value = null;
			if (base.GetType() == typeof(SetWmiInstance))
			{
				SetWmiInstance setWmiInstance = (SetWmiInstance)this;
				if (setWmiInstance.Path == null)
				{
					ManagementScope managementScope = new ManagementScope(WMIHelper.GetScopeString(serverName, setWmiInstance.Namespace), connectionOption);
					ManagementClass managementClass = new ManagementClass(setWmiInstance.Class);
					managementClass.Scope = managementScope;
					value = managementClass.CreateInstance();
				}
				else
				{
					mPath.Server = serverName;
					ManagementScope managementScope1 = new ManagementScope(mPath, connectionOption);
					if (!mPath.IsClass)
					{
						ManagementObject managementObject = new ManagementObject(mPath);
						managementObject.Scope = managementScope1;
						try
						{
							managementObject.Get();
						}
						catch (ManagementException managementException1)
						{
							ManagementException managementException = managementException1;
							if (managementException.ErrorCode == ManagementStatus.NotFound)
							{
								int num = setWmiInstance.Path.IndexOf(':');
								if (num != -1)
								{
									int num1 = setWmiInstance.Path.Substring(num).IndexOf('.');
									if (num1 != -1)
									{
										string str = setWmiInstance.Path.Substring(0, num1 + num);
										ManagementPath managementPath = new ManagementPath(str);
										ManagementClass managementClass1 = new ManagementClass(managementPath);
										managementClass1.Scope = managementScope1;
										managementObject = managementClass1.CreateInstance();
									}
									else
									{
										throw;
									}
								}
								else
								{
									throw;
								}
							}
							else
							{
								throw;
							}
						}
						value = managementObject;
					}
					else
					{
						ManagementClass managementClass2 = new ManagementClass(mPath);
						managementClass2.Scope = managementScope1;
						value = managementClass2.CreateInstance();
					}
				}
				if (setWmiInstance.Arguments != null)
				{
					IDictionaryEnumerator enumerator = setWmiInstance.Arguments.GetEnumerator();
					while (enumerator.MoveNext())
					{
						value[enumerator.Key as string] = enumerator.Value;
					}
				}
			}
			return value;
		}

		internal ManagementObject SetWmiInstanceGetPipelineObject()
		{
			ManagementObject value = null;
			if (base.GetType() == typeof(SetWmiInstance))
			{
				SetWmiInstance setWmiInstance = (SetWmiInstance)this;
				if (setWmiInstance.InputObject != null)
				{
					if (setWmiInstance.InputObject.GetType() != typeof(ManagementClass))
					{
						if (!setWmiInstance.flagSpecified)
						{
							setWmiInstance.PutType = PutType.UpdateOrCreate;
						}
						else
						{
							if (setWmiInstance.PutType != PutType.UpdateOnly && setWmiInstance.PutType != PutType.UpdateOrCreate)
							{
								base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NonUpdateFlagSpecifiedWithInstancePath", ErrorCategory.InvalidOperation, (object)setWmiInstance.PutType));
							}
						}
						value = (ManagementObject)setWmiInstance.InputObject.Clone();
					}
					else
					{
						if (setWmiInstance.flagSpecified && setWmiInstance.PutType != PutType.CreateOnly)
						{
							base.ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "CreateOnlyFlagNotSpecifiedWithClassPath", ErrorCategory.InvalidOperation, (object)setWmiInstance.PutType));
						}
						value = ((ManagementClass)setWmiInstance.InputObject).CreateInstance();
						setWmiInstance.PutType = PutType.CreateOnly;
					}
					if (setWmiInstance.Arguments != null)
					{
						IDictionaryEnumerator enumerator = setWmiInstance.Arguments.GetEnumerator();
						while (enumerator.MoveNext())
						{
							value[enumerator.Key as string] = enumerator.Value;
						}
					}
				}
			}
			return value;
		}
	}
}