using System;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
	internal static class WMIHelper
	{
		internal static string GetScopeString(string computer, string namespaceParameter)
		{
			StringBuilder stringBuilder = new StringBuilder("\\\\");
			stringBuilder.Append(computer);
			stringBuilder.Append("\\");
			stringBuilder.Append(namespaceParameter);
			return stringBuilder.ToString();
		}
	}
}