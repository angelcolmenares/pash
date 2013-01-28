using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal static class ADPathModule
	{
		public static bool ComparePath(string path1, string path2, ADPathFormat format)
		{
			if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2))
			{
				return path1 == path2;
			}
			else
			{
				ADPathFormat aDPathFormat = format;
				switch (aDPathFormat)
				{
					case ADPathFormat.X500:
					{
						return X500Path.ComparePath(path1, path2);
					}
					case ADPathFormat.Canonical:
					{
						return CanonicalPath.ComparePath(path1, path2);
					}
				}
				return false;
			}
		}

		public static string ConvertPath(ADSessionInfo sessionInfo, string path, ADPathFormat fromFormat, ADPathFormat toFormat)
		{
			string str;
			if (fromFormat != toFormat)
			{
				if (fromFormat == ADPathFormat.Canonical && !string.IsNullOrEmpty(path) && CanonicalPath.IndexOfFirstDelimiter(path) == -1)
				{
					path = string.Concat(path, "/");
				}
				using (ADAccountManagement aDAccountManagement = new ADAccountManagement(sessionInfo))
				{
					str = aDAccountManagement.TranslateName(path, fromFormat, toFormat);
				}
				return str;
			}
			else
			{
				return path;
			}
		}

		public static string GetChildName(string path, ADPathFormat format)
		{
			if (!string.IsNullOrEmpty(path))
			{
				ADPathFormat aDPathFormat = format;
				switch (aDPathFormat)
				{
					case ADPathFormat.X500:
					{
						return X500Path.GetChildName(path);
					}
					case ADPathFormat.Canonical:
					{
						return CanonicalPath.GetChildName(path);
					}
				}
				return null;
			}
			else
			{
				return "";
			}
		}

		public static string GetParentPath(string path, string root, ADPathFormat format)
		{
			if (!string.IsNullOrEmpty(path))
			{
				ADPathFormat aDPathFormat = format;
				switch (aDPathFormat)
				{
					case ADPathFormat.X500:
					{
						return X500Path.GetParentPath(path, root);
					}
					case ADPathFormat.Canonical:
					{
						return CanonicalPath.GetParentPath(path, root);
					}
				}
				return null;
			}
			else
			{
				return "";
			}
		}

		public static bool IsChildPath(string path, string parentPath, bool includeSelf, ADPathFormat format)
		{
			if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(parentPath))
			{
				if (!includeSelf)
				{
					return false;
				}
				else
				{
					return path == parentPath;
				}
			}
			else
			{
				ADPathFormat aDPathFormat = format;
				switch (aDPathFormat)
				{
					case ADPathFormat.X500:
					{
						return X500Path.IsChildPath(path, parentPath, includeSelf);
					}
					case ADPathFormat.Canonical:
					{
						return CanonicalPath.IsChildPath(path, parentPath, includeSelf);
					}
				}
				return false;
			}
		}

		public static bool IsValidPath(string path, ADPathFormat format)
		{
			if (!string.IsNullOrEmpty(path))
			{
				ADPathFormat aDPathFormat = format;
				switch (aDPathFormat)
				{
					case ADPathFormat.X500:
					{
						return X500Path.IsValidPath(path);
					}
					case ADPathFormat.Canonical:
					{
						return CanonicalPath.IsValidPath(path);
					}
				}
				return false;
			}
			else
			{
				return true;
			}
		}

		public static string MakePath(string parent, string child, ADPathFormat format)
		{
			if (!string.IsNullOrEmpty(parent) || !string.IsNullOrEmpty(child))
			{
				if (!string.IsNullOrEmpty(parent))
				{
					if (!string.IsNullOrEmpty(child))
					{
						ADPathFormat aDPathFormat = format;
						switch (aDPathFormat)
						{
							case ADPathFormat.X500:
							{
								return X500Path.MakePath(parent, child);
							}
							case ADPathFormat.Canonical:
							{
								return CanonicalPath.MakePath(parent, child);
							}
						}
						return null;
					}
					else
					{
						return parent;
					}
				}
				else
				{
					return child;
				}
			}
			else
			{
				return "";
			}
		}
	}
}