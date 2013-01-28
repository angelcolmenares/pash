using System;

namespace Microsoft.Management.Odata.Common
{
	internal abstract class Store<TItem, TBorrowerId> : CacheController
	{
		protected IItemFactory<TItem, TBorrowerId> Factory
		{
			get; private set;
		}

		public Store(IItemFactory<TItem, TBorrowerId> factory, int timeout) : base(timeout)
		{
			this.Factory = factory;
		}

		public abstract Envelope<TItem, TBorrowerId> Borrow(TBorrowerId borrower, string membershipId);

		public abstract void Return(Envelope<TItem, TBorrowerId> item);

		public abstract void Trace(string message);
	}
}