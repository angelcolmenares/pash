using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;

namespace System.DirectoryServices.AccountManagement
{
	internal class TrackedCollectionEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		private bool disposed;

		private string outerClassName;

		private List<TrackedCollection<T>.ValueEl> combinedValues;

		private T current;

		private IEnumerator enumerator;

		private bool endReached;

		private DateTime creationTime;

		private TrackedCollection<T> trackedCollection;

		public T Current
		{
			get
			{
				this.CheckDisposed();
				if (this.endReached || this.enumerator == null)
				{
					throw new InvalidOperationException(StringResources.TrackedCollectionEnumInvalidPos);
				}
				else
				{
					return this.current;
				}
			}
		}

		object System.Collections.IEnumerator.Current
		{
			get
			{
				return this.Current;
			}
		}

		internal TrackedCollectionEnumerator(string outerClassName, TrackedCollection<T> trackedCollection, List<TrackedCollection<T>.ValueEl> combinedValues)
		{
			this.creationTime = DateTime.UtcNow;
			this.outerClassName = outerClassName;
			this.trackedCollection = trackedCollection;
			this.combinedValues = combinedValues;
		}

		private void CheckChanged()
		{
			if (this.trackedCollection.LastChange <= this.creationTime)
			{
				return;
			}
			else
			{
				throw new InvalidOperationException(StringResources.TrackedCollectionEnumHasChanged);
			}
		}

		private void CheckDisposed()
		{
			if (!this.disposed)
			{
				return;
			}
			else
			{
				throw new ObjectDisposedException(this.outerClassName);
			}
		}

		public void Dispose()
		{
			this.disposed = true;
		}

		public bool MoveNext()
		{
			this.CheckDisposed();
			this.CheckChanged();
			if (!this.endReached)
			{
				if (this.enumerator == null)
				{
					this.enumerator = this.combinedValues.GetEnumerator();
				}
				bool flag = this.enumerator.MoveNext();
				if (!flag)
				{
					this.endReached = true;
				}
				else
				{
					TrackedCollection<T>.ValueEl current = (TrackedCollection<T>.ValueEl)this.enumerator.Current;
					if (!current.isInserted)
					{
						this.current = current.originalValue.Right;
					}
					else
					{
						this.current = current.insertedValue;
					}
				}
				return flag;
			}
			else
			{
				return false;
			}
		}

		public void Reset()
		{
			this.CheckDisposed();
			this.CheckChanged();
			this.endReached = false;
			this.enumerator = null;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		bool System.Collections.IEnumerator.MoveNext()
		{
			return this.MoveNext();
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Collections.IEnumerator.Reset()
		{
			this.Reset();
		}
	}
}