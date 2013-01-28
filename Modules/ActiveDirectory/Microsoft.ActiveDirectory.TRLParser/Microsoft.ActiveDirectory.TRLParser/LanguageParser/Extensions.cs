using Microsoft.ActiveDirectory.TRLParser;
using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	internal static class Extensions
	{
		private const int MaxClaimValueLength = 0x1f4;

		private const string TruncateTrailer = ".....";

		private static int TruncateTrailerLength;

		static Extensions()
		{
			Extensions.TruncateTrailerLength = ".....".Length;
		}

		public static void Validate(this string str)
		{
			if (!str.Contains("\""))
			{
				if (!str.Contains("\n"))
				{
					return;
				}
				else
				{
					object[] objArray = new object[2];
					objArray[0] = "\n";
					objArray[1] = str;
					throw new PolicyValidationException(SR.GetString("POLICY0014", objArray));
				}
			}
			else
			{
				object[] objArray1 = new object[2];
				objArray1[0] = "\"";
				objArray1[1] = str;
				throw new PolicyValidationException(SR.GetString("POLICY0014", objArray1));
			}
		}
	}
}