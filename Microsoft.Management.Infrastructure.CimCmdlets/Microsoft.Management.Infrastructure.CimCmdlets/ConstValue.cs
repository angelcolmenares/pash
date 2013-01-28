using System;
using System.Collections.Generic;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal static class ConstValue
	{
		internal static string[] DefaultSessionName;

		internal static string NullComputerName;

		internal static string[] NullComputerNames;

		internal static string LocalhostComputerName;

		internal static string DefaultNameSpace;

		internal static string DefaultQueryDialect;

		internal static string ShowComputerNameNoteProperty;

		static ConstValue()
		{
			string[] strArrays = new string[1];
			strArrays[0] = "*";
			ConstValue.DefaultSessionName = strArrays;
			ConstValue.NullComputerName = null;
			string[] nullComputerName = new string[1];
			nullComputerName[0] = ConstValue.NullComputerName;
			ConstValue.NullComputerNames = nullComputerName;
			ConstValue.LocalhostComputerName = "localhost";
			ConstValue.DefaultNameSpace = "root\\cimv2";
			ConstValue.DefaultQueryDialect = "WQL";
			ConstValue.ShowComputerNameNoteProperty = "PSShowComputerName";
		}

		internal static string GetComputerName(string computerName)
		{
			if (string.IsNullOrEmpty(computerName))
			{
				return ConstValue.NullComputerName;
			}
			else
			{
				return computerName;
			}
		}

		internal static IEnumerable<string> GetComputerNames(IEnumerable<string> computerNames)
		{
			if (computerNames == null)
			{
				return ConstValue.NullComputerNames;
			}
			else
			{
				return computerNames;
			}
		}

		internal static string GetNamespace(string nameSpace)
		{
			if (nameSpace == null)
			{
				return ConstValue.DefaultNameSpace;
			}
			else
			{
				return nameSpace;
			}
		}

		internal static string GetQueryDialectWithDefault(string queryDialect)
		{
			if (queryDialect == null)
			{
				return ConstValue.DefaultQueryDialect;
			}
			else
			{
				return queryDialect;
			}
		}

		internal static bool IsDefaultComputerName(string computerName)
		{
			return string.IsNullOrEmpty(computerName);
		}
	}
}