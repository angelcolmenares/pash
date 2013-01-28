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
	internal class ValueCollectionEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		private TrackedCollectionEnumerator<T> inner;

		public T Current
		{
			get
			{
				return this.inner.Current;
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

		internal ValueCollectionEnumerator(TrackedCollection<T> trackingList, List<TrackedCollection<T>.ValueEl> combinedValues)
		{
			this.inner = new TrackedCollectionEnumerator<T>("ValueCollectionEnumerator", trackingList, combinedValues);
		}

		public void Dispose()
		{
			this.inner.Dispose();
		}

		public bool MoveNext()
		{
			return this.inner.MoveNext();
		}

		public void Reset()
		{
			this.inner.Reset();
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