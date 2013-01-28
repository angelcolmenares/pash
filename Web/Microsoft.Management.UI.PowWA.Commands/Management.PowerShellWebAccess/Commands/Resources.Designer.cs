using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Management.PowerShellWebAccess.Commands
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		internal static string AuthorizationRule_ForceComputerNameQuery
		{
			get
			{
				return Resources.ResourceManager.GetString("AuthorizationRule_ForceComputerNameQuery", Resources.resourceCulture);
			}
		}

		internal static string AuthorizationRule_UseFqdnQuery
		{
			get
			{
				return Resources.ResourceManager.GetString("AuthorizationRule_UseFqdnQuery", Resources.resourceCulture);
			}
		}

		internal static string AuthorizationRule_UseIpAddressQuery
		{
			get
			{
				return Resources.ResourceManager.GetString("AuthorizationRule_UseIpAddressQuery", Resources.resourceCulture);
			}
		}

		internal static string AuthorizationRuleIdName_DisplayFormat
		{
			get
			{
				return Resources.ResourceManager.GetString("AuthorizationRuleIdName_DisplayFormat", Resources.resourceCulture);
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

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("Microsoft.Management.PowerShellWebAccess.Commands.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		internal static string Rule_NotFoundById
		{
			get
			{
				return Resources.ResourceManager.GetString("Rule_NotFoundById", Resources.resourceCulture);
			}
		}

		internal static string Rule_NotFoundByName
		{
			get
			{
				return Resources.ResourceManager.GetString("Rule_NotFoundByName", Resources.resourceCulture);
			}
		}

		internal static string TestRule_NoMatchWithWarnings
		{
			get
			{
				return Resources.ResourceManager.GetString("TestRule_NoMatchWithWarnings", Resources.resourceCulture);
			}
		}

		internal static string TestRule_Warning
		{
			get
			{
				return Resources.ResourceManager.GetString("TestRule_Warning", Resources.resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}