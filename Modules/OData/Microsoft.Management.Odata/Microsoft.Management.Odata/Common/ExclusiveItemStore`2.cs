using System;
using System.Text;

namespace Microsoft.Management.Odata.Common
{
	internal class ExclusiveItemStore<TItem, TBorrowerId> : Store<TItem, TBorrowerId>
	{
		private DictionaryCache<TBorrowerId, ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount> borrowersDictionary;

		private int borrowerAccountMaxCacheSize;

		public ExclusiveItemStore(IItemFactory<TItem, TBorrowerId> factory, int timeout, int maxCacheSize) : base(factory, timeout)
		{
			this.borrowersDictionary = new DictionaryCache<TBorrowerId, ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount>(0x7fffffff);
			base.RegisterCache(this.borrowersDictionary);
			this.borrowerAccountMaxCacheSize = maxCacheSize;
		}

		public override Envelope<TItem, TBorrowerId> Borrow(TBorrowerId borrower, string membershipId)
		{
			ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount borrowerAccount = null;
			TItem tItem = default(TItem);
			Envelope<TItem, TBorrowerId> envelope;
			this.Trace("Exclusive item store Borrow");
			if (!this.borrowersDictionary.TryLockKey(borrower, out borrowerAccount))
			{
				ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount borrowerAccount1 = new ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount(membershipId, this, this.borrowerAccountMaxCacheSize);
				try
				{
					this.borrowersDictionary.AddOrLockKey(borrower, borrowerAccount1, out borrowerAccount);
				}
				finally
				{
					this.borrowersDictionary.TryUnlockKey(borrower);
				}
			}
			else
			{
				try
				{
					if (borrowerAccount.MembershipId == membershipId)
					{
						if (borrowerAccount.IdleQueue.TryDequeue(out tItem))
						{
							TraceHelper.Current.ExclusiveStoreTookFromCache(borrower.ToString(), typeof(TItem).ToString());
							envelope = new Envelope<TItem, TBorrowerId>(tItem, membershipId, this, borrower);
							return envelope;
						}
					}
					else
					{
						borrowerAccount.IdleQueue.Clear();
						borrowerAccount.MembershipId = membershipId;
					}
					TraceHelper.Current.ExclusiveStoreCreatedNew(borrower.ToString(), typeof(TItem).ToString());
					return new Envelope<TItem, TBorrowerId>(base.Factory.Create(borrower, membershipId), membershipId, this, borrower);
				}
				finally
				{
					this.borrowersDictionary.TryUnlockKey(borrower);
				}
				return envelope;
			}
			TraceHelper.Current.ExclusiveStoreCreatedNew(borrower.ToString(), typeof(TItem).ToString());
			return new Envelope<TItem, TBorrowerId>(base.Factory.Create(borrower, membershipId), membershipId, this, borrower);
		}

		public override void Return(Envelope<TItem, TBorrowerId> envelope)
		{
			ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount borrowerAccount = null;
			this.Trace("Exclusive item store Return");
			try
			{
				if (!this.borrowersDictionary.TryLockKey(envelope.Borrower, out borrowerAccount))
				{
					ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount borrowerAccount1 = new ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount(envelope.MembershipId, this, this.borrowerAccountMaxCacheSize);
					this.borrowersDictionary.AddOrLockKey(envelope.Borrower, borrowerAccount1, out borrowerAccount);
				}
				if (envelope.MembershipId == borrowerAccount.MembershipId)
				{
					try
					{
						borrowerAccount.IdleQueue.Enqueue(envelope.Item);
					}
					catch (OverflowException overflowException1)
					{
						OverflowException overflowException = overflowException1;
						overflowException.Trace("Ignoring exception");
					}
				}
			}
			finally
			{
				this.borrowersDictionary.TryUnlockKey(envelope.Borrower);
			}
		}

		internal DictionaryCache<TBorrowerId, ExclusiveItemStore<TItem, TBorrowerId>.BorrowerAccount> TestHookGetBorrowersDictionary()
		{
			return this.borrowersDictionary;
		}

		public override void Trace(string message)
		{
			if (TraceHelper.IsEnabled(5))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(message);
				stringBuilder.AppendLine(string.Concat("Item type ", typeof(TItem).Name, "\nBorrower Id type ", typeof(TBorrowerId).Name));
				stringBuilder = this.borrowersDictionary.ToTraceMessage("\nBorrowers dictionary ", stringBuilder);
				TraceHelper.Current.DebugMessage(stringBuilder.ToString());
			}
		}

		internal class BorrowerAccount : IDisposable
		{
			private CacheController queueCacheController;

			private bool disposed;

			public QueueCache<TItem> IdleQueue
			{
				get;set;
			}

			public string MembershipId
			{
				get;set;
			}

			public BorrowerAccount(string membershipId, CacheController queueCacheController, int cacheSize)
			{
				this.MembershipId = membershipId;
				this.IdleQueue = new QueueCache<TItem>(cacheSize);
				this.queueCacheController = queueCacheController;
				this.queueCacheController.RegisterCache(this.IdleQueue);
			}

			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposeManagedResources)
			{
				if (!this.disposed && disposeManagedResources && this.queueCacheController != null)
				{
					this.queueCacheController.UnregisterCache(this.IdleQueue);
					this.queueCacheController = null;
				}
				this.disposed = true;
			}

			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Borrowers account");
				stringBuilder.AppendLine(string.Concat("Membership Id = ", this.MembershipId));
				stringBuilder = this.IdleQueue.ToTraceMessage("Idle queue cache", stringBuilder);
				return stringBuilder.ToString();
			}
		}
	}
}