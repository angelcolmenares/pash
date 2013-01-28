using Microsoft.Management.Odata;
using System;
using System.Globalization;
using System.Text;

namespace Microsoft.Management.Odata.Common
{
	internal static class ParameterExtensions
	{
		public static void ThrowIfNull(this object instance, string parameterName, string message, object[] args)
		{
			string empty;
			if (instance != null)
			{
				return;
			}
			else
			{
				if (string.IsNullOrEmpty(message))
				{
					empty = string.Empty;
				}
				else
				{
					empty = string.Format(CultureInfo.CurrentCulture, message, args);
				}
				string str = empty;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Throwing null argument exception. \nCause");
				object[] objArray = new object[2];
				objArray[0] = parameterName;
				objArray[1] = str;
				stringBuilder.AppendLine(string.Format(CultureInfo.CurrentCulture, Resources.NullParameter, objArray));
				stringBuilder.AppendLine("Stack Trace");
				stringBuilder.AppendLine(StackTraceHelper.GetStrackTrace(2));
				TraceHelper.Current.DebugMessage(stringBuilder.ToString());
				if (!string.IsNullOrEmpty(message))
				{
					throw new ArgumentNullException(parameterName, str);
				}
				else
				{
					throw new ArgumentNullException(parameterName);
				}
			}
		}

		public static void ThrowIfNull(this object instance, string parameterName, ParameterExtensions.MessageLoader loader, object[] args)
		{
			if (instance == null)
			{
				instance.ThrowIfNull(parameterName, loader(), args);
			}
		}

		public static void ThrowIfNullOrEmpty(this string instance, string parameterName)
		{
			instance.ThrowIfNullOrEmpty(parameterName, null, new object[0]);
		}

		public static void ThrowIfNullOrEmpty(this string instance, string parameterName, string message, object[] args)
		{
			string empty;
			string nullParameter;
			if (!string.IsNullOrEmpty(instance))
			{
				return;
			}
			else
			{
				if (string.IsNullOrEmpty(message))
				{
					empty = string.Empty;
				}
				else
				{
					empty = string.Format(CultureInfo.CurrentCulture, message, args);
				}
				string str = empty;
				StringBuilder stringBuilder = new StringBuilder();
				if (instance != null)
				{
					stringBuilder.AppendLine("Throwing empty argument exception. \nCause");
				}
				else
				{
					stringBuilder.AppendLine("Throwing null argument exception. \nCause");
				}
				StringBuilder stringBuilder1 = stringBuilder;
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				if (instance == null)
				{
					nullParameter = Resources.NullParameter;
				}
				else
				{
					nullParameter = Resources.EmptyArgumentPassed;
				}
				object[] objArray = new object[2];
				objArray[0] = parameterName;
				objArray[1] = str;
				stringBuilder1.AppendLine(string.Format(currentCulture, nullParameter, objArray));
				stringBuilder.AppendLine("Stack Trace");
				stringBuilder.AppendLine(StackTraceHelper.GetStrackTrace(2));
				TraceHelper.Current.DebugMessage(stringBuilder.ToString());
				if (instance != null)
				{
					throw new ArgumentException(str, parameterName);
				}
				else
				{
					if (!string.IsNullOrEmpty(message))
					{
						throw new ArgumentNullException(parameterName, str);
					}
					else
					{
						throw new ArgumentNullException(parameterName);
					}
				}
			}
		}

		public delegate string MessageLoader();
	}
}