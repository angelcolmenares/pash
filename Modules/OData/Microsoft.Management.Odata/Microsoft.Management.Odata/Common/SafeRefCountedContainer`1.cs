using System;

namespace Microsoft.Management.Odata.Common
{
	internal class SafeRefCountedContainer<TItem>
	{
		private object syncObject;

		private TItem instance;

		private int refCount;

		public TItem Instance
		{
			get
			{
				TItem tItem;
				lock (this.syncObject)
				{
					if (this.refCount != 0)
					{
						tItem = this.instance;
					}
					else
					{
						throw new ObjectDisposedException("SafeRefCountedContainer");
					}
				}
				return tItem;
			}
			private set
			{
				this.instance = value;
			}
		}

		public SafeRefCountedContainer(TItem instance)
		{
			this.syncObject = new object();
			this.refCount = 1;
			this.Instance = instance;
		}

		public void AddRef()
		{
			lock (this.syncObject)
			{
				SafeRefCountedContainer<TItem> safeRefCountedContainer = this;
				safeRefCountedContainer.refCount = safeRefCountedContainer.refCount + 1;
			}
		}

		public void Release()
		{
			this.Release(true);
		}

		private void Release(bool disposeManagedResources)
		{
			lock (this.syncObject)
			{
				if (this.refCount > 0)
				{
					SafeRefCountedContainer<TItem> safeRefCountedContainer = this;
					safeRefCountedContainer.refCount = safeRefCountedContainer.refCount - 1;
					if (this.refCount == 0)
					{
						IDisposable disposable = (object)this.instance as IDisposable;
						if (disposable != null)
						{
							disposable.Dispose();
						}
						GC.SuppressFinalize(this);
					}
				}
			}
		}

		internal int TestHookGetRefCount()
		{
			return this.refCount;
		}
	}
}