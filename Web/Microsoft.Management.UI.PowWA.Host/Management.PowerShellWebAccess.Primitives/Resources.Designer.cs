using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	[CompilerGenerated]
	[DebuggerNonUserCode]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		internal static string BufferHeightOutOfRange_Format
		{
			get
			{
				return Resources.ResourceManager.GetString("BufferHeightOutOfRange_Format", Resources.resourceCulture);
			}
		}

		internal static string BufferSmallerThanWindow
		{
			get
			{
				return Resources.ResourceManager.GetString("BufferSmallerThanWindow", Resources.resourceCulture);
			}
		}

		internal static string BufferWidthOutOfRange_Format
		{
			get
			{
				return Resources.ResourceManager.GetString("BufferWidthOutOfRange_Format", Resources.resourceCulture);
			}
		}

		internal static string ClientMessagesFlushed_Format
		{
			get
			{
				return Resources.ResourceManager.GetString("ClientMessagesFlushed_Format", Resources.resourceCulture);
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

		internal static string DebugLineFormatString
		{
			get
			{
				return Resources.ResourceManager.GetString("DebugLineFormatString", Resources.resourceCulture);
			}
		}

		internal static string ErrorClientMessageTooLarge
		{
			get
			{
				return Resources.ResourceManager.GetString("ErrorClientMessageTooLarge", Resources.resourceCulture);
			}
		}

		internal static string ErrorTooManyClientMessages
		{
			get
			{
				return Resources.ResourceManager.GetString("ErrorTooManyClientMessages", Resources.resourceCulture);
			}
		}

		internal static string GatewayAuthorizationFailure
		{
			get
			{
				return Resources.ResourceManager.GetString("GatewayAuthorizationFailure", Resources.resourceCulture);
			}
		}

		internal static string GatewayAuthorizationFailureInvalidRules
		{
			get
			{
				return Resources.ResourceManager.GetString("GatewayAuthorizationFailureInvalidRules", Resources.resourceCulture);
			}
		}

		internal static string InvalidUserNameInDomainCredentials
		{
			get
			{
				return Resources.ResourceManager.GetString("InvalidUserNameInDomainCredentials", Resources.resourceCulture);
			}
		}

		internal static string LoginFailure
		{
			get
			{
				return Resources.ResourceManager.GetString("LoginFailure", Resources.resourceCulture);
			}
		}

		internal static string PromptForCredentialTargetNameNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PromptForCredentialTargetNameNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PromptTypeConversionError_Format
		{
			get
			{
				return Resources.ResourceManager.GetString("PromptTypeConversionError_Format", Resources.resourceCulture);
			}
		}

		internal static string PSHostEnterNestedPromptNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostEnterNestedPromptNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostExitNestedPromptNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostExitNestedPromptNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceCursorSizeNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceCursorSizeNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceFlushInputBufferNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceFlushInputBufferNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceGetBufferContentsNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceGetBufferContentsNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceKeyAvailableNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceKeyAvailableNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceMaxPhysicalWindowSizeNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceMaxPhysicalWindowSizeNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceMaxWindowSizeNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceMaxWindowSizeNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceReadKeyNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceReadKeyNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceScrollBufferContentsNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceScrollBufferContentsNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceSetBufferContentsNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceSetBufferContentsNotSupported", Resources.resourceCulture);
			}
		}

		internal static string PSHostRawUserInterfaceWindowPositionNotSupported
		{
			get
			{
				return Resources.ResourceManager.GetString("PSHostRawUserInterfaceWindowPositionNotSupported", Resources.resourceCulture);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (object.ReferenceEquals(Resources.resourceMan, null))
				{
					ResourceManager resourceManager = new ResourceManager("Microsoft.Management.PowerShellWebAccess.Primitives.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		internal static string Status_Failure
		{
			get
			{
				return Resources.ResourceManager.GetString("Status_Failure", Resources.resourceCulture);
			}
		}

		internal static string Status_Success
		{
			get
			{
				return Resources.ResourceManager.GetString("Status_Success", Resources.resourceCulture);
			}
		}

		internal static string UserActiveSessionLimitReached
		{
			get
			{
				return Resources.ResourceManager.GetString("UserActiveSessionLimitReached", Resources.resourceCulture);
			}
		}

		internal static string VerboseLineFormatString
		{
			get
			{
				return Resources.ResourceManager.GetString("VerboseLineFormatString", Resources.resourceCulture);
			}
		}

		internal static string WarningLineFormatString
		{
			get
			{
				return Resources.ResourceManager.GetString("WarningLineFormatString", Resources.resourceCulture);
			}
		}

		internal static string WindowHeightOutOfRange_Format
		{
			get
			{
				return Resources.ResourceManager.GetString("WindowHeightOutOfRange_Format", Resources.resourceCulture);
			}
		}

		internal static string WindowLargerThanBuffer
		{
			get
			{
				return Resources.ResourceManager.GetString("WindowLargerThanBuffer", Resources.resourceCulture);
			}
		}

		internal static string WindowWidthOutOfRange_Format
		{
			get
			{
				return Resources.ResourceManager.GetString("WindowWidthOutOfRange_Format", Resources.resourceCulture);
			}
		}

		internal Resources()
		{
		}
	}
}