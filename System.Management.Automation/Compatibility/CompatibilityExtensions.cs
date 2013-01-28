using System;
using System.Reflection;

namespace System
{
	public static class CompatibilityExtensions
	{
		public static object GetValue(this PropertyInfo obj, object handle)
		{
			return obj.GetValue(handle);
		}

		public static string GetSecurePassword(this System.Management.ConnectionOptions obj)
		{
#if MONO
			return string.Empty;
#else
			return obj.SecurePassword;
#endif
		}
	}
}

