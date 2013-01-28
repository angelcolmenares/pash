using System;

namespace Microsoft.Management.Odata.Common
{
	internal class OperationTracerWithTimeout : IDisposable
	{
		private DateTime startTime;

		private int timeoutInSec;

		private string parameter;

		private Action<string> end;

		private Action<string> timeoutEvent;

		public OperationTracerWithTimeout(Action<string> start, Action<string> end, string parameter, Action<string> timeoutEvent, int timeLimitInSec)
		{
			start(parameter);
			this.startTime = DateTimeHelper.Now;
			this.end = end;
			this.timeoutEvent = timeoutEvent;
			this.parameter = parameter;
			this.timeoutInSec = timeLimitInSec;
		}

		public void Dispose()
		{
			DateTime now = DateTimeHelper.Now;
			TimeSpan timeSpan = now - this.startTime;
			if (timeSpan.TotalSeconds > (double)this.timeoutInSec)
			{
				this.timeoutEvent(this.parameter);
			}
			this.end(this.parameter);
			GC.SuppressFinalize(this);
		}
	}
}