using System;
using System.Text;

namespace Microsoft.Management.Odata.Common
{
	internal abstract class Cache
	{
		public int MaxCacheSize
		{
			get;
			private set;
		}

		public Cache(int maxCacheSize)
		{
			this.MaxCacheSize = maxCacheSize;
		}

		public abstract void DoCleanup(DateTime checkpoint);

		public abstract StringBuilder ToTraceMessage(string message, StringBuilder builder);
	}
}