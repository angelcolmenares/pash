using System;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal static class ADPathHelper
	{
		public static bool EndsWithDelimiter(string path, char delimiter, char escapeChar)
		{
			if (!string.IsNullOrEmpty(path))
			{
				if (!path.EndsWith(delimiter.ToString()))
				{
					return false;
				}
				else
				{
					bool flag = false;
					for (int i = path.Length - 2; i >= 0 && path[i] == escapeChar; i--)
					{
						flag = !flag;
					}
					return !flag;
				}
			}
			else
			{
				return false;
			}
		}

		public static int IndexOfFirstDelimiter(string path, char delimiter, char escapeChar)
		{
			bool flag = false;
			if (!string.IsNullOrEmpty(path))
			{
				for (int i = 0; i < path.Length; i++)
				{
					if (path[i] != escapeChar)
					{
						if (flag || path[i] != delimiter)
						{
							flag = false;
						}
						else
						{
							return i;
						}
					}
					else
					{
						flag = !flag;
					}
				}
				return -1;
			}
			else
			{
				return -1;
			}
		}

		public static int IndexOfLastDelimiter(string path, char delimiter, char escapeChar)
		{
			if (!string.IsNullOrEmpty(path))
			{
				for (int i = path.Length - 1; i >= 0; i--)
				{
					if (path[i] == delimiter)
					{
						bool flag = false;
						for (int j = i - 1; j >= 0 && path[j] == escapeChar; j--)
						{
							flag = !flag;
						}
						if (!flag)
						{
							return i;
						}
					}
				}
				return -1;
			}
			else
			{
				return -1;
			}
		}

		public static string MakePshPath(string parent, string child)
		{
			if (parent != string.Empty)
			{
				if (child != string.Empty)
				{
					if (child[0] != '\\' || parent[parent.Length - 1] != '\\')
					{
						if (child[0] == '\\' || parent[parent.Length - 1] == '\\')
						{
							return string.Concat(parent, child);
						}
						else
						{
							return string.Concat(parent, "\\", child);
						}
					}
					else
					{
						return string.Concat(parent, child.Substring(1));
					}
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

		public static bool StartsWithDelimiter(string path, char delimiter)
		{
			if (!string.IsNullOrEmpty(path))
			{
				return path.StartsWith(delimiter.ToString());
			}
			else
			{
				return false;
			}
		}
	}
}