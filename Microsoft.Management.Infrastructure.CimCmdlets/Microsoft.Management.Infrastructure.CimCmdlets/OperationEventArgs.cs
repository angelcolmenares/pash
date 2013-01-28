using System;

namespace Microsoft.Management.Infrastructure.CimCmdlets
{
	internal sealed class OperationEventArgs : EventArgs
	{
		public readonly IDisposable operationCancellation;

		public readonly IObservable<object> operation;

		public readonly bool success;

		public OperationEventArgs(IDisposable operationCancellation, IObservable<object> operation, bool theSuccess)
		{
			this.operationCancellation = operationCancellation;
			this.operation = operation;
			this.success = theSuccess;
		}
	}
}