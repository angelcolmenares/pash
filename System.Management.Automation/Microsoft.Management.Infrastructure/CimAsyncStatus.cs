using System;

namespace Microsoft.Management.Infrastructure.Generic
{
	public class CimAsyncStatus : IObservable<object>
	{
		private readonly IObservable<object> _wrappedObservable;

		internal CimAsyncStatus(IObservable<object> wrappedObservable)
		{
			this._wrappedObservable = wrappedObservable;
		}

		public IDisposable Subscribe(IObserver<object> observer)
		{
			if (observer != null)
			{
				return this._wrappedObservable.Subscribe(observer);
			}
			else
			{
				throw new ArgumentNullException("observer");
			}
		}
	}
}