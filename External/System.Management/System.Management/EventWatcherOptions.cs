using System;
using System.Runtime;

namespace System.Management
{
	public class EventWatcherOptions : ManagementOptions
	{
		private int blockSize;

		public int BlockSize
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.blockSize;
			}
			set
			{
				this.blockSize = value;
				base.FireIdentifierChanged();
			}
		}

		public EventWatcherOptions() : this(null, ManagementOptions.InfiniteTimeout, 1)
		{
		}

		public EventWatcherOptions(ManagementNamedValueCollection context, TimeSpan timeout, int blockSize) : base(context, timeout)
		{
			this.blockSize = 1;
			base.Flags = 48;
			this.BlockSize = blockSize;
		}

		public override object Clone()
		{
			ManagementNamedValueCollection managementNamedValueCollection = null;
			if (base.Context != null)
			{
				managementNamedValueCollection = base.Context.Clone();
			}
			return new EventWatcherOptions(managementNamedValueCollection, base.Timeout, this.blockSize);
		}
	}
}