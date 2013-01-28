using System;

namespace Microsoft.Management.Odata.Common
{
	internal class Envelope<TItem, TBorrowerId> : IDisposable
	{
		public TBorrowerId Borrower
		{
			get; private set;
		}

		public TItem Item
		{
			get; private set;
		}

		public string MembershipId
		{
			get; private set;
		}

		public Store<TItem, TBorrowerId> Store
		{
			get; private set;
		}

		public Envelope(TItem item, string membershipId, Store<TItem, TBorrowerId> store, TBorrowerId borrower)
		{
			this.Item = item;
			this.MembershipId = membershipId;
			this.Store = store;
			this.Borrower = borrower;
		}

		public void Dispose()
		{
			this.Store.Return(this);
			GC.SuppressFinalize(this);
		}
	}
}