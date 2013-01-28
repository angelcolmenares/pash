using System;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management
{
	internal class LdapOptionConstants
	{
		internal static string LdapOptionSeperator;

		internal static string LowRangeGroup;

		internal static string HiRangeGroup;

		internal static string RangeOptionText;

		internal static string RangeValueSeperator;

		internal static string RangeOptionFormatString;

		internal static Regex RangeOptionRegex;

		internal static int LowRangeIndex;

		internal static int HighRangeIndex;

		static LdapOptionConstants()
		{
			LdapOptionConstants.LdapOptionSeperator = ";";
			LdapOptionConstants.LowRangeGroup = "LOWRANGE";
			LdapOptionConstants.HiRangeGroup = "HIRANGE";
			LdapOptionConstants.RangeOptionText = "Range=";
			LdapOptionConstants.RangeValueSeperator = "-";
			string[] ldapOptionSeperator = new string[6];
			ldapOptionSeperator[0] = "{0}";
			ldapOptionSeperator[1] = LdapOptionConstants.LdapOptionSeperator;
			ldapOptionSeperator[2] = LdapOptionConstants.RangeOptionText;
			ldapOptionSeperator[3] = "{1}";
			ldapOptionSeperator[4] = LdapOptionConstants.RangeValueSeperator;
			ldapOptionSeperator[5] = "{2}";
			LdapOptionConstants.RangeOptionFormatString = string.Concat(ldapOptionSeperator);
			string[] rangeOptionText = new string[8];
			rangeOptionText[0] = LdapOptionConstants.RangeOptionText;
			rangeOptionText[1] = "(?<";
			rangeOptionText[2] = LdapOptionConstants.LowRangeGroup;
			rangeOptionText[3] = ">[0-9]*)";
			rangeOptionText[4] = LdapOptionConstants.RangeValueSeperator;
			rangeOptionText[5] = "(?<";
			rangeOptionText[6] = LdapOptionConstants.HiRangeGroup;
			rangeOptionText[7] = ">[0-9]*|\\*)$";
			LdapOptionConstants.RangeOptionRegex = new Regex(string.Concat(rangeOptionText), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft | RegexOptions.CultureInvariant);
			LdapOptionConstants.LowRangeIndex = LdapOptionConstants.RangeOptionRegex.GroupNumberFromName(LdapOptionConstants.LowRangeGroup);
			LdapOptionConstants.HighRangeIndex = LdapOptionConstants.RangeOptionRegex.GroupNumberFromName(LdapOptionConstants.HiRangeGroup);
		}

		private LdapOptionConstants()
		{
		}
	}
}