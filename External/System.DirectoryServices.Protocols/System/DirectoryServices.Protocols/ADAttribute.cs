using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	internal class ADAttribute
	{
		public string Name;

		public ArrayList Values;

		public ADAttribute()
		{
			this.Values = new ArrayList();
		}

		public override int GetHashCode()
		{
			return this.Name.GetHashCode();
		}
	}
}