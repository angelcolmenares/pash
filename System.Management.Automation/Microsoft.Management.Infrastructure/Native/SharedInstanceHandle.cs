using Microsoft.Management.Infrastructure.Native;
using System;

namespace Microsoft.Management.Infrastructure.Internal
{
	internal class SharedInstanceHandle
	{
		private readonly InstanceHandle _handle;

		private readonly SharedInstanceHandle _parent;

		private readonly object _numberOfReferencesLock;

		private int _numberOfReferences;

		internal InstanceHandle Handle
		{
			get
			{
				lock (this._numberOfReferencesLock)
				{
					if (this._numberOfReferences == 0)
					{
						throw new ObjectDisposedException(this.ToString());
					}
				}
				return this._handle;
			}
		}

		internal SharedInstanceHandle(InstanceHandle handle)
		{
			this._numberOfReferencesLock = new object();
			this._numberOfReferences = 1;
			this._handle = handle;
		}

		internal SharedInstanceHandle(InstanceHandle handle, SharedInstanceHandle parent) : this(handle)
		{
			this._parent = parent;
			if (this._parent != null)
			{
				this._parent.AddRef();
			}
		}

		internal void AddRef()
		{
			lock (this._numberOfReferencesLock)
			{
				if (this._numberOfReferences != 0)
				{
					SharedInstanceHandle sharedInstanceHandle = this;
					sharedInstanceHandle._numberOfReferences = sharedInstanceHandle._numberOfReferences + 1;
				}
				else
				{
					throw new ObjectDisposedException(this.ToString());
				}
			}
		}

		internal void Release()
		{
			lock (this._numberOfReferencesLock)
			{
				SharedInstanceHandle sharedInstanceHandle = this;
				sharedInstanceHandle._numberOfReferences = sharedInstanceHandle._numberOfReferences - 1;
				if (this._numberOfReferences == 0)
				{
					this._handle.Dispose();
					if (this._parent != null)
					{
						this._parent.Release();
					}
				}
			}
		}
	}
}