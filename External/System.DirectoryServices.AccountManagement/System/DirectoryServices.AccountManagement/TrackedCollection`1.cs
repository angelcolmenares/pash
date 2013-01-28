using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;

namespace System.DirectoryServices.AccountManagement
{
	internal class TrackedCollection<T> : ICollection<T>, ICollection, IEnumerable<T>, IEnumerable
	{
		internal List<TrackedCollection<T>.ValueEl> combinedValues;

		internal List<T> removedValues;

		private DateTime lastChange;

		internal bool Changed
		{
			get
			{
				bool flag;
				if (this.removedValues.Count <= 0)
				{
					List<TrackedCollection<T>.ValueEl>.Enumerator enumerator = this.combinedValues.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							TrackedCollection<T>.ValueEl current = enumerator.Current;
							if (!current.isInserted)
							{
								T left = current.originalValue.Left;
								if (left.Equals(current.originalValue.Right))
								{
									continue;
								}
								flag = true;
								return flag;
							}
							else
							{
								flag = true;
								return flag;
							}
						}
						return false;
					}
					finally
					{
						enumerator.Dispose();
					}
					return flag;
				}
				else
				{
					return true;
				}
			}
		}

		internal List<Pair<T, T>> ChangedValues
		{
			get
			{
				List<Pair<T, T>> pairs = new List<Pair<T, T>>();
				foreach (TrackedCollection<T>.ValueEl combinedValue in this.combinedValues)
				{
					if (combinedValue.isInserted)
					{
						continue;
					}
					T left = combinedValue.originalValue.Left;
					if (left.Equals(combinedValue.originalValue.Right))
					{
						continue;
					}
					pairs.Add(new Pair<T, T>(combinedValue.originalValue.Left, combinedValue.originalValue.Right));
				}
				return pairs;
			}
		}

		public int Count
		{
			get
			{
				return this.combinedValues.Count;
			}
		}

		internal List<T> Inserted
		{
			get
			{
				List<T> ts = new List<T>();
				foreach (TrackedCollection<T>.ValueEl combinedValue in this.combinedValues)
				{
					if (!combinedValue.isInserted)
					{
						continue;
					}
					ts.Add(combinedValue.insertedValue);
				}
				return ts;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		internal DateTime LastChange
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.lastChange;
			}
		}

		internal List<T> Removed
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.removedValues;
			}
		}

		int System.Collections.ICollection.Count
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.Count;
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				return this;
			}
		}

		public TrackedCollection()
		{
			this.combinedValues = new List<TrackedCollection<T>.ValueEl>();
			this.removedValues = new List<T>();
			this.lastChange = DateTime.UtcNow;
		}

		public void Add(T o)
		{
			this.MarkChange();
			TrackedCollection<T>.ValueEl valueEl = new TrackedCollection<T>.ValueEl();
			valueEl.isInserted = true;
			valueEl.insertedValue = o;
			this.combinedValues.Add(valueEl);
		}

		public void Clear()
		{
			this.MarkChange();
			foreach (TrackedCollection<T>.ValueEl combinedValue in this.combinedValues)
			{
				if (combinedValue.isInserted)
				{
					continue;
				}
				this.removedValues.Add(combinedValue.originalValue.Left);
			}
			this.combinedValues.Clear();
		}

		public bool Contains(T value)
		{
			bool flag;
			List<TrackedCollection<T>.ValueEl>.Enumerator enumerator = this.combinedValues.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TrackedCollection<T>.ValueEl current = enumerator.Current;
					T currentValue = current.GetCurrentValue();
					if (!currentValue.Equals(value))
					{
						continue;
					}
					flag = true;
					return flag;
				}
				return false;
			}
			finally
			{
				enumerator.Dispose();
			}
			return flag;
		}

		public void CopyTo(T[] array, int index)
		{
			this.CopyTo(array, index);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new TrackedCollectionEnumerator<T>("TrackedCollectionEnumerator", this, this.combinedValues);
		}

		internal void MarkChange()
		{
			this.lastChange = DateTime.UtcNow;
		}

		public bool Remove(T value)
		{
			bool flag;
			this.MarkChange();
			List<TrackedCollection<T>.ValueEl>.Enumerator enumerator = this.combinedValues.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TrackedCollection<T>.ValueEl current = enumerator.Current;
					if (!current.isInserted || !current.insertedValue.Equals(value))
					{
						if (current.isInserted)
						{
							continue;
						}
						T right = current.originalValue.Right;
						if (!right.Equals(value))
						{
							continue;
						}
						this.combinedValues.Remove(current);
						this.removedValues.Add(current.originalValue.Left);
						flag = true;
						return flag;
					}
					else
					{
						this.combinedValues.Remove(current);
						flag = true;
						return flag;
					}
				}
				return false;
			}
			finally
			{
				enumerator.Dispose();
			}
			return flag;
		}

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			if (index >= 0)
			{
				if (array != null)
				{
					if (array.Rank == 1)
					{
						if (index < array.GetLength(0))
						{
							if (array.GetLength(0) - index >= this.combinedValues.Count)
							{
								foreach (TrackedCollection<T>.ValueEl combinedValue in this.combinedValues)
								{
									array.SetValue(combinedValue.GetCurrentValue(), index);
									index++;
								}
								return;
							}
							else
							{
								throw new ArgumentException(StringResources.TrackedCollectionArrayTooSmall);
							}
						}
						else
						{
							throw new ArgumentException(StringResources.TrackedCollectionIndexNotInArray);
						}
					}
					else
					{
						throw new ArgumentException(StringResources.TrackedCollectionNotOneDimensional);
					}
				}
				else
				{
					throw new ArgumentNullException("array");
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException("index");
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		internal class ValueEl
		{
			public bool isInserted;

			public T insertedValue;

			public Pair<T, T> originalValue;

			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			public ValueEl()
			{
			}

			public T GetCurrentValue()
			{
				if (!this.isInserted)
				{
					return this.originalValue.Right;
				}
				else
				{
					return this.insertedValue;
				}
			}
		}
	}
}