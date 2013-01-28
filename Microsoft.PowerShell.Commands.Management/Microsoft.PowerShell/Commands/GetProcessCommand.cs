using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "Process", DefaultParameterSetName="Name", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113324", RemotingCapability=RemotingCapability.SupportedByCommand)]
	[OutputType(new Type[] { typeof(ProcessModule), typeof(FileVersionInfo), typeof(Process) })]
	public sealed class GetProcessCommand : ProcessBaseCommand
	{
		private SwitchParameter _module;

		private SwitchParameter _fileversioninfo;

		[Alias(new string[] { "Cn" })]
		[Parameter(Mandatory=false, ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
		public string[] ComputerName
		{
			get
			{
				return base.SuppliedComputerName;
			}
			set
			{
				base.SuppliedComputerName = value;
			}
		}

		[Alias(new string[] { "FV", "FVI" })]
		[Parameter]
		[ValidateNotNull]
		public SwitchParameter FileVersionInfo
		{
			get
			{
				return this._fileversioninfo;
			}
			set
			{
				this._fileversioninfo = value;
			}
		}

		[Alias(new string[] { "PID" })]
		[Parameter(ParameterSetName="Id", Mandatory=true, ValueFromPipelineByPropertyName=true)]
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

		[Parameter]
		[ValidateNotNull]
		public SwitchParameter Module
		{
			get
			{
				return this._module;
			}
			set
			{
				this._module = value;
			}
		}

		[Alias(new string[] { "ProcessName" })]
		[Parameter(Position=0, ParameterSetName="Name", ValueFromPipelineByPropertyName=true)]
		[ValidateNotNullOrEmpty]
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

		public GetProcessCommand()
		{
		}

		protected override void ProcessRecord()
		{
			if ((int)this.ComputerName.Length > 0 && (this._fileversioninfo.IsPresent || this._module.IsPresent))
			{
				Exception invalidOperationException = new InvalidOperationException(ProcessResources.NoComputerNameWithFileVersion);
				ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, "InvalidOperationException", ErrorCategory.InvalidOperation, this.ComputerName);
				base.ThrowTerminatingError(errorRecord);
			}
			foreach (Process process in base.MatchingProcesses())
			{
				if (!this._module.IsPresent || !this._fileversioninfo.IsPresent)
				{
					if (!this._module.IsPresent)
					{
						if (!this._fileversioninfo.IsPresent)
						{
							base.WriteObject(process);
						}
						else
						{
							try
							{
								base.WriteObject(PsUtils.GetMainModule(process).FileVersionInfo, true);
							}
							catch (InvalidOperationException invalidOperationException2)
							{
								InvalidOperationException invalidOperationException1 = invalidOperationException2;
								base.WriteNonTerminatingError(process, invalidOperationException1, ProcessResources.CouldnotEnumerateFileVer, "CouldnotEnumerateFileVer", ErrorCategory.PermissionDenied);
							}
							catch (ArgumentException argumentException1)
							{
								ArgumentException argumentException = argumentException1;
								base.WriteNonTerminatingError(process, argumentException, ProcessResources.CouldnotEnumerateFileVer, "CouldnotEnumerateFileVer", ErrorCategory.PermissionDenied);
							}
							catch (Win32Exception win32Exception3)
							{
								Win32Exception win32Exception = win32Exception3;
								try
								{
									if (win32Exception.ErrorCode != 0x12b)
									{
										base.WriteNonTerminatingError(process, win32Exception, ProcessResources.CouldnotEnumerateFileVer, "CouldnotEnumerateFileVer", ErrorCategory.PermissionDenied);
									}
									else
									{
										base.WriteObject(PsUtils.GetMainModule(process).FileVersionInfo, true);
									}
								}
								catch (Win32Exception win32Exception2)
								{
									Win32Exception win32Exception1 = win32Exception2;
									base.WriteNonTerminatingError(process, win32Exception1, ProcessResources.CouldnotEnumerateFileVer, "CouldnotEnumerateFileVer", ErrorCategory.PermissionDenied);
								}
							}
							catch (Exception exception1)
							{
								Exception exception = exception1;
								CommandsCommon.CheckForSevereException(this, exception);
								base.WriteNonTerminatingError(process, exception, ProcessResources.CouldnotEnumerateFileVer, "CouldnotEnumerateFileVer", ErrorCategory.PermissionDenied);
							}
						}
					}
					else
					{
						try
						{
							base.WriteObject(process.Modules, true);
						}
						catch (Win32Exception win32Exception7)
						{
							Win32Exception win32Exception4 = win32Exception7;
							try
							{
								if (win32Exception4.ErrorCode != 0x12b)
								{
									base.WriteNonTerminatingError(process, win32Exception4, ProcessResources.CouldnotEnumerateModules, "CouldnotEnumerateModules", ErrorCategory.PermissionDenied);
								}
								else
								{
									base.WriteObject(process.Modules, true);
								}
							}
							catch (Win32Exception win32Exception6)
							{
								Win32Exception win32Exception5 = win32Exception6;
								base.WriteNonTerminatingError(process, win32Exception5, ProcessResources.CouldnotEnumerateModules, "CouldnotEnumerateModules", ErrorCategory.PermissionDenied);
							}
						}
						catch (Exception exception3)
						{
							Exception exception2 = exception3;
							CommandsCommon.CheckForSevereException(this, exception2);
							base.WriteNonTerminatingError(process, exception2, ProcessResources.CouldnotEnumerateModules, "CouldnotEnumerateModules", ErrorCategory.PermissionDenied);
						}
					}
				}
				else
				{
					ProcessModule processModule = null;
					try
					{
						ProcessModuleCollection modules = process.Modules;
						foreach (ProcessModule module in modules)
						{
							processModule = module;
							base.WriteObject(module.FileVersionInfo, true);
						}
					}
					catch (InvalidOperationException invalidOperationException4)
					{
						InvalidOperationException invalidOperationException3 = invalidOperationException4;
						base.WriteNonTerminatingError(process, invalidOperationException3, ProcessResources.CouldnotEnumerateModuleFileVer, "CouldnotEnumerateModuleFileVer", ErrorCategory.PermissionDenied);
					}
					catch (ArgumentException argumentException3)
					{
						ArgumentException argumentException2 = argumentException3;
						base.WriteNonTerminatingError(process, argumentException2, ProcessResources.CouldnotEnumerateModuleFileVer, "CouldnotEnumerateModuleFileVer", ErrorCategory.PermissionDenied);
					}
					catch (Win32Exception win32Exception11)
					{
						Win32Exception win32Exception8 = win32Exception11;
						try
						{
							if (win32Exception8.ErrorCode != 0x12b)
							{
								base.WriteNonTerminatingError(process, win32Exception8, ProcessResources.CouldnotEnumerateModuleFileVer, "CouldnotEnumerateModuleFileVer", ErrorCategory.PermissionDenied);
							}
							else
							{
								base.WriteObject(processModule.FileVersionInfo, true);
							}
						}
						catch (Win32Exception win32Exception10)
						{
							Win32Exception win32Exception9 = win32Exception10;
							base.WriteNonTerminatingError(process, win32Exception9, ProcessResources.CouldnotEnumerateModuleFileVer, "CouldnotEnumerateModuleFileVer", ErrorCategory.PermissionDenied);
						}
					}
					catch (Exception exception5)
					{
						Exception exception4 = exception5;
						CommandsCommon.CheckForSevereException(this, exception4);
						base.WriteNonTerminatingError(process, exception4, ProcessResources.CouldnotEnumerateModuleFileVer, "CouldnotEnumerateModuleFileVer", ErrorCategory.PermissionDenied);
					}
				}
			}
		}
	}
}