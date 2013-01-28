using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime;

namespace System.Runtime.Collections
{
	internal class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		private OrderedDictionary privateDictionary;

		public int Count
		{
			get
			{
				return this.privateDictionary.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				if (key != null)
				{
					if (!this.privateDictionary.Contains(key))
					{
						throw Fx.Exception.AsError(new KeyNotFoundException(InternalSR.KeyNotFoundInDictionary));
					}
					else
					{
						return (TValue)this.privateDictionary[(object)key];
					}
				}
				else
				{
					throw Fx.Exception.ArgumentNull("key");
				}
			}
			set
			{
				if (key != null)
				{
					this.privateDictionary[(object)key] = value;
					return;
				}
				else
				{
					throw Fx.Exception.ArgumentNull("key");
				}
			}
		}

		public ICollection<TKey> Keys
		{
			get
			{
				List<TKey> tKeys = new List<TKey>(this.privateDictionary.Count);
				foreach (TKey key in this.privateDictionary.Keys)
				{
					tKeys.Add(key);
				}
				return tKeys;
			}
		}

		int System.Collections.ICollection.Count
		{
			get
			{
				return this.privateDictionary.Count;
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get
			{
				return this.privateDictionary.IsSynchronized;
			}
		}

		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				return this.privateDictionary.SyncRoot;
			}
		}

		bool System.Collections.IDictionary.IsFixedSize
		{
			get
			{
				return this.privateDictionary.IsFixedSize;
			}
		}

		bool System.Collections.IDictionary.IsReadOnly
		{
			get
			{
				return this.privateDictionary.IsReadOnly;
			}
		}

		object System.Collections.IDictionary.this[object key]
		{
			get
			{
				return this.privateDictionary[key];
			}
			set
			{
				this.privateDictionary[key] = value;
			}
		}

		ICollection System.Collections.IDictionary.Keys
		{
			get
			{
				return this.privateDictionary.Keys;
			}
		}

		ICollection System.Collections.IDictionary.Values
		{
			get
			{
				return this.privateDictionary.Values;
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				List<TValue> tValues = new List<TValue>(this.privateDictionary.Count);
				foreach (TValue value in this.privateDictionary.Values)
				{
					tValues.Add(value);
				}
				return tValues;
			}
		}

		public OrderedDictionary()
		{
			this.privateDictionary = new OrderedDictionary();
		}

		public OrderedDictionary(IDictionary<TKey, TValue> dictionary)
		{
			if (dictionary != null)
			{
				this.privateDictionary = new OrderedDictionary();
				foreach (KeyValuePair<TKey, TValue> keyValuePair in dictionary)
				{
					this.privateDictionary.Add(keyValuePair.Key, keyValuePair.Value);
				}
			}
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}

		public void Add(TKey key, TValue value)
		{
			if (key != null)
			{
				this.privateDictionary.Add(key, value);
				return;
			}
			else
			{
				throw Fx.Exception.ArgumentNull("key");
			}
		}

		public void Clear()
		{
			this.privateDictionary.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			if (item.Key == null || !this.privateDictionary.Contains(item.Key))
			{
				return false;
			}
			else
			{
				return this.privateDictionary[(object)item.Key].Equals(item.Value);
			}
		}

		public bool ContainsKey(TKey key)
		{
			if (key != null)
			{
				return this.privateDictionary.Contains(key);
			}
			else
			{
				throw Fx.Exception.ArgumentNull("key");
			}
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (array != null)
			{
				if (arrayIndex >= 0)
				{
					if (array.Rank > 1 || arrayIndex >= (int)array.Length || (int)array.Length - arrayIndex < this.privateDictionary.Count)
					{
						throw Fx.Exception.Argument("array", InternalSR.BadCopyToArray);
					}
					else
					{
						int num = arrayIndex;
						foreach (DictionaryEntry dictionaryEntry in this.privateDictionary)
						{
							array[num] = new KeyValuePair<TKey, TValue>((TKey)dictionaryEntry.Key, (TValue)dictionaryEntry.Value);
							num++;
						}
						return;
					}
				}
				else
				{
					throw Fx.Exception.AsError(new ArgumentOutOfRangeException("arrayIndex"));
				}
			}
			else
			{
				throw Fx.Exception.ArgumentNull("array");
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (DictionaryEntry dictionaryEntry in this.privateDictionary)
			{
				DictionaryEntry dictionaryEntry1 = dictionaryEntry;
				DictionaryEntry dictionaryEntry2 = dictionaryEntry;
				yield return new KeyValuePair<TKey, TValue>((TKey)dictionaryEntry1.Key, (TValue)dictionaryEntry2.Value);
			}
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (!this.Contains(item))
			{
				return false;
			}
			else
			{
				this.privateDictionary.Remove(item.Key);
				return true;
			}
		}

		public bool Remove(TKey key)
		{
			if (key != null)
			{
				if (!this.privateDictionary.Contains(key))
				{
					return false;
				}
				else
				{
					this.privateDictionary.Remove(key);
					return true;
				}
			}
			else
			{
				throw Fx.Exception.ArgumentNull("key");
			}
		}

		void System.Collections.ICollection.CopyTo(Array array, int index)
		{
			this.privateDictionary.CopyTo(array, index);
		}

		void System.Collections.IDictionary.Add(object key, object value)
		{
			this.privateDictionary.Add(key, value);
		}

		void System.Collections.IDictionary.Clear()
		{
			this.privateDictionary.Clear();
		}

		bool System.Collections.IDictionary.Contains(object key)
		{
			return this.privateDictionary.Contains(key);
		}

		IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator()
		{
			return this.privateDictionary.GetEnumerator();
		}

		void System.Collections.IDictionary.Remove(object key)
		{
			this.privateDictionary.Remove(key);
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			TValue item;
			if (key != null)
			{
				bool flag = this.privateDictionary.Contains(key);
				TValue tValuePointer = value;
				if (flag)
				{
					item = (TValue)this.privateDictionary[(object)key];
				}
				else
				{
					TValue tValue = default(TValue);
					item = tValue;
				}
				tValuePointer = item;
				return flag;
			}
			else
			{
				throw Fx.Exception.ArgumentNull("key");
			}
		}
	}
}