using System;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class MappingTableEntry
	{
		private string _extendedAttribute;

		internal string ExtendedAttribute
		{
			get
			{
				return this._extendedAttribute;
			}
		}

		internal MappingTableEntry(string extendedAttribute)
		{
			this._extendedAttribute = extendedAttribute;
		}

		private MappingTableEntry()
		{
		}
	}
}