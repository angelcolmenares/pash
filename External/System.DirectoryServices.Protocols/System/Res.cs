using System;

namespace System.DirectoryServices
{
	public static class Res
	{
		public static string GetString(string resourceKey)
		{
			return resourceKey;
		}

		public static string GetString(string resourceName, string resourceKey)
		{
			return resourceKey;
		}

		public static string GetString(string resourceKey, object[] args)
		{
			return resourceKey;
		}
	}
}

