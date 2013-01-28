using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Threading;

namespace System.Runtime
{
	internal static class PartialTrustHelpers
	{
		[SecurityCritical]
		private static Type aptca;

		[SecurityCritical]
		private static volatile bool checkedForFullTrust;

		[SecurityCritical]
		private static bool inFullTrust;

		internal static bool AppDomainFullyTrusted
		{
			[SecuritySafeCritical]
			get
			{
				if (!PartialTrustHelpers.checkedForFullTrust)
				{
					PartialTrustHelpers.inFullTrust = AppDomain.CurrentDomain.IsFullyTrusted;
					PartialTrustHelpers.checkedForFullTrust = true;
				}
				return PartialTrustHelpers.inFullTrust;
			}
		}

		internal static bool ShouldFlowSecurityContext
		{
			[SecurityCritical]
			get
			{
				return SecurityManager.CurrentThreadRequiresSecurityContextCapture();
			}
		}

		[SecurityCritical]
		internal static SecurityContext CaptureSecurityContextNoIdentityFlow()
		{
			SecurityContext securityContext;
			if (!SecurityContext.IsWindowsIdentityFlowSuppressed())
			{
				AsyncFlowControl asyncFlowControl = SecurityContext.SuppressFlowWindowsIdentity();
				try
				{
					securityContext = SecurityContext.Capture();
				}
				finally
				{
					asyncFlowControl.Dispose();
				}
				return securityContext;
			}
			else
			{
				return SecurityContext.Capture();
			}
		}

		[SecurityCritical]
		internal static bool CheckAppDomainPermissions(PermissionSet permissions)
		{
			if (!AppDomain.CurrentDomain.IsHomogenous)
			{
				return false;
			}
			else
			{
				return permissions.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
			}
		}

		[PermissionSet(SecurityAction.Demand, Unrestricted=true)]
		[SecuritySafeCritical]
		internal static void DemandForFullTrust()
		{
		}

		[SecurityCritical]
		internal static bool HasEtwPermissions()
		{
			PermissionSet permissionSets = new PermissionSet(PermissionState.Unrestricted);
			return PartialTrustHelpers.CheckAppDomainPermissions(permissionSets);
		}

		[SecurityCritical]
		private static bool IsAssemblyAptca(Assembly assembly)
		{
			if (PartialTrustHelpers.aptca == null)
			{
				PartialTrustHelpers.aptca = typeof(AllowPartiallyTrustedCallersAttribute);
			}
			return (int)assembly.GetCustomAttributes(PartialTrustHelpers.aptca, false).Length > 0;
		}

		[FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		private static bool IsAssemblySigned(Assembly assembly)
		{
			byte[] publicKeyToken = assembly.GetName().GetPublicKeyToken();
			return publicKeyToken != null & (int)publicKeyToken.Length > 0;
		}

		[SecurityCritical]
		internal static bool IsInFullTrust()
		{
			bool flag;
			if (SecurityManager.CurrentThreadRequiresSecurityContextCapture())
			{
				try
				{
					PartialTrustHelpers.DemandForFullTrust();
					flag = true;
				}
				catch (SecurityException securityException)
				{
					flag = false;
				}
				return flag;
			}
			else
			{
				return true;
			}
		}

		[SecurityCritical]
		internal static bool IsTypeAptca(Type type)
		{
			Assembly assembly = type.Assembly;
			if (PartialTrustHelpers.IsAssemblyAptca(assembly))
			{
				return true;
			}
			else
			{
				return !PartialTrustHelpers.IsAssemblySigned(assembly);
			}
		}
	}
}