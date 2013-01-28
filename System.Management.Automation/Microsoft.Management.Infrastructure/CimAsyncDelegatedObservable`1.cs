using System;
using System.Threading;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal class CimAsyncDelegatedObservable<T> : IObservable<T>
	{
		private readonly Action<IObserver<T>> _subscribe;

		internal CimAsyncDelegatedObservable(Action<IObserver<T>> subscribe)
		{
			this._subscribe = subscribe;
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			if (observer != null)
			{
				ThreadPool.QueueUserWorkItem((object param0) => observer.OnNext((T)param0));
				return EmptyDisposable.Singleton;
			}
			else
			{
				throw new ArgumentNullException("observer");
			}
		}
	}
}