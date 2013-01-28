using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Management.PowerShellWebAccess.Management
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		internal static string AccountToSidConvertError
		{
			get
			{
				return Resources.ResourceManager.GetString("AccountToSidConvertError", Resources.resourceCulture);
			}
		}

		internal static string AccountToSidConvertExtendedError
		{
			get
			{
				return Resources.ResourceManager.GetString("AccountToSidConvertExtendedError", Resources.resourceCulture);
			}
		}

		internal static string AuthorizationRuleFile_WarningComment
		{
			get
			{
				return Resources.ResourceManager.GetString("AuthorizationRuleFile_WarningComment", Resources.resourceCulture);
			}
		}

		internal static string AuthorizationRuleName_Format
		{
			get
			{
				return Resources.ResourceManager.GetString("AuthorizationRuleName_Format", Resources.resourceCulture);
			}
		}

		internal static string CannotFindComputer
		{
			get
			{
				return Resources.ResourceManager.GetString("CannotFindComputer", Resources.resourceCulture);
			}
		}

		internal static string ComputerGroupIsNotSecurityGroup
		{
			get
			{
				return Resources.ResourceManager.GetString("ComputerGroupIsNotSecurityGroup", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		internal static string DestinationTypeDoesNotMatchComputer
		{
			get
			{
				return Resources.ResourceManager.GetString("DestinationTypeDoesNotMatchComputer", Resources.resourceCulture);
			}
		}

		internal static string DestinationTypeDoesNotMatchComputerGroup
		{
			get
			{
				return Resources.ResourceManager.GetString("DestinationTypeDoesNotMatchComputerGroup", Resources.resourceCulture);
			}
		}

		internal static string ErrorKey
		{
			get
			{
				return Resources.ResourceManager.GetString("ErrorKey", Resources.resourceCulture);
			}
		}

		internal static string InvalidComputerNameFormat
		{
			get
			{
				return Resources.ResourceManager.GetString("InvalidComputerNameFormat", Resources.resourceCulture);
			}
		}

		internal static string InvalidUserAndGroupNameFormat
		{
			get
			{
				return Resources.ResourceManager.GetString("InvalidUserAndGroupNameFormat", Resources.resourceCulture);
			}
		}

		internal static string LoadFile_ConfigFileBad
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadFile_ConfigFileBad", Resources.resourceCulture);
			}
		}

		internal static string LoadFile_ConfigFileMissing
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadFile_ConfigFileMissing", Resources.resourceCulture);
			}
		}

		internal static string LoadFile_ConfigFileMissingAuthorizationFile
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadFile_ConfigFileMissingAuthorizationFile", Resources.resourceCulture);
			}
		}

		internal static string LoadFile_RuleFileBad
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadFile_RuleFileBad", Resources.resourceCulture);
			}
		}

		internal static string LoadFile_RuleFileMissing
		{
			get
			{
				return Resources.ResourceManager.GetString("LoadFile_RuleFileMissing", Resources.resourceCulture);
			}
		}

		internal static string NoActiveDirectoryPermission
		{
			get
			{
				return Resources.ResourceManager.GetString("NoActiveDirectoryPermission", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("Microsoft.Management.PowerShellWebAccess.Management.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		internal static string SaveFileFailed
		{
			get
			{
				return Resources.ResourceManager.GetString("SaveFileFailed", Resources.resourceCulture);
			}
		}

		internal static string SidToAccountConvertError
		{
			get
			{
				return Resources.ResourceManager.GetString("SidToAccountConvertError", Resources.resourceCulture);
			}
		}

		internal static string UserGroupIsNotSecurityGroup
		{
			get
			{
				return Resources.ResourceManager.GetString("UserGroupIsNotSecurityGroup", Resources.resourceCulture);
			}
		}

		internal static string UserTypeDoesNotMatchUser
		{
			get
			{
				return Resources.ResourceManager.GetString("UserTypeDoesNotMatchUser", Resources.resourceCulture);
			}
		}

		internal static string UserTypeDoesNotMatchUserGroup
		{
			get
			{
				return Resources.ResourceManager.GetString("UserTypeDoesNotMatchUserGroup", Resources.resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}