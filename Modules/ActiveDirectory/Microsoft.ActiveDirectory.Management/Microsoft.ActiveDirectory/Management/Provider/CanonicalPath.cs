using System;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal static class CanonicalPath
	{
		private readonly static char delimiterChar;

		private readonly static char escapeChar;

		static CanonicalPath()
		{
			CanonicalPath.delimiterChar = '/';
			CanonicalPath.escapeChar = '\\';
		}

		public static bool ComparePath(string path1, string path2)
		{
			if (ADPathHelper.StartsWithDelimiter(path1, CanonicalPath.delimiterChar))
			{
				path1 = path1.Substring(1);
			}
			if (ADPathHelper.StartsWithDelimiter(path2, CanonicalPath.delimiterChar))
			{
				path2 = path2.Substring(1);
			}
			if (ADPathHelper.EndsWithDelimiter(path1, CanonicalPath.delimiterChar, CanonicalPath.escapeChar))
			{
				path1 = path1.Substring(0, path1.Length - 1);
			}
			if (ADPathHelper.EndsWithDelimiter(path2, CanonicalPath.delimiterChar, CanonicalPath.escapeChar))
			{
				path2 = path2.Substring(0, path2.Length - 1);
			}
			return path1.Equals(path2, StringComparison.OrdinalIgnoreCase);
		}

		public static string GetChildName(string path)
		{
			if (ADPathHelper.EndsWithDelimiter(path, CanonicalPath.delimiterChar, CanonicalPath.escapeChar))
			{
				path = path.Substring(0, path.Length - 1);
			}
			int num = ADPathHelper.IndexOfLastDelimiter(path, CanonicalPath.delimiterChar, CanonicalPath.escapeChar);
			if (num < 0 || num >= path.Length - 1)
			{
				return path;
			}
			else
			{
				return path.Substring(num + 1);
			}
		}

		public static string GetParentPath(string path, string root)
		{
			if (ADPathHelper.EndsWithDelimiter(path, CanonicalPath.delimiterChar, CanonicalPath.escapeChar))
			{
				path = path.Substring(0, path.Length - 1);
			}
			if (ADPathHelper.EndsWithDelimiter(root, CanonicalPath.delimiterChar, CanonicalPath.escapeChar))
			{
				root = root.Substring(0, root.Length - 1);
			}
			if (!string.Equals(path, root, StringComparison.InvariantCultureIgnoreCase))
			{
				int num = ADPathHelper.IndexOfLastDelimiter(path, CanonicalPath.delimiterChar, CanonicalPath.escapeChar);
				if (num < 0 || num >= path.Length - 1)
				{
					return "";
				}
				else
				{
					return path.Substring(0, num);
				}
			}
			else
			{
				return "";
			}
		}

		internal static int IndexOfFirstDelimiter(string path)
		{
			return ADPathHelper.IndexOfFirstDelimiter(path, CanonicalPath.delimiterChar, CanonicalPath.escapeChar);
		}

		public static bool IsChildPath(string path, string parentPath, bool includeSelf)
		{
			if (includeSelf || path.Length > parentPath.Length)
			{
				return path.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				return false;
			}
		}

		public static bool IsValidPath(string path)
		{
			bool flag = false;
			bool flag1 = false;
			int num = -1;
			uint num1 = 0;
			if (path.Length >= 2)
			{
				for (int i = 0; i < path.Length; i++)
				{
					if (!flag1)
					{
						if (!flag)
						{
							if (path[i] != CanonicalPath.delimiterChar)
							{
								if (path[i] == CanonicalPath.escapeChar)
								{
									flag = true;
								}
							}
							else
							{
								num = i;
								num1++;
							}
						}
						else
						{
							if (!Uri.IsHexDigit(path[i]))
							{
								if (path[i] == CanonicalPath.escapeChar || path[i] == CanonicalPath.delimiterChar)
								{
									flag = false;
								}
								else
								{
									return false;
								}
							}
							else
							{
								flag1 = true;
								flag = false;
							}
						}
					}
					else
					{
						if (Uri.IsHexDigit(path[i]))
						{
							flag1 = false;
						}
						else
						{
							return false;
						}
					}
				}
				if (flag || flag1)
				{
					return false;
				}
				else
				{
					if (num1 != 0)
					{
						if (num1 <= 1 || num != path.Length - 1)
						{
							return true;
						}
						else
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				return false;
			}
		}

		public static string MakePath(string parent, string child)
		{
			if (!ADPathHelper.StartsWithDelimiter(child, CanonicalPath.delimiterChar) || !ADPathHelper.EndsWithDelimiter(parent, CanonicalPath.delimiterChar, CanonicalPath.escapeChar))
			{
				if (ADPathHelper.StartsWithDelimiter(child, CanonicalPath.delimiterChar) || ADPathHelper.EndsWithDelimiter(parent, CanonicalPath.delimiterChar, CanonicalPath.escapeChar))
				{
					return string.Concat(parent, child);
				}
				else
				{
					return string.Concat(parent, CanonicalPath.delimiterChar, child);
				}
			}
			else
			{
				return string.Concat(parent, child.Substring(1));
			}
		}
	}
}