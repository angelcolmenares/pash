using System;
using System.Text;

namespace Microsoft.Management.Odata.Common
{
	internal class SharedItemStore<TItem, TBorrowerId> : Store<TItem, TBorrowerId>
	{
		private DictionaryCache<string, TItem> cache;

		public SharedItemStore(IItemFactory<TItem, TBorrowerId> factory, int timeout, int cacheSize) : base(factory, timeout)
		{
			this.cache = new DictionaryCache<string, TItem>(cacheSize);
			base.RegisterCache(this.cache);
		}

		public override Envelope<TItem, TBorrowerId> Borrow(TBorrowerId borrower, string membershipId)
		{
			TItem tItem = default(TItem);
			this.Trace("Shared Item store Borrow");
			if (this.cache.TryLockKey(membershipId, out tItem))
			{
				TraceHelper.Current.SharedStoreTookFromCache(borrower.ToString(), typeof(TItem).ToString());
			}
			else
			{
				TraceHelper.Current.SharedStoreCreatedNew(borrower.ToString(), typeof(TItem).ToString());
				TItem tItem1 = base.Factory.Create(borrower, membershipId);
				try
				{
					this.cache.AddOrLockKey(membershipId, tItem1, out tItem);
				}
				catch (OverflowException overflowException1)
				{
					OverflowException overflowException = overflowException1;
					overflowException.Trace("Ignoring exception");
				}
			}
			return new Envelope<TItem, TBorrowerId>(tItem, membershipId, this, borrower);
		}

		public override void Return(Envelope<TItem, TBorrowerId> envelope)
		{
			this.Trace("Shared Item store Return");
			this.cache.TryUnlockKey(envelope.MembershipId);
		}

		internal DictionaryCache<string, TItem> TestHookGetDictionaryCache()
		{
			return this.cache;
		}

		public override void Trace(string message)
		{
			if (TraceHelper.IsEnabled(5))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(message);
				stringBuilder.AppendLine(string.Concat("Item type ", typeof(TItem).Name, "\nBorrower Id type ", typeof(TBorrowerId).Name));
				stringBuilder = this.cache.ToTraceMessage("\nCache", stringBuilder);
				TraceHelper.Current.DebugMessage(stringBuilder.ToString());
			}
		}
	}
}