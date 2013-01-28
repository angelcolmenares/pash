using Microsoft.Management.Odata.Common;
using System;

namespace Microsoft.Management.Odata.GenericInvoke
{
	internal class PipelineInvocationCollection : DictionaryCache<Guid, PipelineInvocation>
	{
		public PipelineInvocationCollection() : base(0x7fffffff)
		{
		}

		protected override bool CanItemBeRemoved(CacheEntry<PipelineInvocation> cachedItem, DateTime checkPoint)
		{
			if (cachedItem.IsLocked || !(cachedItem.Value.ExpirationTime <= DateTimeHelper.UtcNow))
			{
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}