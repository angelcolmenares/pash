using Microsoft.PowerShell;
using Microsoft.PowerShell.Security;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Set", "Acl", SupportsShouldProcess=true, SupportsTransactions=true, DefaultParameterSetName="ByPath", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113389")]
	public sealed class SetAclCommand : SecurityDescriptorCommandsBase
	{
		private string[] path;

		private PSObject inputObject;

		private bool isLiteralPath;

		private object securityDescriptor;

		private string centralAccessPolicy;

		private SwitchParameter clearCentralAccessPolicy;

		private SwitchParameter passthru;

		[Parameter(Position=1, Mandatory=true, ValueFromPipeline=true, ParameterSetName="ByPath")]
		[Parameter(Position=1, Mandatory=true, ValueFromPipeline=true, ParameterSetName="ByLiteralPath")]
		[Parameter(Position=1, Mandatory=true, ValueFromPipeline=true, ParameterSetName="ByInputObject")]
		public object AclObject
		{
			get
			{
				return this.securityDescriptor;
			}
			set
			{
				this.securityDescriptor = PSObject.Base(value);
			}
		}

		[Parameter(Position=2, Mandatory=false, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath")]
		[Parameter(Position=2, Mandatory=false, ValueFromPipelineByPropertyName=true, ParameterSetName="ByPath")]
		public string CentralAccessPolicy
		{
			get
			{
				return this.centralAccessPolicy;
			}
			set
			{
				this.centralAccessPolicy = value;
			}
		}

		[Parameter(Mandatory=false, ParameterSetName="ByLiteralPath")]
		[Parameter(Mandatory=false, ParameterSetName="ByPath")]
		public SwitchParameter ClearCentralAccessPolicy
		{
			get
			{
				return this.clearCentralAccessPolicy;
			}
			set
			{
				this.clearCentralAccessPolicy = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByInputObject")]
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
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByLiteralPath")]
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

		[Parameter]
		public SwitchParameter Passthru
		{
			get
			{
				return this.passthru;
			}
			set
			{
				this.passthru = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ByPath")]
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

		public SetAclCommand()
		{
		}

		private IntPtr GetEmptySacl()
		{
			IntPtr zero = IntPtr.Zero;
			bool flag = true;
			try
			{
				NativeMethods.ACL aCL = new NativeMethods.ACL();
				int num = Marshal.SizeOf(aCL) + Marshal.SizeOf((int)0) - 1 & -4;
				zero = Marshal.AllocHGlobal(num);
				flag = NativeMethods.InitializeAcl(zero, num, 2);
				if (!flag)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally
			{
				if (!flag)
				{
					Marshal.FreeHGlobal(zero);
					zero = IntPtr.Zero;
				}
			}
			return zero;
		}

		private IntPtr GetSaclWithCapId(string capStr)
		{
			IntPtr zero;
			IntPtr cAPID = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			bool sid = true;
			bool flag = true;
			int num = 0;
			try
			{
				sid = NativeMethods.ConvertStringSidToSid(capStr, out cAPID);
				if (!sid)
				{
					flag = false;
					int num1 = 0;
					num = NativeMethods.LsaQueryCAPs(null, 0, out zero1, out num1);
					if (num == 0)
					{
						if (num1 == 0 || zero1 == IntPtr.Zero)
						{
							zero = IntPtr.Zero;
							return zero;
						}
						else
						{
							IntPtr intPtr1 = zero1;
							uint num2 = 0;
							while (num2 < num1)
							{
								NativeMethods.CENTRAL_ACCESS_POLICY structure = (NativeMethods.CENTRAL_ACCESS_POLICY)Marshal.PtrToStructure(intPtr1, typeof(NativeMethods.CENTRAL_ACCESS_POLICY));
								string stringUni = Marshal.PtrToStringUni(structure.Name.Buffer, structure.Name.Length / 2);
								if (!stringUni.Equals(capStr, StringComparison.OrdinalIgnoreCase))
								{
									intPtr1 = intPtr1 + Marshal.SizeOf(structure);
									num2++;
								}
								else
								{
									cAPID = structure.CAPID;
									break;
								}
							}
						}
					}
					else
					{
						throw new Win32Exception(num);
					}
				}
				if (cAPID != IntPtr.Zero)
				{
					sid = NativeMethods.IsValidSid(cAPID);
					if (sid)
					{
						uint lengthSid = NativeMethods.GetLengthSid(cAPID);
						NativeMethods.ACL aCL = new NativeMethods.ACL();
						NativeMethods.SYSTEM_AUDIT_ACE sYSTEMAUDITACE = new NativeMethods.SYSTEM_AUDIT_ACE();
						int num3 = (int)((long)(Marshal.SizeOf(aCL) + Marshal.SizeOf(sYSTEMAUDITACE)) + (long)lengthSid - (long)1) & -4;
						intPtr = Marshal.AllocHGlobal(num3);
						sid = NativeMethods.InitializeAcl(intPtr, num3, 2);
						if (sid)
						{
							num = NativeMethods.RtlAddScopedPolicyIDAce(intPtr, 2, 3, 0, cAPID);
							if (num == 0)
							{
								return intPtr;
							}
							else
							{
								if (num != -1073741811)
								{
									throw new Win32Exception(num);
								}
								else
								{
									throw new ArgumentException(UtilsStrings.InvalidCentralAccessPolicyIdentifier);
								}
							}
						}
						else
						{
							throw new Win32Exception(Marshal.GetLastWin32Error());
						}
					}
					else
					{
						throw new Win32Exception(Marshal.GetLastWin32Error());
					}
				}
				else
				{
					Exception argumentException = new ArgumentException(UtilsStrings.InvalidCentralAccessPolicyIdentifier);
					base.WriteError(new ErrorRecord(argumentException, "SetAcl_CentralAccessPolicy", ErrorCategory.InvalidArgument, this.AclObject));
					zero = IntPtr.Zero;
				}
			}
			finally
			{
				if (!sid || num != 0)
				{
					Marshal.FreeHGlobal(intPtr);
				}
				num = NativeMethods.LsaFreeMemory(zero1);
				if (flag)
				{
					NativeMethods.LocalFree(cAPID);
				}
			}
			return zero;
		}

		private IntPtr GetTokenWithEnabledPrivilege(string privilege, NativeMethods.TOKEN_PRIVILEGE previousState)
		{
			IntPtr zero = IntPtr.Zero;
			bool flag = true;
			try
			{
				flag = NativeMethods.OpenThreadToken(NativeMethods.GetCurrentThread(), 40, true, out zero);
				if (!flag)
				{
					if ((long)Marshal.GetLastWin32Error() == (long)0x3f0)
					{
						flag = NativeMethods.OpenProcessToken(NativeMethods.GetCurrentProcess(), 40, out zero);
					}
					if (!flag)
					{
						throw new Win32Exception(Marshal.GetLastWin32Error());
					}
				}
				NativeMethods.LUID lUID = new NativeMethods.LUID();
				flag = NativeMethods.LookupPrivilegeValue(null, privilege, ref lUID);
				if (flag)
				{
					NativeMethods.TOKEN_PRIVILEGE tOKENPRIVILEGE = new NativeMethods.TOKEN_PRIVILEGE();
					tOKENPRIVILEGE.PrivilegeCount = 1;
					tOKENPRIVILEGE.Privilege.Attributes = 2;
					tOKENPRIVILEGE.Privilege.Luid = lUID;
					uint num = 0;
					flag = NativeMethods.AdjustTokenPrivileges(zero, false, ref tOKENPRIVILEGE, Marshal.SizeOf(previousState), ref previousState, ref num);
					if (!flag)
					{
						throw new Win32Exception(Marshal.GetLastWin32Error());
					}
				}
				else
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally
			{
				if (!flag)
				{
					NativeMethods.CloseHandle(zero);
					zero = IntPtr.Zero;
				}
			}
			return zero;
		}

		protected override void ProcessRecord()
		{
			string sddlForm;
			ObjectSecurity objectSecurity = this.securityDescriptor as ObjectSecurity;
			if (this.inputObject == null)
			{
				if (this.Path != null)
				{
					if (objectSecurity != null)
					{
						if ((this.CentralAccessPolicy != null || this.ClearCentralAccessPolicy) && !DownLevelHelper.IsWin8AndAbove())
						{
							Exception parameterBindingException = new ParameterBindingException();
							base.WriteError(new ErrorRecord(parameterBindingException, "SetAcl_OperationNotSupported", ErrorCategory.InvalidArgument, null));
							return;
						}
						else
						{
							if (this.CentralAccessPolicy == null || !this.ClearCentralAccessPolicy)
							{
								IntPtr zero = IntPtr.Zero;
								NativeMethods.TOKEN_PRIVILEGE tOKENPRIVILEGE = new NativeMethods.TOKEN_PRIVILEGE();
								try
								{
									if (this.CentralAccessPolicy == null)
									{
										if (this.ClearCentralAccessPolicy)
										{
											zero = this.GetEmptySacl();
											if (zero == IntPtr.Zero)
											{
												SystemException systemException = new SystemException(UtilsStrings.GetEmptySaclFail);
												base.WriteError(new ErrorRecord(systemException, "SetAcl_ClearCentralAccessPolicy", ErrorCategory.InvalidResult, null));
												return;
											}
										}
									}
									else
									{
										zero = this.GetSaclWithCapId(this.CentralAccessPolicy);
										if (zero == IntPtr.Zero)
										{
											SystemException systemException1 = new SystemException(UtilsStrings.GetSaclWithCapIdFail);
											base.WriteError(new ErrorRecord(systemException1, "SetAcl_CentralAccessPolicy", ErrorCategory.InvalidResult, null));
											return;
										}
									}
									string[] path = this.Path;
									for (int i = 0; i < (int)path.Length; i++)
									{
										string str = path[i];
										Collection<PathInfo> pathInfos = new Collection<PathInfo>();
										CmdletProviderContext cmdletProviderContext = base.CmdletProviderContext;
										cmdletProviderContext.PassThru = this.Passthru;
										if (!this.isLiteralPath)
										{
											pathInfos = base.SessionState.Path.GetResolvedPSPathFromPSPath(str, base.CmdletProviderContext);
										}
										else
										{
											ProviderInfo providerInfo = null;
											PSDriveInfo pSDriveInfo = null;
											string unresolvedProviderPathFromPSPath = base.SessionState.Path.GetUnresolvedProviderPathFromPSPath(str, out providerInfo, out pSDriveInfo);
											pathInfos.Add(new PathInfo(pSDriveInfo, providerInfo, unresolvedProviderPathFromPSPath, base.SessionState));
											cmdletProviderContext.SuppressWildcardExpansion = true;
										}
										foreach (PathInfo pathInfo in pathInfos)
										{
											if (!base.ShouldProcess(pathInfo.Path))
											{
												continue;
											}
											try
											{
												base.InvokeProvider.SecurityDescriptor.Set(pathInfo.Path, objectSecurity, cmdletProviderContext);
												if (this.CentralAccessPolicy != null || this.ClearCentralAccessPolicy)
												{
													if (pathInfo.Provider.NameEquals(base.Context.ProviderNames.FileSystem))
													{
														IntPtr tokenWithEnabledPrivilege = this.GetTokenWithEnabledPrivilege("SeSecurityPrivilege", tOKENPRIVILEGE);
														if (tokenWithEnabledPrivilege != IntPtr.Zero)
														{
															int num = NativeMethods.SetNamedSecurityInfo(pathInfo.ProviderPath, NativeMethods.SeObjectType.SE_FILE_OBJECT, NativeMethods.SecurityInformation.SCOPE_SECURITY_INFORMATION, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, zero);
															if (tokenWithEnabledPrivilege != IntPtr.Zero)
															{
																NativeMethods.TOKEN_PRIVILEGE tOKENPRIVILEGE1 = new NativeMethods.TOKEN_PRIVILEGE();
																uint num1 = 0;
																NativeMethods.AdjustTokenPrivileges(tokenWithEnabledPrivilege, false, ref tOKENPRIVILEGE, Marshal.SizeOf(tOKENPRIVILEGE1), ref tOKENPRIVILEGE1, ref num1);
																NativeMethods.CloseHandle(tokenWithEnabledPrivilege);
															}
															if (num != 0)
															{
																SystemException win32Exception = new Win32Exception(num, UtilsStrings.SetCentralAccessPolicyFail);
																base.WriteError(new ErrorRecord(win32Exception, "SetAcl_SetNamedSecurityInfo", ErrorCategory.InvalidResult, null));
															}
														}
														else
														{
															SystemException systemException2 = new SystemException(UtilsStrings.GetTokenWithEnabledPrivilegeFail);
															base.WriteError(new ErrorRecord(systemException2, "SetAcl_AdjustTokenPrivileges", ErrorCategory.InvalidResult, null));
															return;
														}
													}
													else
													{
														Exception argumentException = new ArgumentException("Path");
														base.WriteError(new ErrorRecord(argumentException, "SetAcl_Path", ErrorCategory.InvalidArgument, this.AclObject));
														continue;
													}
												}
											}
											catch (NotSupportedException notSupportedException)
											{
												object[] objArray = new object[1];
												objArray[0] = pathInfo.Path;
												ErrorRecord errorRecord = SecurityUtils.CreateNotSupportedErrorRecord(UtilsStrings.OperationNotSupportedOnPath, "SetAcl_OperationNotSupported", objArray);
												base.WriteError(errorRecord);
											}
										}
									}
									return;
								}
								finally
								{
									Marshal.FreeHGlobal(zero);
								}
							}
							else
							{
								Exception exception = new ArgumentException(UtilsStrings.InvalidCentralAccessPolicyParameters);
								ErrorRecord errorRecord1 = SecurityUtils.CreateInvalidArgumentErrorRecord(exception, "SetAcl_OperationNotSupported");
								base.WriteError(errorRecord1);
								return;
							}
						}
					}
					else
					{
						Exception argumentException1 = new ArgumentException("AclObject");
						base.WriteError(new ErrorRecord(argumentException1, "SetAcl_AclObject", ErrorCategory.InvalidArgument, this.AclObject));
						return;
					}
				}
				else
				{
					Exception exception1 = new ArgumentException("Path");
					base.WriteError(new ErrorRecord(exception1, "SetAcl_Path", ErrorCategory.InvalidArgument, this.AclObject));
				}
			}
			else
			{
				PSMethodInfo item = this.inputObject.Methods["SetSecurityDescriptor"];
				if (item == null)
				{
					ErrorRecord errorRecord2 = SecurityUtils.CreateNotSupportedErrorRecord(UtilsStrings.SetMethodNotFound, "SetAcl_OperationNotSupported", new object[0]);
					base.WriteError(errorRecord2);
					return;
				}
				else
				{
					CommonSecurityDescriptor commonSecurityDescriptor = this.securityDescriptor as CommonSecurityDescriptor;
					if (objectSecurity == null)
					{
						if (commonSecurityDescriptor == null)
						{
							Exception argumentException2 = new ArgumentException("AclObject");
							base.WriteError(new ErrorRecord(argumentException2, "SetAcl_AclObject", ErrorCategory.InvalidArgument, this.AclObject));
							return;
						}
						else
						{
							sddlForm = commonSecurityDescriptor.GetSddlForm(AccessControlSections.All);
						}
					}
					else
					{
						sddlForm = objectSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.All);
					}
					try
					{
						object[] objArray1 = new object[1];
						objArray1[0] = sddlForm;
						item.Invoke(objArray1);
						return;
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						CommandProcessorBase.CheckForSevereException(exception2);
						ErrorRecord errorRecord3 = SecurityUtils.CreateNotSupportedErrorRecord(UtilsStrings.MethodInvokeFail, "SetAcl_OperationNotSupported", new object[0]);
						base.WriteError(errorRecord3);
					}
				}
			}
		}
	}
}