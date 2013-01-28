using System;

namespace Microsoft.Management.Infrastructure.Generic
{
	public class CimAsyncMultipleResults<T> : IObservable<T>
	{
		private readonly IObservable<T> _wrappedObservable;

		internal CimAsyncMultipleResults(IObservable<T> wrappedObservable)
		{
			this._wrappedObservable = wrappedObservable;
		}

		public IDisposable Subscribe(IObserver<T> observer)
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