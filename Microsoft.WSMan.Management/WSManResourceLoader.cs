using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Microsoft.WSMan.Management
{
	internal static class WSManResourceLoader
	{
		private static Dictionary<string, string> ResourceValueCache;

		static WSManResourceLoader()
		{
			WSManResourceLoader.ResourceValueCache = new Dictionary<string, string>();
		}

		internal static string GetResourceString(string Key)
		{
			if (WSManResourceLoader.ResourceValueCache.Count <= 0)
			{
				WSManResourceLoader.LoadResourceData();
			}
			string str = "";
			if (WSManResourceLoader.ResourceValueCache.ContainsKey(Key.Trim()))
			{
				WSManResourceLoader.ResourceValueCache.TryGetValue(Key.Trim(), out str);
			}
			return str.Trim();
		}

		internal static void LoadResourceData()
		{
			try
			{
				object[] lCID = new object[1];
				lCID[0] = (uint)Thread.CurrentThread.CurrentUICulture.LCID;
				string str = string.Concat(Environment.ExpandEnvironmentVariables("%Windir%"), "\\System32\\Winrm\\", string.Concat("0", string.Format(CultureInfo.CurrentCulture, "{0:x2}", lCID)), "\\winrm.ini");
				if (File.Exists(str))
				{
					FileStream fileStream = new FileStream(str, FileMode.Open, FileAccess.Read);
					StreamReader streamReader = new StreamReader(fileStream);
					while (!streamReader.EndOfStream)
					{
						string str1 = streamReader.ReadLine();
						if (!str1.Contains("="))
						{
							continue;
						}
						char[] chrArray = new char[1];
						chrArray[0] = '=';
						string[] strArrays = str1.Split(chrArray, 2);
						if (WSManResourceLoader.ResourceValueCache.ContainsKey(strArrays[0].Trim()))
						{
							continue;
						}
						char[] chrArray1 = new char[1];
						chrArray1[0] = '\"';
						char[] chrArray2 = new char[1];
						chrArray2[0] = '\"';
						string str2 = strArrays[1].TrimStart(chrArray1).TrimEnd(chrArray2);
						WSManResourceLoader.ResourceValueCache.Add(strArrays[0].Trim(), str2.Trim());
					}
				}
			}
			catch (IOException oException1)
			{
				IOException oException = oException1;
				throw oException;
			}
		}
	}
}