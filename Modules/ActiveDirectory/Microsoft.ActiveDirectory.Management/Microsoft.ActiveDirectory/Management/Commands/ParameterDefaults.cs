using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class ParameterDefaults
	{
		internal const int ResultPageSize = 0x100;

		internal const ADSearchScope SearchScope = ADSearchScope.Subtree;

		internal static int? ResultSetSize;

		static ParameterDefaults()
		{
			ParameterDefaults.ResultSetSize = null;
		}
	}
}