using System.Collections.ObjectModel;

namespace System.Runtime
{
	internal class ReadOnlyKeyedCollection<TKey, TValue> : ReadOnlyCollection<TValue>
	{
		private KeyedCollection<TKey, TValue> innerCollection;

		public TValue this[TKey key]
		{
			get
			{
				return this.innerCollection[key];
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ReadOnlyKeyedCollection(KeyedCollection<TKey, TValue> innerCollection) : base(innerCollection)
		{
			this.innerCollection = innerCollection;
		}
	}
}