using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Microsoft.Management.PowerShellWebAccess
{
	public static class PswaHelper
	{
		private static Regex localNameRegex;

		static PswaHelper()
		{
			PswaHelper.localNameRegex = new Regex("^(\\.|localhost)\\\\(?<name>.+)$", RegexOptions.IgnoreCase);
		}

		public static string TranslateLocalAccountName(string name)
		{
			Match match = PswaHelper.localNameRegex.Match(name);
			if (!match.Success)
			{
				return name;
			}
			else
			{
				return string.Concat(Environment.MachineName, "\\", match.Groups["name"].ToString());
			}
		}

		public static string TranslateLocalComputerName(string name)
		{
			string str;
			if (string.Compare(name, "localhost", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(name, ".", StringComparison.OrdinalIgnoreCase) == 0)
			{
				try
				{
					string hostName = Dns.GetHostEntry("localhost").HostName;
					if (string.IsNullOrEmpty(hostName))
					{
						return name;
					}
					else
					{
						str = hostName;
					}
				}
				catch (Exception exception)
				{
					return name;
				}
				return str;
			}
			return name;
		}
	}
}