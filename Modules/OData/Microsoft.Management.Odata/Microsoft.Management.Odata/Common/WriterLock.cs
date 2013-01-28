using System;
using System.Threading;

namespace Microsoft.Management.Odata.Common
{
	internal class WriterLock : IDisposable
	{
		private ReaderWriterLockSlim readerWriterLock;

		public WriterLock(ReaderWriterLockSlim readerWriterLock)
		{
			this.readerWriterLock = readerWriterLock;
			this.readerWriterLock.EnterWriteLock();
		}

		public void Dispose()
		{
			this.readerWriterLock.ExitWriteLock();
			GC.SuppressFinalize(this);
		}
	}
}