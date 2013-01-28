using System;
using System.Diagnostics;

namespace Microsoft.Management.Odata.Common
{
	internal static class StackTraceHelper
	{
		public const int MaxStringizeStackSize = 0xfa0;

		public static string GetStrackTrace(int skipFrames)
		{
			StackTrace stackTrace = new StackTrace(skipFrames + 1);
			string str = stackTrace.ToString();
			if (str.Length >= 0xfa0)
			{
				return str.Substring(0, 0xfa0);
			}
			else
			{
				return str;
			}
		}
	}
}