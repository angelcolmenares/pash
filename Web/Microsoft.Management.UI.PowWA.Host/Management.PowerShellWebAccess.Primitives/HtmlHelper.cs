using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI.HtmlControls;

namespace Microsoft.Management.PowerShellWebAccess.Primitives
{
	public static class HtmlHelper
	{
		private static Dictionary<ConsoleColor, string> ConsoleColorToHtmlColorMap;

		private readonly static Regex MobileBrowserRegex;

		static HtmlHelper()
		{
			Dictionary<ConsoleColor, string> consoleColors = new Dictionary<ConsoleColor, string>();
			consoleColors.Add(ConsoleColor.Black, "#000000");
			consoleColors.Add(ConsoleColor.DarkBlue, "#012456");
			consoleColors.Add(ConsoleColor.DarkGreen, "#006400");
			consoleColors.Add(ConsoleColor.DarkCyan, "#008B8B");
			consoleColors.Add(ConsoleColor.DarkRed, "#8B0000");
			consoleColors.Add(ConsoleColor.DarkMagenta, "#8B008B");
			consoleColors.Add(ConsoleColor.DarkYellow, "#8B8B00");
			consoleColors.Add(ConsoleColor.Gray, "#808080");
			consoleColors.Add(ConsoleColor.DarkGray, "#A9A9A9");
			consoleColors.Add(ConsoleColor.Blue, "#0000FF");
			consoleColors.Add(ConsoleColor.Green, "#00FF00");
			consoleColors.Add(ConsoleColor.Cyan, "#00FFFF");
			consoleColors.Add(ConsoleColor.Red, "#FF0000");
			consoleColors.Add(ConsoleColor.Magenta, "#FF00FF");
			consoleColors.Add(ConsoleColor.Yellow, "#FFFF00");
			consoleColors.Add(ConsoleColor.White, "#FFFFFF");
			HtmlHelper.ConsoleColorToHtmlColorMap = consoleColors;
			HtmlHelper.MobileBrowserRegex = new Regex("(iPhone;)|(XBLWP7;)|(Windows\\sPhone\\sOS\\s7.\\d;)");
		}

		public static void AppendCssClass(this HtmlControl control, string cssClassName)
		{
			string item = control.Attributes["class"];
			if (!string.IsNullOrEmpty(item))
			{
				control.Attributes["class"] = string.Concat(item, " ", cssClassName);
				return;
			}
			else
			{
				control.Attributes["class"] = cssClassName;
				return;
			}
		}

		public static bool IsMobileBrowser(string agent)
		{
			return HtmlHelper.MobileBrowserRegex.IsMatch(agent);
		}

		public static void RemoveCssClass(this HtmlControl control, string cssClassName)
		{
			char[] chrArray = new char[1];
			chrArray[0] = ' ';
			string[] strArrays = control.Attributes["class"].Split(chrArray);
			if ((int)strArrays.Length != 0)
			{
				string str = "";
				string[] strArrays1 = strArrays;
				for (int i = 0; i < (int)strArrays1.Length; i++)
				{
					string str1 = strArrays1[i];
					if (string.Compare(str1, cssClassName, StringComparison.OrdinalIgnoreCase) != 0)
					{
						if (str.Length != 0)
						{
							str = string.Concat(str, " ", str1);
						}
						else
						{
							str = str1;
						}
					}
				}
				control.Attributes["class"] = str;
				return;
			}
			else
			{
				return;
			}
		}

		public static string ToHtmlColor(ConsoleColor color)
		{
			return HtmlHelper.ConsoleColorToHtmlColorMap[color];
		}
	}
}