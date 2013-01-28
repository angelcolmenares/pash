using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.Common
{
	internal class BoundedResetList<TItem>
	{
		private BoundedInteger listSize;

		private List<TItem> list;

		public BoundedResetList(int listSize)
		{
			this.listSize = new BoundedInteger(listSize, 1, 0x7fffffff);
			this.list = new List<TItem>();
		}

		public void Add(TItem dataItem)
		{
			this.list.Add(dataItem);
			if (this.list.Count >= this.listSize.Value)
			{
				this.Reset();
			}
		}

		public void Reset()
		{
			if (this.PreResetEventHandler != null)
			{
				this.PreResetEventHandler(this, new BoundedResetList<TItem>.PreResetEventArgs(this.list));
			}
			this.list = new List<TItem>();
		}

		internal List<TItem> TestHookGetList()
		{
			return this.list;
		}

		public event EventHandler<BoundedResetList<TItem>.PreResetEventArgs> PreResetEventHandler;
		internal class PreResetEventArgs : EventArgs
		{
			public List<TItem> PreResetList
			{
				get;private set;
			}

			public PreResetEventArgs(List<TItem> preResetList)
			{
				this.PreResetList = preResetList;
			}
		}
	}
}