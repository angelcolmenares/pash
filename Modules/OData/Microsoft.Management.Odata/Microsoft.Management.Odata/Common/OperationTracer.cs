using System;

namespace Microsoft.Management.Odata.Common
{
	internal class OperationTracer : IDisposable
	{
		private string parameter;

		private Action<string> end;

		public OperationTracer(string operation) : this(new Action<string>(TraceHelper.Current.BeginOperation0), new Action<string>(TraceHelper.Current.EndOperation), operation)
		{
		}

		public OperationTracer(Action<string> start, Action<string> end, string parameter)
		{
			this.parameter = parameter;
			start(parameter);
			this.end = end;
		}

		public void Dispose()
		{
			this.end(this.parameter);
			GC.SuppressFinalize(this);
		}
	}
}