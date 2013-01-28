using System;
using System.Collections;

namespace System.DirectoryServices.Protocols
{
	internal class ADFilter
	{
		public ADFilter.FilterType Type;

		public ADFilter.FilterContent Filter;

		public ADFilter()
		{
			this.Filter = new ADFilter.FilterContent();
		}

		public struct FilterContent
		{
			public ArrayList And;

			public ArrayList Or;

			public ADFilter Not;

			public ADAttribute EqualityMatch;

			public ADSubstringFilter Substrings;

			public ADAttribute GreaterOrEqual;

			public ADAttribute LessOrEqual;

			public string Present;

			public ADAttribute ApproxMatch;

			public ADExtenMatchFilter ExtensibleMatch;

		}

		public enum FilterType
		{
			And,
			Or,
			Not,
			EqualityMatch,
			Substrings,
			GreaterOrEqual,
			LessOrEqual,
			Present,
			ApproxMatch,
			ExtensibleMatch
		}
	}
}