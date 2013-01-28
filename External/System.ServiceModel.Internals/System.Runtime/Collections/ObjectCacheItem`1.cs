using System;
using System.Runtime;

namespace System.Runtime.Collections
{
	internal abstract class ObjectCacheItem<T>
	where T : class
	{
		public abstract T Value
		{
			get;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ObjectCacheItem()
		{
		}

		public abstract void ReleaseReference();

		public abstract bool TryAddReference();
	}
}