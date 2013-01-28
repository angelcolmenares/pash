using System;

namespace Microsoft.Management.Infrastructure.Internal.Operations
{
	internal sealed class EmptyDisposable : IDisposable
	{
		private readonly static Lazy<EmptyDisposable> lazySingleton;

		internal static EmptyDisposable Singleton
		{
			get
			{
				return EmptyDisposable.lazySingleton.Value;
			}
		}

		static EmptyDisposable()
		{
			EmptyDisposable.lazySingleton = new Lazy<EmptyDisposable>(() => new EmptyDisposable(), true);
		}

		private EmptyDisposable()
		{
		}

		public void Dispose()
		{
		}
	}
}