using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Management.Automation;
using System.Management.Automation.Security;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Microsoft.PowerShell.Commands
{
	public abstract class SecurityDescriptorCommandsBase : PSCmdlet
	{
		private string filter;

		private string[] include;

		private string[] exclude;

		internal CmdletProviderContext CmdletProviderContext
		{
			get
			{
				CmdletProviderContext cmdletProviderContext = new CmdletProviderContext(this);
				Collection<string> collection = SessionStateUtilities.ConvertArrayToCollection<string>(this.Include);
				Collection<string> strs = SessionStateUtilities.ConvertArrayToCollection<string>(this.Exclude);
				cmdletProviderContext.SetFilters(collection, strs, this.Filter);
				return cmdletProviderContext;
			}
		}

		[Parameter]
		public string[] Exclude
		{
			get
			{
				return this.exclude;
			}
			set
			{
				this.exclude = value;
			}
		}

		[Parameter]
		public string Filter
		{
			get
			{
				return this.filter;
			}
			set
			{
				this.filter = value;
			}
		}

		[Parameter]
		public string[] Include
		{
			get
			{
				return this.include;
			}
			set
			{
				this.include = value;
			}
		}

		protected SecurityDescriptorCommandsBase()
		{
			this.include = new string[0];
			this.exclude = new string[0];
		}

		internal static void AddBrokeredProperties(Collection<PSObject> results, bool audit, bool allCentralAccessPolicies)
		{
			foreach (PSObject result in results)
			{
				if (audit)
				{
					result.Properties.Add(new PSCodeProperty("Audit", typeof(SecurityDescriptorCommandsBase).GetMethod("GetAudit")));
				}
				if (allCentralAccessPolicies)
				{
					result.Properties.Add(new PSCodeProperty("AllCentralAccessPolicies", typeof(SecurityDescriptorCommandsBase).GetMethod("GetAllCentralAccessPolicies")));
				}
				result.Properties.Add(new PSCodeProperty("CentralAccessPolicyId", typeof(SecurityDescriptorCommandsBase).GetMethod("GetCentralAccessPolicyId")));
				result.Properties.Add(new PSCodeProperty("CentralAccessPolicyName", typeof(SecurityDescriptorCommandsBase).GetMethod("GetCentralAccessPolicyName")));
			}
		}

		public static AuthorizationRuleCollection GetAccess(PSObject instance)
		{
			AuthorizationRuleCollection accessRules;
			if (instance != null)
			{
				ObjectSecurity baseObject = instance.BaseObject as ObjectSecurity;
				if (baseObject == null)
				{
					PSTraceSource.NewArgumentException("instance");
				}
				CommonObjectSecurity commonObjectSecurity = baseObject as CommonObjectSecurity;
				if (commonObjectSecurity == null)
				{
					DirectoryObjectSecurity directoryObjectSecurity = baseObject as DirectoryObjectSecurity;
					accessRules = directoryObjectSecurity.GetAccessRules(true, true, typeof(NTAccount));
				}
				else
				{
					accessRules = commonObjectSecurity.GetAccessRules(true, true, typeof(NTAccount));
				}
				return accessRules;
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("instance");
			}
		}

		public static string[] GetAllCentralAccessPolicies(PSObject instance)
		{
			unsafe
			{
				string[] strArrays;
				IntPtr zero = IntPtr.Zero;
				try
				{
					int num = 0;
					int num1 = NativeMethods.LsaQueryCAPs(null, 0, out zero, out num);
					if (num1 == 0)
					{
						if (num == 0 || zero == IntPtr.Zero)
						{
							strArrays = null;
						}
						else
						{
							string[] strArrays1 = new string[num];
							IntPtr intPtr = zero;
							uint num2 = 0;
							while (num2 < num)
							{
								NativeMethods.CENTRAL_ACCESS_POLICY structure = (NativeMethods.CENTRAL_ACCESS_POLICY)Marshal.PtrToStructure(intPtr, typeof(NativeMethods.CENTRAL_ACCESS_POLICY));
								strArrays1[num2] = string.Concat("\"", Marshal.PtrToStringUni(structure.Name.Buffer, structure.Name.Length / 2), "\"");
								IntPtr cAPID = structure.CAPID;
								bool flag = NativeMethods.IsValidSid(cAPID);
								if (flag)
								{
									SecurityIdentifier securityIdentifier = new SecurityIdentifier(cAPID);
									string[] strArrays2 = strArrays1;
									string[] strArrays3 = strArrays2;
									uint num3 = num2;
									IntPtr intPtr1 = (IntPtr)num3;
									strArrays2[num3] = string.Concat(strArrays3[intPtr1.ToInt32()], " (", securityIdentifier.ToString(), ")");
									intPtr = intPtr + Marshal.SizeOf(structure);
									num2++;
								}
								else
								{
									throw new Win32Exception(Marshal.GetLastWin32Error());
								}
							}
							strArrays = strArrays1;
						}
					}
					else
					{
						throw new Win32Exception(num1);
					}
				}
				finally
				{
					NativeMethods.LsaFreeMemory(zero);
				}
				return strArrays;
			}
		}

		public static AuthorizationRuleCollection GetAudit(PSObject instance)
		{
			AuthorizationRuleCollection auditRules;
			if (instance != null)
			{
				ObjectSecurity baseObject = instance.BaseObject as ObjectSecurity;
				if (baseObject == null)
				{
					PSTraceSource.NewArgumentException("instance");
				}
				CommonObjectSecurity commonObjectSecurity = baseObject as CommonObjectSecurity;
				if (commonObjectSecurity == null)
				{
					DirectoryObjectSecurity directoryObjectSecurity = baseObject as DirectoryObjectSecurity;
					auditRules = directoryObjectSecurity.GetAuditRules(true, true, typeof(NTAccount));
				}
				else
				{
					auditRules = commonObjectSecurity.GetAuditRules(true, true, typeof(NTAccount));
				}
				return auditRules;
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("instance");
			}
		}

		public static SecurityIdentifier GetCentralAccessPolicyId(PSObject instance)
		{
			SecurityIdentifier securityIdentifier;
			SessionState sessionState = new SessionState();
			string unresolvedProviderPathFromPSPath = sessionState.Path.GetUnresolvedProviderPathFromPSPath(SecurityDescriptorCommandsBase.GetPath(instance));
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			IntPtr intPtr1 = IntPtr.Zero;
			IntPtr zero2 = IntPtr.Zero;
			try
			{
				int namedSecurityInfo = NativeMethods.GetNamedSecurityInfo(unresolvedProviderPathFromPSPath, NativeMethods.SeObjectType.SE_FILE_OBJECT, NativeMethods.SecurityInformation.SCOPE_SECURITY_INFORMATION, out zero, out intPtr, out zero1, out intPtr1, out zero2);
				if (namedSecurityInfo == 0)
				{
					if (intPtr1 != IntPtr.Zero)
					{
						NativeMethods.ACL structure = (NativeMethods.ACL)Marshal.PtrToStructure(intPtr1, typeof(NativeMethods.ACL));
						if (structure.AceCount != 0)
						{
							NativeMethods.ACL aCL = new NativeMethods.ACL();
							IntPtr aceSize = intPtr1 + Marshal.SizeOf(aCL);
							for (uint i = 0; i < structure.AceCount; i++)
							{
								NativeMethods.ACE_HEADER aCEHEADER = (NativeMethods.ACE_HEADER)Marshal.PtrToStructure(aceSize, typeof(NativeMethods.ACE_HEADER));
								if ((aCEHEADER.AceFlags & 8) == 0)
								{
									break;
								}
								aceSize = aceSize + aCEHEADER.AceSize;
							}
							NativeMethods.SYSTEM_AUDIT_ACE sYSTEMAUDITACE = new NativeMethods.SYSTEM_AUDIT_ACE();
							IntPtr intPtr2 = aceSize + Marshal.SizeOf(sYSTEMAUDITACE) - Marshal.SizeOf((uint)0);
							bool flag = NativeMethods.IsValidSid(intPtr2);
							if (flag)
							{
								securityIdentifier = new SecurityIdentifier(intPtr2);
							}
							else
							{
								throw new Win32Exception(Marshal.GetLastWin32Error());
							}
						}
						else
						{
							securityIdentifier = null;
						}
					}
					else
					{
						securityIdentifier = null;
					}
				}
				else
				{
					throw new Win32Exception(namedSecurityInfo);
				}
			}
			finally
			{
				NativeMethods.LocalFree(zero2);
			}
			return securityIdentifier;
		}

		public static string GetCentralAccessPolicyName(PSObject instance)
		{
			string stringUni;
			SecurityIdentifier centralAccessPolicyId = SecurityDescriptorCommandsBase.GetCentralAccessPolicyId(instance);
			int binaryLength = centralAccessPolicyId.BinaryLength;
			byte[] numArray = new byte[binaryLength];
			centralAccessPolicyId.GetBinaryForm(numArray, 0);
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = Marshal.AllocHGlobal(binaryLength);
			try
			{
				Marshal.Copy(numArray, 0, intPtr, binaryLength);
				IntPtr[] intPtrArray = new IntPtr[1];
				intPtrArray[0] = intPtr;
				int num = 0;
				int num1 = NativeMethods.LsaQueryCAPs(intPtrArray, 1, out zero, out num);
				if (num1 == 0)
				{
					if (num == 0 || zero == IntPtr.Zero)
					{
						stringUni = null;
					}
					else
					{
						NativeMethods.CENTRAL_ACCESS_POLICY structure = (NativeMethods.CENTRAL_ACCESS_POLICY)Marshal.PtrToStructure(zero, typeof(NativeMethods.CENTRAL_ACCESS_POLICY));
						stringUni = Marshal.PtrToStringUni(structure.Name.Buffer, structure.Name.Length / 2);
					}
				}
				else
				{
					throw new Win32Exception(num1);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(intPtr);
				NativeMethods.LsaFreeMemory(zero);
			}
			return stringUni;
		}

		public static string GetGroup(PSObject instance)
		{
			string str;
			if (instance != null)
			{
				ObjectSecurity baseObject = instance.BaseObject as ObjectSecurity;
				if (baseObject != null)
				{
					try
					{
						IdentityReference group = baseObject.GetGroup(typeof(NTAccount));
						str = group.ToString();
					}
					catch (IdentityNotMappedException identityNotMappedException)
					{
						return baseObject.GetSecurityDescriptorSddlForm(AccessControlSections.Group);
					}
					return str;
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("instance");
				}
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("instance");
			}
		}

		public static string GetOwner(PSObject instance)
		{
			string str;
			if (instance != null)
			{
				ObjectSecurity baseObject = instance.BaseObject as ObjectSecurity;
				if (baseObject != null)
				{
					try
					{
						IdentityReference owner = baseObject.GetOwner(typeof(NTAccount));
						str = owner.ToString();
					}
					catch (IdentityNotMappedException identityNotMappedException)
					{
						return baseObject.GetSecurityDescriptorSddlForm(AccessControlSections.Owner);
					}
					return str;
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("instance");
				}
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("instance");
			}
		}

		public static string GetPath(PSObject instance)
		{
			if (instance != null)
			{
				return instance.Properties["PSPath"].Value.ToString();
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("instance");
			}
		}

		public static string GetSddl(PSObject instance)
		{
			if (instance != null)
			{
				ObjectSecurity baseObject = instance.BaseObject as ObjectSecurity;
				if (baseObject != null)
				{
					string securityDescriptorSddlForm = baseObject.GetSecurityDescriptorSddlForm(AccessControlSections.All);
					return securityDescriptorSddlForm;
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("instance");
				}
			}
			else
			{
				throw PSTraceSource.NewArgumentNullException("instance");
			}
		}
	}
}