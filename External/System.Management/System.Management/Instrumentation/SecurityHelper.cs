using System.Security.Permissions;

namespace System.Management.Instrumentation
{
	internal sealed class SecurityHelper
	{
		internal readonly static SecurityPermission UnmanagedCode;

		static SecurityHelper()
		{
			SecurityHelper.UnmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
		}

		public SecurityHelper()
		{
		}
	}
}