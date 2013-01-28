using System;

namespace System.DirectoryServices.Protocols
{
	internal class ADExtenMatchFilter
	{
		public string Name;

		public ADValue Value;

		public bool DNAttributes;

		public string MatchingRule;

		public ADExtenMatchFilter()
		{
			this.Value = null;
			this.DNAttributes = false;
		}
	}
}