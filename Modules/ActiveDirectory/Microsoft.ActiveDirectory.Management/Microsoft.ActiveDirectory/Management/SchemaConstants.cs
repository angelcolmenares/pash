using System;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management
{
	internal class SchemaConstants
	{
		internal const string MatchAnyObject = "*";

		internal static string SidAttributeSyntax;

		internal static string NameGroup;

		internal static string SyntaxGroup;

		internal static string SingleValueGroup;

		internal static string SystemOnlyGroup;

		internal static ulong systemFlagsConstructedBitMask;

		internal static Regex AttributeTypesRegex;

		internal static Regex ExtendedAttrInfoRegex;

		internal static string Attributelist;

		internal static string ParentClasslist;

		static SchemaConstants()
		{
			SchemaConstants.SidAttributeSyntax = "2.5.5.17";
			SchemaConstants.NameGroup = "NAME";
			SchemaConstants.SyntaxGroup = "SYNTAX";
			SchemaConstants.SingleValueGroup = "SINGLEVALUE";
			SchemaConstants.SystemOnlyGroup = "SYSTEMONLY";
			SchemaConstants.systemFlagsConstructedBitMask = (long)4;
			string[] nameGroup = new string[7];
			nameGroup[0] = "(?:\\(\\s{1}\\S*\\s{1}NAME\\s*')(?<";
			nameGroup[1] = SchemaConstants.NameGroup;
			nameGroup[2] = ">[^']*)(?:'\\s*SYNTAX\\s{1}')(?<";
			nameGroup[3] = SchemaConstants.SyntaxGroup;
			nameGroup[4] = ">[^']*)(?<";
			nameGroup[5] = SchemaConstants.SingleValueGroup;
			nameGroup[6] = ">.*SINGLE-VALUE)?";
			SchemaConstants.AttributeTypesRegex = new Regex(string.Concat(nameGroup), RegexOptions.Compiled);
			string[] systemOnlyGroup = new string[5];
			systemOnlyGroup[0] = "(?:\\(\\s{1}\\S*\\s{1}NAME\\s*')(?<";
			systemOnlyGroup[1] = SchemaConstants.NameGroup;
			systemOnlyGroup[2] = ">[^']*).*?(?<";
			systemOnlyGroup[3] = SchemaConstants.SystemOnlyGroup;
			systemOnlyGroup[4] = ">.*SYSTEM-ONLY)?";
			SchemaConstants.ExtendedAttrInfoRegex = new Regex(string.Concat(systemOnlyGroup), RegexOptions.Compiled);
			SchemaConstants.Attributelist = "attributelist";
			SchemaConstants.ParentClasslist = "parentClasslist";
		}

		public SchemaConstants()
		{
		}
	}
}