using System;
using System.Configuration;
using System.IO;
using System.Management.Automation;
using System.Web;

namespace Microsoft.Management.Odata.Common
{
	internal static class Utils
	{
		public static bool CompareByteArrays(byte[] firstArray, byte[] secondArray)
		{
			if (firstArray != secondArray)
			{
				if (firstArray == null || secondArray == null || (int)firstArray.Length != (int)secondArray.Length)
				{
					return false;
				}
				else
				{
					int num = 0;
					while (num < (int)firstArray.Length)
					{
						if (firstArray[num] == secondArray[num])
						{
							num++;
						}
						else
						{
							return false;
						}
					}
					return true;
				}
			}
			else
			{
				return true;
			}
		}

		public static string GetBaseBinDirectory(string binaryFileName)
		{
			return Utils.GetBaseDirectory(true, binaryFileName);
		}

		public static string GetBaseContentDirectory()
		{
			return Utils.GetBaseDirectory(false, "");
		}

		private static string GetBaseDirectory(bool binaryDirectory, string binaryFileName = "")
		{
			string currentDirectory;
			if (HttpContext.Current == null)
			{
				currentDirectory = Directory.GetCurrentDirectory();
			}
			else
			{
				if (!binaryDirectory || !string.Equals(Path.GetFileName(binaryFileName), binaryFileName, StringComparison.Ordinal))
				{
					currentDirectory = HttpContext.Current.Server.MapPath(".");
				}
				else
				{
					currentDirectory = HttpContext.Current.Server.MapPath(".\\bin");
				}
			}
			string item = ConfigurationManager.AppSettings["BasePath"];
			string[] strArrays = new string[5];
			strArrays[0] = "initial path '";
			strArrays[1] = currentDirectory;
			strArrays[2] = "'; override path '";
			strArrays[3] = item;
			strArrays[4] = "'";
			TraceHelper.Current.DebugMessage(string.Concat(strArrays));
			if (!string.IsNullOrEmpty(item))
			{
				currentDirectory = Path.Combine(currentDirectory, item);
			}
			TraceHelper.Current.DebugMessage(string.Concat("using base path '", currentDirectory, "'"));
			return currentDirectory;
		}

		public static PSInvocationSettings GetPSInvocationSettings()
		{
			PSInvocationSettings pSInvocationSetting = new PSInvocationSettings();
			pSInvocationSetting.FlowImpersonationPolicy = true;
			return pSInvocationSetting;
		}
	}
}