using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management.Provider
{
	internal static class X500Path
	{
		private readonly static char delimiterChar;

		private readonly static char escapeChar;

		private readonly static char equalsChar;

		private readonly static char[] escapableCharsArray;

		private readonly static HashSet<char> escapableChars;

		private readonly static char[] charsRequiringEscapingArray;

		private readonly static HashSet<char> charsRequiringEscaping;

		private readonly static Regex rdnAttIDRegex;

		private readonly static Regex berValRegex;

		static X500Path()
		{
			X500Path.delimiterChar = ',';
			X500Path.escapeChar = '\\';
			X500Path.equalsChar = '=';
			char[] chrArray = new char[] { ',', '+', '\"', '\\', '>', '<', ';', '=', ' ', '#' };
			X500Path.escapableCharsArray = chrArray;
			X500Path.escapableChars = new HashSet<char>(X500Path.escapableCharsArray);
			char[] chrArray1 = new char[] { '+', '\"', '>', '<', ';', ',', '=' };
			X500Path.charsRequiringEscapingArray = chrArray1;
			X500Path.charsRequiringEscaping = new HashSet<char>(X500Path.charsRequiringEscapingArray);
			X500Path.rdnAttIDRegex = new Regex("^[^\\,+=\"<>;#]+$");
			X500Path.berValRegex = new Regex("^#(?:[0-9a-fA-F][0-9a-fA-F])+$");
		}

		public static bool ComparePath(string path1, string path2)
		{
			if (ADPathHelper.StartsWithDelimiter(path1, X500Path.delimiterChar))
			{
				path1 = path1.Substring(1);
			}
			if (ADPathHelper.StartsWithDelimiter(path2, X500Path.delimiterChar))
			{
				path2 = path2.Substring(1);
			}
			if (ADPathHelper.EndsWithDelimiter(path1, X500Path.delimiterChar, X500Path.escapeChar))
			{
				path1 = path1.Substring(0, path1.Length - 1);
			}
			if (ADPathHelper.EndsWithDelimiter(path2, X500Path.delimiterChar, X500Path.escapeChar))
			{
				path2 = path2.Substring(0, path2.Length - 1);
			}
			path1 = X500Path.StandardizeX500Path(path1);
			path2 = X500Path.StandardizeX500Path(path2);
			return path1.Equals(path2, StringComparison.OrdinalIgnoreCase);
		}

		public static string GetChildName(string path)
		{
			if (ADPathHelper.StartsWithDelimiter(path, X500Path.delimiterChar))
			{
				path = path.Substring(1);
			}
			int num = ADPathHelper.IndexOfFirstDelimiter(path, X500Path.delimiterChar, X500Path.escapeChar);
			if (num < 0)
			{
				return path;
			}
			else
			{
				return path.Substring(0, num);
			}
		}

		public static string GetParentPath(string path, string root)
		{
			if (ADPathHelper.StartsWithDelimiter(path, X500Path.delimiterChar))
			{
				path = path.Substring(1);
			}
			if (ADPathHelper.StartsWithDelimiter(root, X500Path.delimiterChar))
			{
				root = root.Substring(1);
			}
			root = X500Path.StripX500Whitespace(root);
			if (!string.Equals(X500Path.StripX500Whitespace(path), root, StringComparison.InvariantCultureIgnoreCase))
			{
				int num = ADPathHelper.IndexOfFirstDelimiter(path, X500Path.delimiterChar, X500Path.escapeChar);
				if (num < 0 || num >= path.Length - 1)
				{
					return "";
				}
				else
				{
					return path.Substring(num + 1);
				}
			}
			else
			{
				return "";
			}
		}

		internal static int IndexOfFirstDelimiter(string path)
		{
			return ADPathHelper.IndexOfFirstDelimiter(path, X500Path.delimiterChar, X500Path.escapeChar);
		}

		public static bool IsChildPath(string path, string parentPath, bool includeSelf)
		{
			path = X500Path.StandardizeX500Path(path);
			parentPath = X500Path.StandardizeX500Path(parentPath);
			if (includeSelf || path.Length > parentPath.Length)
			{
				return path.EndsWith(parentPath, StringComparison.OrdinalIgnoreCase);
			}
			else
			{
				return false;
			}
		}

		private static bool IsProperlyFormedRDNAttID(string component)
		{
			if (!string.IsNullOrEmpty(component))
			{
				return X500Path.rdnAttIDRegex.IsMatch(component);
			}
			else
			{
				return false;
			}
		}

		private static bool IsProperlyFormedValue(string component)
		{
			bool flag = false;
			bool flag1 = false;
			if (!string.IsNullOrEmpty(component))
			{
				if (component[0] != '#')
				{
					for (int i = 0; i < component.Length; i++)
					{
						if (!flag1)
						{
							if (!flag)
							{
								if (!X500Path.charsRequiringEscaping.Contains(component[i]))
								{
									if (component[i] == X500Path.escapeChar)
									{
										flag = true;
									}
								}
								else
								{
									return false;
								}
							}
							else
							{
								if (!Uri.IsHexDigit(component[i]))
								{
									if (!X500Path.escapableChars.Contains(component[i]))
									{
										return false;
									}
									else
									{
										flag = false;
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
							if (Uri.IsHexDigit(component[i]))
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
						return true;
					}
				}
				else
				{
					return X500Path.berValRegex.IsMatch(component);
				}
			}
			else
			{
				return false;
			}
		}

		public static bool IsValidPath(string path)
		{
			string str = null;
			path = X500Path.StripX500Whitespace(path);
			while (path != null)
			{
				int num = ADPathHelper.IndexOfFirstDelimiter(path, X500Path.delimiterChar, X500Path.escapeChar);
				if (num <= 0 || num >= path.Length - 1)
				{
					if (num == 0 || num == path.Length - 1)
					{
						return false;
					}
					else
					{
						str = path;
						path = null;
					}
				}
				else
				{
					str = path.Substring(0, num);
					path = path.Substring(num + 1);
				}
				int num1 = str.IndexOf(X500Path.equalsChar);
				if (num1 <= 0 || num1 >= str.Length - 1)
				{
					if (num1 == 0 || num1 == str.Length - 1)
					{
						return false;
					}
					else
					{
						return false;
					}
				}
				else
				{
					string str1 = str.Substring(0, num1);
					string str2 = str.Substring(num1 + 1);
					if (X500Path.IsProperlyFormedRDNAttID(str1) && X500Path.IsProperlyFormedValue(str2))
					{
						continue;
					}
					return false;
				}
			}
			return true;
		}

		public static string MakePath(string parent, string child)
		{
			if (!ADPathHelper.StartsWithDelimiter(parent, X500Path.delimiterChar) || !ADPathHelper.EndsWithDelimiter(child, X500Path.delimiterChar, X500Path.escapeChar))
			{
				if (ADPathHelper.StartsWithDelimiter(parent, X500Path.delimiterChar) || ADPathHelper.EndsWithDelimiter(child, X500Path.delimiterChar, X500Path.escapeChar))
				{
					return string.Concat(child, parent);
				}
				else
				{
					return string.Concat(child, X500Path.delimiterChar, parent);
				}
			}
			else
			{
				return string.Concat(child, parent.Substring(1));
			}
		}

		public static string StandardizeX500Path(string path)
		{
			return X500Path.StripX500Whitespace(path);
		}

		public static string StripX500Whitespace(string path)
		{
			bool flag = false;
			bool flag1 = true;
			int num = 0;
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(path))
			{
				for (int i = 0; i < path.Length; i++)
				{
					if (!flag)
					{
						if (path[i] != ' ')
						{
							if (path[i] == X500Path.equalsChar || path[i] == X500Path.delimiterChar)
							{
								flag1 = true;
							}
							else
							{
								flag1 = false;
								if (path[i] == X500Path.escapeChar)
								{
									flag = true;
								}
								for (int j = 0; j < num; j++)
								{
									stringBuilder.Append(' ');
								}
							}
							stringBuilder.Append(path[i]);
							num = 0;
						}
						else
						{
							if (!flag1)
							{
								num++;
							}
						}
					}
					else
					{
						flag = false;
						stringBuilder.Append(path[i]);
					}
				}
				return stringBuilder.ToString();
			}
			else
			{
				return "";
			}
		}
	}
}