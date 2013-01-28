using Microsoft.Management.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Microsoft.Management.Infrastructure.Internal
{
	internal static class Helpers
	{
		public static void SafeInvoke<T>(this EventHandler<T> eventHandler, object sender, T eventArgs)
		where T : EventArgs
		{
			if (eventHandler != null)
			{
				eventHandler(sender, eventArgs);
			}
		}

		public static string ToStringFromNameAndValue(string name, object value)
		{
			if (value != null)
			{
				if (value as CimInstance != null || value as Array != null)
				{
					value = "...";
				}
				else
				{
					if (value as string != null || Convert.ToChar(value) != Convert.ToChar(0))
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append('\"');
						string str = value.ToString();
						for (int i = 0; i < str.Length; i++)
						{
							char chr = str[i];
							if (char.IsControl(chr) || !char.IsLetterOrDigit(chr) && !char.IsPunctuation(chr) && !char.IsWhiteSpace(chr))
							{
								stringBuilder.Append('?');
							}
							else
							{
								stringBuilder.Append(chr);
							}
						}
						stringBuilder.Append('\"');
						value = stringBuilder.ToString();
					}
				}
				string str1 = value.ToString();
				if (str1.Length > 40)
				{
					str1 = string.Concat(str1.Substring(0, 40), "...");
				}
				object[] objArray = new object[2];
				objArray[0] = name;
				objArray[1] = str1;
				string str2 = string.Format(CultureInfo.InvariantCulture, Strings.CimNameAndValueToString, objArray);
				return str2;
			}
			else
			{
				return name;
			}
		}

		public static void ValidateNoNullElements(IList list)
		{
			if (list != null)
			{
				IEnumerable<object> objs = list.Cast<object>();
				if (!objs.Any<object>((object element) => (element == null)))
				{
					return;
				}
				else
				{
					throw new ArgumentException(Strings.ArrayCannotContainNullElements);
				}
			}
			else
			{
				return;
			}
		}

        internal static IntPtr GetCurrentSecurityToken()
        {
			return IntPtr.Zero; /* TODO: */
        }
    }
}