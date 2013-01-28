using Microsoft.ActiveDirectory.Management;
using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal delegate IADOPathNode ToSearchFilterDelegate(string extendedAttributeName, string[] directoryAttributes, IADOPathNode filterClause, CmdletSessionInfo cmdletSessionInfo);
}