using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime;
using System.Security;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	internal class FindResultEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		private ResultSet resultSet;

		private bool beforeStart;

		private bool endReached;

		private bool disposed;

		public T Current
		{
			[SecurityCritical]
			get
			{
				this.CheckDisposed();
				if (this.beforeStart || this.endReached || this.resultSet == null)
				{
					throw new InvalidOperationException(StringResources.FindResultEnumInvalidPos);
				}
				else
				{
					return (T)this.resultSet.CurrentAsPrincipal;
				}
			}
		}

		object System.Collections.IEnumerator.Current
		{
			[SecurityCritical]
			get
			{
				return this.Current;
			}
		}

		internal FindResultEnumerator(ResultSet resultSet)
		{
			this.beforeStart = true;
			this.resultSet = resultSet;
		}

		private void CheckDisposed()
		{
			if (!this.disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException("FindResultEnumerator");
			}
		}

		public void Dispose()
		{
			this.disposed = true;
		}

		[SecurityCritical]
		public bool MoveNext()
		{
			bool flag;
			this.CheckDisposed();
			if (!this.endReached)
			{
				if (this.resultSet != null)
				{
					lock (this.resultSet)
					{
						if (this.beforeStart)
						{
							this.beforeStart = false;
							this.resultSet.Reset();
						}
						flag = this.resultSet.MoveNext();
					}
					if (!flag)
					{
						this.endReached = true;
					}
					return flag;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		[SecurityCritical]
		public void Reset()
		{
			this.CheckDisposed();
			this.endReached = false;
			this.beforeStart = true;
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		bool System.Collections.IEnumerator.MoveNext()
		{
			return this.MoveNext();
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Collections.IEnumerator.Reset()
		{
			this.Reset();
		}
	}
}