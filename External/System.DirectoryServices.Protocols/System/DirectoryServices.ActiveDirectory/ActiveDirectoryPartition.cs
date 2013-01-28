using System;
using System.DirectoryServices;
using System.Runtime;
using System.Security.Permissions;

namespace System.DirectoryServices.ActiveDirectory
{
	public abstract class ActiveDirectoryPartition : IDisposable
	{
		private bool disposed;

		internal string partitionName;

		internal DirectoryContext context;

		internal DirectoryEntryManager directoryEntryMgr;

		public string Name
		{
			get
			{
				this.CheckIfDisposed();
				return this.partitionName;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		protected ActiveDirectoryPartition()
		{
		}

		internal ActiveDirectoryPartition(DirectoryContext context, string name)
		{
			this.context = context;
			this.partitionName = name;
		}

		internal void CheckIfDisposed()
		{
			if (!this.disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.GetType().Name);
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					foreach (DirectoryEntry cachedDirectoryEntry in this.directoryEntryMgr.GetCachedDirectoryEntries())
					{
						cachedDirectoryEntry.Dispose();
					}
				}
				this.disposed = true;
			}
		}

		[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
		public abstract DirectoryEntry GetDirectoryEntry();

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public override string ToString()
		{
			return this.Name;
		}
	}
}