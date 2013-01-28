using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Internal
{
	internal abstract class VersioningList<TElement> : IEnumerable<TElement>, IEnumerable
	{
		public abstract int Count
		{
			get;
		}

		public TElement this[int index]
		{
			get
			{
				if (index < this.Count)
				{
					return this.IndexedElement(index);
				}
				else
				{
					throw new IndexOutOfRangeException();
				}
			}
		}

		protected VersioningList()
		{
		}

		public abstract VersioningList<TElement> Add(TElement value);

		public static VersioningList<TElement> Create()
		{
			return new VersioningList<TElement>.EmptyVersioningList();
		}

		public abstract IEnumerator<TElement> GetEnumerator();

		protected abstract TElement IndexedElement(int index);

		public VersioningList<TElement> RemoveAt(int index)
		{
			if (index < this.Count)
			{
				return this.RemoveIndexedElement(index);
			}
			else
			{
				throw new IndexOutOfRangeException();
			}
		}

		protected abstract VersioningList<TElement> RemoveIndexedElement(int index);

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		internal sealed class ArrayListEnumerator : IEnumerator<TElement>, IDisposable, IEnumerator
		{
			private readonly VersioningList<TElement>.ArrayVersioningList array;

			private int index;

			public TElement Current
			{
				get
				{
					if (this.index > this.array.Count)
					{
						TElement tElement = default(TElement);
						return tElement;
					}
					else
					{
						return this.array.ElementAt(this.index - 1);
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

			public ArrayListEnumerator(VersioningList<TElement>.ArrayVersioningList array)
			{
				this.array = array;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				int count = this.array.Count;
				if (this.index <= count)
				{
					var arrayListEnumerator = this;
					arrayListEnumerator.index = arrayListEnumerator.index + 1;
				}
				return this.index <= count;
			}

			public void Reset()
			{
				this.index = 0;
			}
		}

		internal sealed class ArrayVersioningList : VersioningList<TElement>
		{
			private readonly TElement[] elements;

			public override int Count
			{
				get
				{
					return (int)this.elements.Length;
				}
			}

			public ArrayVersioningList(VersioningList<TElement> preceding, TElement last)
			{
				this.elements = new TElement[preceding.Count + 1];
				int num = 0;
				foreach (TElement tElement in preceding)
				{
					int num1 = num;
					num = num1 + 1;
					this.elements[num1] = tElement;
				}
				this.elements[num] = last;
			}

			private ArrayVersioningList(TElement[] elements)
			{
				this.elements = elements;
			}

			public override VersioningList<TElement> Add(TElement value)
			{
				return new VersioningList<TElement>.LinkedVersioningList(this, value);
			}

			public TElement ElementAt(int index)
			{
				return this.elements[index];
			}

			public override IEnumerator<TElement> GetEnumerator()
			{
				return new VersioningList<TElement>.ArrayListEnumerator(this);
			}

			protected override TElement IndexedElement(int index)
			{
				return this.elements[index];
			}

			protected override VersioningList<TElement> RemoveIndexedElement(int index)
			{
				if ((int)this.elements.Length != 1)
				{
					int num = 0;
					TElement[] tElementArray = new TElement[(int)this.elements.Length - 1];
					for (int i = 0; i < (int)this.elements.Length; i++)
					{
						if (i != index)
						{
							int num1 = num;
							num = num1 + 1;
							tElementArray[num1] = this.elements[i];
						}
					}
					return new VersioningList<TElement>.ArrayVersioningList(tElementArray);
				}
				else
				{
					return new VersioningList<TElement>.EmptyVersioningList();
				}
			}
		}

		internal sealed class EmptyListEnumerator : IEnumerator<TElement>, IDisposable, IEnumerator
		{
			public TElement Current
			{
				get
				{
					TElement tElement = default(TElement);
					return tElement;
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public EmptyListEnumerator()
			{
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return false;
			}

			public void Reset()
			{
			}
		}

		internal sealed class EmptyVersioningList : VersioningList<TElement>
		{
			public override int Count
			{
				get
				{
					return 0;
				}
			}

			public EmptyVersioningList()
			{
			}

			public override VersioningList<TElement> Add(TElement value)
			{
				return new VersioningList<TElement>.LinkedVersioningList(this, value);
			}

			public override IEnumerator<TElement> GetEnumerator()
			{
				return new VersioningList<TElement>.EmptyListEnumerator();
			}

			protected override TElement IndexedElement(int index)
			{
				throw new IndexOutOfRangeException();
			}

			protected override VersioningList<TElement> RemoveIndexedElement(int index)
			{
				throw new IndexOutOfRangeException();
			}
		}

		internal sealed class LinkedListEnumerator : IEnumerator<TElement>, IDisposable, IEnumerator
		{
			private readonly VersioningList<TElement>.LinkedVersioningList list;

			private IEnumerator<TElement> preceding;

			private bool complete;

			public TElement Current
			{
				get
				{
					if (!this.complete)
					{
						return this.preceding.Current;
					}
					else
					{
						return this.list.Last;
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

			public LinkedListEnumerator(VersioningList<TElement>.LinkedVersioningList list)
			{
				this.list = list;
				this.preceding = list.Preceding.GetEnumerator();
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (!this.complete)
				{
					if (!this.preceding.MoveNext())
					{
						this.complete = true;
					}
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				this.preceding.Reset();
				this.complete = false;
			}
		}

		internal sealed class LinkedVersioningList : VersioningList<TElement>
		{
			private readonly VersioningList<TElement> preceding;

			private readonly TElement last;

			public override int Count
			{
				get
				{
					return this.preceding.Count + 1;
				}
			}

			private int Depth
			{
				get
				{
					int num = 0;
					for (VersioningList<TElement>.LinkedVersioningList i = this; i != null; i = i.Preceding as VersioningList<TElement>.LinkedVersioningList)
					{
						num++;
					}
					return num;
				}
			}

			public TElement Last
			{
				get
				{
					return this.last;
				}
			}

			public VersioningList<TElement> Preceding
			{
				get
				{
					return this.preceding;
				}
			}

			public LinkedVersioningList(VersioningList<TElement> preceding, TElement last)
			{
				this.preceding = preceding;
				this.last = last;
			}

			public override VersioningList<TElement> Add(TElement value)
			{
				if (this.Depth >= 5)
				{
					return new VersioningList<TElement>.ArrayVersioningList(this, value);
				}
				else
				{
					return new VersioningList<TElement>.LinkedVersioningList(this, value);
				}
			}

			public override IEnumerator<TElement> GetEnumerator()
			{
				return new VersioningList<TElement>.LinkedListEnumerator(this);
			}

			protected override TElement IndexedElement(int index)
			{
				if (index != this.Count - 1)
				{
					return this.preceding.IndexedElement(index);
				}
				else
				{
					return this.last;
				}
			}

			protected override VersioningList<TElement> RemoveIndexedElement(int index)
			{
				if (index != this.Count - 1)
				{
					return new VersioningList<TElement>.LinkedVersioningList(this.preceding.RemoveIndexedElement(index), this.last);
				}
				else
				{
					return this.preceding;
				}
			}
		}
	}
}