using Microsoft.PowerShell;
using Microsoft.PowerShell.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Security.AccessControl;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "Acl", SupportsTransactions=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113305")]
	public sealed class GetAclCommand : SecurityDescriptorCommandsBase
	{
		private string[] path;

		private PSObject inputObject;

		private bool isLiteralPath;

		private SwitchParameter audit;

		private SwitchParameter allCentralAccessPolicies;

		[Parameter]
		public SwitchParameter AllCentralAccessPolicies
		{
			get
			{
				return this.allCentralAccessPolicies;
			}
			set
			{
				this.allCentralAccessPolicies = value;
			}
		}

		[Parameter]
		public SwitchParameter Audit
		{
			get
			{
				return this.audit;
			}
			set
			{
				this.audit = value;
			}
		}

		[Parameter(Mandatory=true, ParameterSetName="ByInputObject")]
		public PSObject InputObject
		{
			get
			{
				return this.inputObject;
			}
			set
			{
				this.inputObject = value;
			}
		}

		[Alias(new string[] { "PSPath" })]
		[Parameter(ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath")]
		[ValidateNotNullOrEmpty]
		public string[] LiteralPath
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
				this.isLiteralPath = true;
			}
		}

		[Parameter(Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByPath")]
		[ValidateNotNullOrEmpty]
		public string[] Path
		{
			get
			{
				return this.path;
			}
			set
			{
				this.path = value;
			}
		}

		public GetAclCommand()
		{
			string[] strArrays = new string[1];
			strArrays[0] = ".";
			this.path = strArrays;
		}

		protected override void ProcessRecord()
		{
			AccessControlSections accessControlSection = AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group;
			if (this.audit)
			{
				accessControlSection = accessControlSection | AccessControlSections.Audit;
			}
			if (this.inputObject == null)
			{
				string[] path = this.Path;
				for (int i = 0; i < (int)path.Length; i++)
				{
					string str = path[i];
					List<string> strs = new List<string>();
					string str1 = null;
					try
					{
						if (!this.isLiteralPath)
						{
							Collection<PathInfo> resolvedPSPathFromPSPath = base.SessionState.Path.GetResolvedPSPathFromPSPath(str, base.CmdletProviderContext);
							foreach (PathInfo pathInfo in resolvedPSPathFromPSPath)
							{
								strs.Add(pathInfo.Path);
							}
						}
						else
						{
							strs.Add(base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(str));
						}
						foreach (string str2 in strs)
						{
							str1 = str2;
							CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(base.Context);
							cmdletProviderContext.SuppressWildcardExpansion = true;
							if (base.InvokeProvider.Item.Exists(str2, false, this.isLiteralPath))
							{
								base.InvokeProvider.SecurityDescriptor.Get(str2, accessControlSection, cmdletProviderContext);
								Collection<PSObject> accumulatedObjects = cmdletProviderContext.GetAccumulatedObjects();
								if (accumulatedObjects == null)
								{
									continue;
								}
								SecurityDescriptorCommandsBase.AddBrokeredProperties(accumulatedObjects, this.audit, this.allCentralAccessPolicies);
								base.WriteObject(accumulatedObjects, true);
							}
							else
							{
								ErrorRecord errorRecord = SecurityUtils.CreatePathNotFoundErrorRecord(str2, "GetAcl_PathNotFound");
								base.WriteError(errorRecord);
							}
						}
					}
					catch (NotSupportedException notSupportedException)
					{
						object[] objArray = new object[1];
						objArray[0] = str1;
						ErrorRecord errorRecord1 = SecurityUtils.CreateNotSupportedErrorRecord(UtilsStrings.OperationNotSupportedOnPath, "GetAcl_OperationNotSupported", objArray);
						base.WriteError(errorRecord1);
					}
					catch (ItemNotFoundException itemNotFoundException)
					{
						ErrorRecord errorRecord2 = SecurityUtils.CreatePathNotFoundErrorRecord(str, "GetAcl_PathNotFound_Exception");
						base.WriteError(errorRecord2);
					}
				}
			}
			else
			{
				PSMethodInfo item = this.inputObject.Methods["GetSecurityDescriptor"];
				if (item == null)
				{
					ErrorRecord errorRecord3 = SecurityUtils.CreateNotSupportedErrorRecord(UtilsStrings.GetMethodNotFound, "GetAcl_OperationNotSupported", new object[0]);
					base.WriteError(errorRecord3);
					return;
				}
				else
				{
					object commonSecurityDescriptor = null;
					try
					{
						commonSecurityDescriptor = PSObject.Base(item.Invoke(new object[0]));
						if (commonSecurityDescriptor as FileSystemSecurity == null)
						{
							commonSecurityDescriptor = new CommonSecurityDescriptor(false, false, commonSecurityDescriptor.ToString());
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						CommandProcessorBase.CheckForSevereException(exception);
						ErrorRecord errorRecord4 = SecurityUtils.CreateNotSupportedErrorRecord(UtilsStrings.MethodInvokeFail, "GetAcl_OperationNotSupported", new object[0]);
						base.WriteError(errorRecord4);
						return;
					}
					base.WriteObject(commonSecurityDescriptor, true);
					return;
				}
			}
		}
	}
}