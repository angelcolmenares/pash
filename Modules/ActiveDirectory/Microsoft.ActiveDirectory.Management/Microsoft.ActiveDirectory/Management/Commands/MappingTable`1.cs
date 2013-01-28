using System;
using System.Collections.Generic;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class MappingTable<T> : Dictionary<string, T>
	where T : MappingTableEntry
	{
		internal MappingTable() : base(StringComparer.OrdinalIgnoreCase)
		{
		}
	}
}