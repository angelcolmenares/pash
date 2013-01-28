using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal sealed class ConvertingObservable<TWrappedType, TTargetType> : IObservable<TTargetType>
	where TTargetType : class
	{
		private readonly IObservable<TWrappedType> _wrappedObservable;

		internal ConvertingObservable(IObservable<TWrappedType> wrappedObservable)
		{
			this._wrappedObservable = wrappedObservable;
		}

		public IDisposable Subscribe(IObserver<TTargetType> observer)
		{
			ConvertingObservable<TWrappedType, TTargetType>.ConvertingObserverProxy convertingObserverProxy = new ConvertingObservable<TWrappedType, TTargetType>.ConvertingObserverProxy(observer);
			return this._wrappedObservable.Subscribe(convertingObserverProxy);
		}

		private class ConvertingObserverProxy : IObserver<TWrappedType>
		{
			private readonly IObserver<TTargetType> _targetObserver;

			internal ConvertingObserverProxy(IObserver<TTargetType> targetObserver)
			{
				this._targetObserver = targetObserver;
			}

			public void OnCompleted()
			{
				this._targetObserver.OnCompleted();
			}

			public void OnError(Exception error)
			{
				this._targetObserver.OnError(error);
			}

			public void OnNext(TWrappedType value)
			{
				TTargetType tTargetType = (TTargetType)((object)value as TTargetType);
				this._targetObserver.OnNext(tTargetType);
			}
		}
	}
}