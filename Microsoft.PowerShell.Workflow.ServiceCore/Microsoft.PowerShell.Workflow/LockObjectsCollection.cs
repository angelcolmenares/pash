using System;
using System.Collections.Generic;

namespace Microsoft.PowerShell.Workflow
{
	internal class LockObjectsCollection
	{
		private object syncLock;

		private Dictionary<Guid, object> lockObjects;

		public LockObjectsCollection()
		{
			this.syncLock = new object();
			this.lockObjects = new Dictionary<Guid, object>();
		}

		internal object GetLockObject(Guid id)
		{
			object item;
			lock (this.syncLock)
			{
				if (!this.lockObjects.ContainsKey(id))
				{
					this.lockObjects.Add(id, new object());
				}
				item = this.lockObjects[id];
			}
			return item;
		}

		internal void RemoveLockObject(Guid id)
		{
			if (this.lockObjects.ContainsKey(id))
			{
				lock (this.syncLock)
				{
					if (this.lockObjects.ContainsKey(id))
					{
						this.lockObjects.Remove(id);
					}
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}