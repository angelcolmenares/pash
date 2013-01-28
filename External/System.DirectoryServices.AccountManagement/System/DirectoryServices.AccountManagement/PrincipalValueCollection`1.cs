using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime;
using System.Security;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
	public class PrincipalValueCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
	{
		private TrackedCollection<T> inner;

		internal bool Changed
		{
			get
			{
				return this.inner.Changed;
			}
		}

		internal List<Pair<T, T>> ChangedValues
		{
			get
			{
				return this.inner.ChangedValues;
			}
		}

		public int Count
		{
			get
			{
				return this.inner.Count;
			}
		}

		internal List<T> Inserted
		{
			get
			{
				return this.inner.Inserted;
			}
		}

		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return this.inner.IsSynchronized;
			}
		}

		public T this[int index]
		{
			get
			{
				if (index < 0 || index >= this.inner.combinedValues.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				else
				{
					TrackedCollection<T>.ValueEl item = this.inner.combinedValues[index];
					if (!item.isInserted)
					{
						return item.originalValue.Right;
					}
					else
					{
						return item.insertedValue;
					}
				}
			}
			set
			{
				this.inner.MarkChange();
				if (index < 0 || index >= this.inner.combinedValues.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				else
				{
					if (value != null)
					{
						TrackedCollection<T>.ValueEl item = this.inner.combinedValues[index];
						if (!item.isInserted)
						{
							item.originalValue.Right = value;
							return;
						}
						else
						{
							item.insertedValue = value;
							return;
						}
					}
					else
					{
						throw new ArgumentNullException("value");
					}
				}
			}
		}

		internal List<T> Removed
		{
			get
			{
				return this.inner.Removed;
			}
		}

		public object SyncRoot
		{
			get
			{
				return this;
			}
		}

		int System.Collections.ICollection.Count
		{
			get
			{
				return this.inner.Count;
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.IsSynchronized;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.SyncRoot;
			}
		}

		bool System.Collections.IList.IsFixedSize
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.IsFixedSize;
			}
		}

		bool System.Collections.IList.IsReadOnly
		{
			[SecurityCritical]
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.IsReadOnly;
			}
		}

		object System.Collections.IList.this[int index]
		{
			[SecurityCritical]
			get
			{
				return this[index];
			}
			[SecurityCritical]
			set
			{
				if (value != null)
				{
					this[index] = (T)value;
					return;
				}
				else
				{
					throw new ArgumentNullException("value");
				}
			}
		}

		internal PrincipalValueCollection()
		{
			this.inner = new TrackedCollection<T>();
		}

		public void Add(T value)
		{
			if (value != null)
			{
				this.inner.Add(value);
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Clear()
		{
			this.inner.Clear();
		}

		public bool Contains(T value)
		{
			if (value != null)
			{
				return this.inner.Contains(value);
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void CopyTo(T[] array, int index)
		{
			this.CopyTo(array, index);
		}

		[SecurityCritical]
		public IEnumerator<T> GetEnumerator()
		{
			return new ValueCollectionEnumerator<T>(this.inner, this.inner.combinedValues);
		}

		public int IndexOf(T value)
		{
			int num;
			if (value != null)
			{
				int num1 = 0;
				List<TrackedCollection<T>.ValueEl>.Enumerator enumerator = this.inner.combinedValues.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						TrackedCollection<T>.ValueEl current = enumerator.Current;
						if (!current.isInserted || !current.insertedValue.Equals(value))
						{
							if (!current.isInserted)
							{
								T right = current.originalValue.Right;
								if (right.Equals(value))
								{
									num = num1;
									return num;
								}
							}
							num1++;
						}
						else
						{
							num = num1;
							return num;
						}
					}
					return -1;
				}
				finally
				{
					enumerator.Dispose();
				}
				return num;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void Insert(int index, T value)
		{
			this.inner.MarkChange();
			if (value != null)
			{
				if (index < 0 || index > this.inner.combinedValues.Count)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				else
				{
					TrackedCollection<T>.ValueEl valueEl = new TrackedCollection<T>.ValueEl();
					valueEl.isInserted = true;
					valueEl.insertedValue = value;
					this.inner.combinedValues.Insert(index, valueEl);
					return;
				}
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		internal void Load(List<T> values)
		{
			this.inner.combinedValues.Clear();
			this.inner.removedValues.Clear();
			foreach (T value in values)
			{
				TrackedCollection<T>.ValueEl valueEl = new TrackedCollection<T>.ValueEl();
				valueEl.isInserted = false;
				valueEl.originalValue = new Pair<T, T>(value, value);
				this.inner.combinedValues.Add(valueEl);
			}
		}

		public bool Remove(T value)
		{
			if (value != null)
			{
				return this.inner.Remove(value);
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		public void RemoveAt(int index)
		{
			this.inner.MarkChange();
			if (index < 0 || index >= this.inner.combinedValues.Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			else
			{
				TrackedCollection<T>.ValueEl item = this.inner.combinedValues[index];
				if (!item.isInserted)
				{
					Pair<T, T> pair = this.inner.combinedValues[index].originalValue;
					this.inner.combinedValues.RemoveAt(index);
					this.inner.removedValues.Add(pair.Left);
					return;
				}
				else
				{
					this.inner.combinedValues.RemoveAt(index);
					return;
				}
			}
		}

		internal void ResetTracking()
		{
			this.inner.removedValues.Clear();
			foreach (TrackedCollection<T>.ValueEl combinedValue in this.inner.combinedValues)
			{
				if (!combinedValue.isInserted)
				{
					Pair<T, T> right = combinedValue.originalValue;
					T left = right.Left;
					if (left.Equals(right.Right))
					{
						continue;
					}
					right.Left = right.Right;
				}
				else
				{
					combinedValue.isInserted = false;
					combinedValue.originalValue = new Pair<T, T>(combinedValue.insertedValue, combinedValue.insertedValue);
				}
			}
		}

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			this.inner.CopyTo(array, index);
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		[SecurityCritical]
		int System.Collections.IList.Add(object value)
		{
			if (value != null)
			{
				this.inner.Add((T)value);
				return this.Count;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Collections.IList.Clear()
		{
			this.Clear();
		}

		bool System.Collections.IList.Contains(object value)
		{
			if (value != null)
			{
				return this.inner.Contains((T)value);
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		[SecurityCritical]
		int System.Collections.IList.IndexOf(object value)
		{
			if (value != null)
			{
				return this.IndexOf((T)value);
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		[SecurityCritical]
		void System.Collections.IList.Insert(int index, object value)
		{
			if (value != null)
			{
				this.Insert(index, (T)value);
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		void System.Collections.IList.Remove(object value)
		{
			if (value != null)
			{
				this.inner.Remove((T)value);
				return;
			}
			else
			{
				throw new ArgumentNullException("value");
			}
		}

		[SecurityCritical]
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		void System.Collections.IList.RemoveAt(int index)
		{
			this.RemoveAt(index);
		}
	}
}