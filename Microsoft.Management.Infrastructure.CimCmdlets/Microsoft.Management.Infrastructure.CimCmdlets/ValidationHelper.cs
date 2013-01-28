using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal static class ValidationHelper
	{
		public static string ValidateArgumentIsValidName(string parameterName, string value)
		{
			DebugHelper.WriteLogEx();
			if (value != null)
			{
				string str = value.Trim();
				Regex regex = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*\\z");
				if (regex.IsMatch(str))
				{
					object[] objArray = new object[2];
					objArray[0] = parameterName;
					objArray[1] = value;
					DebugHelper.WriteLogEx("A valid name: {0}={1}", 0, objArray);
					return str;
				}
			}
			object[] objArray1 = new object[2];
			objArray1[0] = parameterName;
			objArray1[1] = value;
			DebugHelper.WriteLogEx("An invalid name: {0}={1}", 0, objArray1);
			object[] objArray2 = new object[2];
			objArray2[0] = value;
			objArray2[1] = parameterName;
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Strings.InvalidParameterValue, objArray2));
		}

		public static string[] ValidateArgumentIsValidName(string parameterName, string[] value)
		{
			if (value != null)
			{
				string[] strArrays = value;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str = strArrays[i];
					if (str == null || string.Compare(str.Trim(), "*", StringComparison.OrdinalIgnoreCase) != 0)
					{
						ValidationHelper.ValidateArgumentIsValidName(parameterName, str);
					}
				}
			}
			return value;
		}

		public static void ValidateNoNullArgument(object obj, string argumentName)
		{
			if (obj != null)
			{
				return;
			}
			else
			{
				throw new ArgumentNullException(argumentName);
			}
		}

		public static void ValidateNoNullorWhiteSpaceArgument(string obj, string argumentName)
		{
			if (!string.IsNullOrWhiteSpace(obj))
			{
				return;
			}
			else
			{
				throw new ArgumentException(argumentName);
			}
		}
	}
}