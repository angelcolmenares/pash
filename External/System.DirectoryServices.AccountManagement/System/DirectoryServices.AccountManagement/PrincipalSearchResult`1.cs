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
	public class PrincipalSearchResult<T> : IEnumerable<T>, IEnumerable, IDisposable
	{
		private ResultSet resultSet;

		private bool disposed;

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		internal PrincipalSearchResult(ResultSet resultSet)
		{
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
				throw new ObjectDisposedException("PrincipalSearchResult");
			}
		}

		public void Dispose()
		{
			if (!this.disposed)
			{
				if (this.resultSet != null)
				{
					lock (this.resultSet)
					{
						this.resultSet.Dispose();
					}
				}
				this.disposed = true;
				GC.SuppressFinalize(this);
			}
		}

		[SecurityCritical]
		public IEnumerator<T> GetEnumerator()
		{
			this.CheckDisposed();
			return new FindResultEnumerator<T>(this.resultSet);
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}