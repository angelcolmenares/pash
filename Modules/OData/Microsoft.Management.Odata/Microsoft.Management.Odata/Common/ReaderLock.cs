using System;
using System.Threading;

namespace Microsoft.Management.Odata.Common
{
	internal class ReaderLock : IDisposable
	{
		private ReaderWriterLockSlim readerWriterLock;

		public ReaderLock(ReaderWriterLockSlim readerWriterLock)
		{
			this.readerWriterLock = readerWriterLock;
			this.readerWriterLock.EnterReadLock();
		}

		public void Dispose()
		{
			this.readerWriterLock.ExitReadLock();
			GC.SuppressFinalize(this);
		}
	}
}