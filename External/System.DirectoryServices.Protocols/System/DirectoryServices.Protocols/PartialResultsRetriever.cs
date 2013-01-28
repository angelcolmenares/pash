using System;
using System.Threading;

namespace System.DirectoryServices.Protocols
{
	internal class PartialResultsRetriever
	{
		private ManualResetEvent workThreadWaitHandle;

		private Thread oThread;

		private LdapPartialResultsProcessor processor;

		internal PartialResultsRetriever(ManualResetEvent eventHandle, LdapPartialResultsProcessor processor)
		{
			this.workThreadWaitHandle = eventHandle;
			this.processor = processor;
			this.oThread = new Thread(new ThreadStart(this.ThreadRoutine));
			this.oThread.IsBackground = true;
			this.oThread.Start();
		}

		private void ThreadRoutine()
		{
			while (true)
			{
				this.workThreadWaitHandle.WaitOne();
				try
				{
					this.processor.RetrievingSearchResults();
				}
				catch (Exception exception)
				{
				}
				Thread.Sleep(250);
			}
		}
	}
}