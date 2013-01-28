using Microsoft.ActiveDirectory.TRLParser;
using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	internal static class Utility
	{
		private const int MaxClaimsPageLength = 0x800;

		private const string TruncationIndicator = "...";

		public static object VerifyNonNull(string name, object value)
		{
			if (value != null)
			{
				string str = value as string;
				if (str == null || !string.IsNullOrEmpty(str))
				{
					return value;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = name;
					throw new PolicyValidationException(SR.GetString("POLICY0000", objArray));
				}
			}
			else
			{
				object[] objArray1 = new object[1];
				objArray1[0] = name;
				throw new PolicyValidationException(SR.GetString("POLICY0005", objArray1));
			}
		}

		public static object VerifyNonNullArgument(string name, object value)
		{
			if (value != null)
			{
				string str = value as string;
				if (str == null || !string.IsNullOrEmpty(str))
				{
					return value;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = name;
					throw new ArgumentException(SR.GetString("POLICY0000", objArray));
				}
			}
			else
			{
				throw new ArgumentNullException(name);
			}
		}
	}
}