using System;
using System.Collections.ObjectModel;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[Serializable]
	internal class NonNullableCollection<T> : Collection<T>
	{
		public NonNullableCollection()
		{
		}

		protected override void InsertItem(int index, T item)
		{
			Utility.VerifyNonNullArgument("item", item);
			base.InsertItem(index, item);
		}

		protected override void SetItem(int index, T item)
		{
			Utility.VerifyNonNullArgument("item", item);
			base.SetItem(index, item);
		}
	}
}