using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Register", "ScheduledJob", SupportsShouldProcess=true, DefaultParameterSetName="ScriptBlock", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223922")]
	[OutputType(new Type[] { typeof(ScheduledJobDefinition) })]
	public sealed class RegisterScheduledJobCommand : ScheduleJobCmdletBase
	{
		private const string FilePathParameterSet = "FilePath";

		private const string ScriptBlockParameterSet = "ScriptBlock";

		private string _filePath;

		private ScriptBlock _scriptBlock;

		private string _name;

		private ScheduledJobTrigger[] _triggers;

		private ScriptBlock _initializationScript;

		private SwitchParameter _runAs32;

		private PSCredential _credential;

		private AuthenticationMechanism _authenticationMechanism;

		private ScheduledJobOptions _options;

		private object[] _arguments;

		private int _executionHistoryLength;

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		[ValidateNotNullOrEmpty]
		public object[] ArgumentList
		{
			get
			{
				return this._arguments;
			}
			set
			{
				this._arguments = value;
			}
		}

		[Parameter(ParameterSetName="FilePath")]
		[Parameter(ParameterSetName="ScriptBlock")]
		public AuthenticationMechanism Authentication
		{
			get
			{
				return this._authenticationMechanism;
			}
			set
			{
				this._authenticationMechanism = value;
			}
		}

		[Credential]
		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
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

		[Parameter(Position=1, Mandatory=true, ParameterSetName="FilePath")]
		[ValidateNotNullOrEmpty]
		public string FilePath
		{
			get
			{
				return this._filePath;
			}
			set
			{
				this._filePath = value;
			}
		}

		[Parameter(ParameterSetName="FilePath")]
		[Parameter(ParameterSetName="ScriptBlock")]
		[ValidateNotNull]
		public ScriptBlock InitializationScript
		{
			get
			{
				return this._initializationScript;
			}
			set
			{
				this._initializationScript = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		public int MaxResultCount
		{
			get
			{
				return this._executionHistoryLength;
			}
			set
			{
				this._executionHistoryLength = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="ScriptBlock")]
		[Parameter(Position=0, Mandatory=true, ParameterSetName="FilePath")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		public SwitchParameter RunAs32
		{
			get
			{
				return this._runAs32;
			}
			set
			{
				this._runAs32 = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		[ValidateNotNull]
		public ScheduledJobOptions ScheduledJobOption
		{
			get
			{
				return this._options;
			}
			set
			{
				this._options = value;
			}
		}

		[Parameter(Position=1, Mandatory=true, ParameterSetName="ScriptBlock")]
		[ValidateNotNull]
		public ScriptBlock ScriptBlock
		{
			get
			{
				return this._scriptBlock;
			}
			set
			{
				this._scriptBlock = value;
			}
		}

		[Parameter(ParameterSetName="FilePath")]
		[Parameter(ParameterSetName="ScriptBlock")]
		[ValidateNotNullOrEmpty]
		public ScheduledJobTrigger[] Trigger
		{
			get
			{
				return this._triggers;
			}
			set
			{
				this._triggers = value;
			}
		}

		public RegisterScheduledJobCommand()
		{
		}

		private Dictionary<string, object> CreateCommonParameters()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			SwitchParameter runAs32 = this.RunAs32;
			strs.Add("RunAs32", runAs32.ToBool());
			strs.Add("Authentication", this.Authentication);
			if (this.InitializationScript != null)
			{
				strs.Add("InitializationScript", this.InitializationScript);
			}
			if (this.ArgumentList != null)
			{
				strs.Add("ArgumentList", this.ArgumentList);
			}
			return strs;
		}

		private ScheduledJobDefinition CreateFilePathDefinition()
		{
			JobDefinition jobDefinition = new JobDefinition(typeof(ScheduledJobSourceAdapter), this.FilePath, this._name);
			jobDefinition.ModuleName = "PSScheduledJob";
			Dictionary<string, object> strs = this.CreateCommonParameters();
			if (this.FilePath.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
			{
				Collection<PathInfo> resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(this.FilePath);
				if (resolvedPSPathFromPSPath.Count == 1)
				{
					strs.Add("FilePath", resolvedPSPathFromPSPath[0].Path);
					JobInvocationInfo scheduledJobInvocationInfo = new ScheduledJobInvocationInfo(jobDefinition, strs);
					ScheduledJobDefinition scheduledJobDefinition = new ScheduledJobDefinition(scheduledJobInvocationInfo, this.Trigger, this.ScheduledJobOption, this._credential);
					return scheduledJobDefinition;
				}
				else
				{
					string str = StringUtil.Format(ScheduledJobErrorStrings.InvalidFilePath, new object[0]);
					Exception runtimeException = new RuntimeException(str);
					ErrorRecord errorRecord = new ErrorRecord(runtimeException, "InvalidFilePathParameterForRegisterScheduledJobDefinition", ErrorCategory.InvalidArgument, this);
					base.WriteError(errorRecord);
					return null;
				}
			}
			else
			{
				string str1 = StringUtil.Format(ScheduledJobErrorStrings.InvalidFilePathFile, new object[0]);
				Exception exception = new RuntimeException(str1);
				ErrorRecord errorRecord1 = new ErrorRecord(exception, "InvalidFilePathParameterForRegisterScheduledJobDefinition", ErrorCategory.InvalidArgument, this);
				base.WriteError(errorRecord1);
				return null;
			}
		}

		private ScheduledJobDefinition CreateScriptBlockDefinition()
		{
			JobDefinition jobDefinition = new JobDefinition(typeof(ScheduledJobSourceAdapter), this.ScriptBlock.ToString(), this._name);
			jobDefinition.ModuleName = "PSScheduledJob";
			Dictionary<string, object> strs = this.CreateCommonParameters();
			strs.Add("ScriptBlock", this.ScriptBlock);
			JobInvocationInfo scheduledJobInvocationInfo = new ScheduledJobInvocationInfo(jobDefinition, strs);
			ScheduledJobDefinition scheduledJobDefinition = new ScheduledJobDefinition(scheduledJobInvocationInfo, this.Trigger, this.ScheduledJobOption, this._credential);
			return scheduledJobDefinition;
		}

		protected override void ProcessRecord()
		{
			string message;
			string str = StringUtil.Format(ScheduledJobErrorStrings.DefinitionWhatIf, this.Name);
			if (base.ShouldProcess(str, "Register"))
			{
				ScheduledJobDefinition scheduledJobDefinition = null;
				string parameterSetName = base.ParameterSetName;
				string str1 = parameterSetName;
				if (parameterSetName != null)
				{
					if (str1 == "ScriptBlock")
					{
						scheduledJobDefinition = this.CreateScriptBlockDefinition();
					}
					else
					{
						if (str1 == "FilePath")
						{
							scheduledJobDefinition = this.CreateFilePathDefinition();
						}
					}
				}
				if (scheduledJobDefinition != null)
				{
					if (base.MyInvocation.BoundParameters.ContainsKey("MaxResultCount"))
					{
						if (this.MaxResultCount >= 1)
						{
							scheduledJobDefinition.SetExecutionHistoryLength(this.MaxResultCount, false);
						}
						else
						{
							string str2 = StringUtil.Format(ScheduledJobErrorStrings.InvalidMaxResultCount, new object[0]);
							Exception runtimeException = new RuntimeException(str2);
							ErrorRecord errorRecord = new ErrorRecord(runtimeException, "InvalidMaxResultCountParameterForRegisterScheduledJobDefinition", ErrorCategory.InvalidArgument, null);
							base.WriteError(errorRecord);
							return;
						}
					}
					try
					{
						scheduledJobDefinition.Register();
						base.WriteObject(scheduledJobDefinition);
					}
					catch (ScheduledJobException scheduledJobException1)
					{
						ScheduledJobException scheduledJobException = scheduledJobException1;
						if (scheduledJobException.InnerException == null || scheduledJobException.InnerException as UnauthorizedAccessException == null)
						{
							if (scheduledJobException.InnerException == null || scheduledJobException.InnerException as DirectoryNotFoundException == null)
							{
								if (scheduledJobException.InnerException == null || scheduledJobException.InnerException as InvalidDataContractException == null)
								{
									ErrorRecord errorRecord1 = new ErrorRecord(scheduledJobException, "CantRegisterScheduledJobDefinition", ErrorCategory.InvalidOperation, scheduledJobDefinition);
									base.WriteError(errorRecord1);
								}
								else
								{
									if (!string.IsNullOrEmpty(scheduledJobException.InnerException.Message))
									{
										message = scheduledJobException.InnerException.Message;
									}
									else
									{
										message = string.Empty;
									}
									string str3 = message;
									object[] name = new object[2];
									name[0] = scheduledJobDefinition.Name;
									name[1] = str3;
									string str4 = StringUtil.Format(ScheduledJobErrorStrings.CannotSerializeData, name);
									Exception exception = new RuntimeException(str4, scheduledJobException);
									ErrorRecord errorRecord2 = new ErrorRecord(exception, "CannotSerializeDataWhenRegisteringScheduledJobDefinition", ErrorCategory.InvalidData, scheduledJobDefinition);
									base.WriteError(errorRecord2);
								}
							}
							else
							{
								string str5 = StringUtil.Format(ScheduledJobErrorStrings.DirectoryNotFoundError, scheduledJobDefinition.Name);
								Exception runtimeException1 = new RuntimeException(str5, scheduledJobException);
								ErrorRecord errorRecord3 = new ErrorRecord(runtimeException1, "DirectoryNotFoundWhenRegisteringScheduledJobDefinition", ErrorCategory.ObjectNotFound, scheduledJobDefinition);
								base.WriteError(errorRecord3);
							}
						}
						else
						{
							string str6 = StringUtil.Format(ScheduledJobErrorStrings.UnauthorizedAccessError, scheduledJobDefinition.Name);
							Exception exception1 = new RuntimeException(str6, scheduledJobException);
							ErrorRecord errorRecord4 = new ErrorRecord(exception1, "UnauthorizedAccessToRegisterScheduledJobDefinition", ErrorCategory.PermissionDenied, scheduledJobDefinition);
							base.WriteError(errorRecord4);
						}
					}
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}