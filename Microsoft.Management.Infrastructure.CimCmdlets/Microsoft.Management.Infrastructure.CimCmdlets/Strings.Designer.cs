using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Strings
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		internal static string CimOperationCompleted
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationCompleted", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameCreateInstance
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameCreateInstance", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameDeleteInstance
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameDeleteInstance", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameEnumerateAssociatedInstances
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameEnumerateAssociatedInstances", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameEnumerateClasses
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameEnumerateClasses", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameEnumerateInstances
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameEnumerateInstances", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameGetClass
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameGetClass", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameGetInstance
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameGetInstance", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameInvokeMethod
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameInvokeMethod", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameModifyInstance
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameModifyInstance", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameQueryInstances
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameQueryInstances", Strings.resourceCulture);
			}
		}

		internal static string CimOperationNameSubscribeIndication
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationNameSubscribeIndication", Strings.resourceCulture);
			}
		}

		internal static string CimOperationStart
		{
			get
			{
				return Strings.ResourceManager.GetString("CimOperationStart", Strings.resourceCulture);
			}
		}

		internal static string ConflictParameterWasSet
		{
			get
			{
				return Strings.ResourceManager.GetString("ConflictParameterWasSet", Strings.resourceCulture);
			}
		}

		internal static string CouldNotFindCimsessionObject
		{
			get
			{
				return Strings.ResourceManager.GetString("CouldNotFindCimsessionObject", Strings.resourceCulture);
			}
		}

		internal static string CouldNotFindPropertyFromGivenClass
		{
			get
			{
				return Strings.ResourceManager.GetString("CouldNotFindPropertyFromGivenClass", Strings.resourceCulture);
			}
		}

		internal static string CouldNotModifyReadonlyProperty
		{
			get
			{
				return Strings.ResourceManager.GetString("CouldNotModifyReadonlyProperty", Strings.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Strings.resourceCulture;
			}
			set
			{
				Strings.resourceCulture = value;
			}
		}

		internal static string DefaultStatusDescription
		{
			get
			{
				return Strings.ResourceManager.GetString("DefaultStatusDescription", Strings.resourceCulture);
			}
		}

		internal static string InvalidAuthenticationTypeWithNullCredential
		{
			get
			{
				return Strings.ResourceManager.GetString("InvalidAuthenticationTypeWithNullCredential", Strings.resourceCulture);
			}
		}

		internal static string InvalidMethod
		{
			get
			{
				return Strings.ResourceManager.GetString("InvalidMethod", Strings.resourceCulture);
			}
		}

		internal static string InvalidMethodParameter
		{
			get
			{
				return Strings.ResourceManager.GetString("InvalidMethodParameter", Strings.resourceCulture);
			}
		}

		internal static string InvalidOperation
		{
			get
			{
				return Strings.ResourceManager.GetString("InvalidOperation", Strings.resourceCulture);
			}
		}

		internal static string InvalidParameterValue
		{
			get
			{
				return Strings.ResourceManager.GetString("InvalidParameterValue", Strings.resourceCulture);
			}
		}

		internal static string NullArgument
		{
			get
			{
				return Strings.ResourceManager.GetString("NullArgument", Strings.resourceCulture);
			}
		}

		internal static string OperationInProgress
		{
			get
			{
				return Strings.ResourceManager.GetString("OperationInProgress", Strings.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Strings.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("Microsoft.Management.Infrastructure.CimCmdlets.Strings", typeof(Strings).Assembly);
					Strings.resourceMan = resourceManager;
				}
				return Strings.resourceMan;
			}
		}

		internal static string UnableToAddPropertyToInstance
		{
			get
			{
				return Strings.ResourceManager.GetString("UnableToAddPropertyToInstance", Strings.resourceCulture);
			}
		}

		internal static string UnableToResolvePareameterSetName
		{
			get
			{
				return Strings.ResourceManager.GetString("UnableToResolvePareameterSetName", Strings.resourceCulture);
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		internal Strings()
		{
		}
	}
}