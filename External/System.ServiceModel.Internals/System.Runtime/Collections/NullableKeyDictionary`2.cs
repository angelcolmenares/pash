using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;

namespace System.Runtime.Collections
{
	internal class NullableKeyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	{
		private bool isNullKeyPresent;

		private TValue nullKeyValue;

		private IDictionary<TKey, TValue> innerDictionary;

		public int Count
		{
			get
			{
				int num;
				int count = this.innerDictionary.Count;
				if (this.isNullKeyPresent)
				{
					num = 1;
				}
				else
				{
					num = 0;
				}
				return count + num;
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
					return this.innerDictionary[key];
				}
				else
				{
					if (!this.isNullKeyPresent)
					{
						throw Fx.Exception.AsError(new KeyNotFoundException());
					}
					else
					{
						return this.nullKeyValue;
					}
				}
			}
			set
			{
				if (key != null)
				{
					this.innerDictionary[key] = value;
					return;
				}
				else
				{
					this.isNullKeyPresent = true;
					this.nullKeyValue = value;
					return;
				}
			}
		}

		public ICollection<TKey> Keys
		{
			get
			{
				return new NullableKeyDictionary<TKey, TValue>.NullKeyDictionaryKeyCollection<TKey, TValue>(this);
			}
		}

		public ICollection<TValue> Values
		{
			get
			{
				return new NullableKeyDictionary<TKey, TValue>.NullKeyDictionaryValueCollection<TKey, TValue>(this);
			}
		}

		public NullableKeyDictionary()
		{
			this.innerDictionary = new Dictionary<TKey, TValue>();
		}

		public void Add(TKey key, TValue value)
		{
			if (key != null)
			{
				this.innerDictionary.Add(key, value);
				return;
			}
			else
			{
				if (!this.isNullKeyPresent)
				{
					this.isNullKeyPresent = true;
					this.nullKeyValue = value;
					return;
				}
				else
				{
					throw Fx.Exception.Argument("key", InternalSR.NullKeyAlreadyPresent);
				}
			}
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			this.isNullKeyPresent = false;
			this.nullKeyValue = default(TValue);
			this.innerDictionary.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			if (item.Key != null)
			{
				return this.innerDictionary.Contains(item);
			}
			else
			{
				if (!this.isNullKeyPresent)
				{
					return false;
				}
				else
				{
					if (item.Value == null)
					{
						return this.nullKeyValue == null;
					}
					else
					{
						TValue value = item.Value;
						return value.Equals(this.nullKeyValue);
					}
				}
			}
		}

		public bool ContainsKey(TKey key)
		{
			if (key == null)
			{
				return this.isNullKeyPresent;
			}
			else
			{
				return this.innerDictionary.ContainsKey(key);
			}
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			this.innerDictionary.CopyTo(array, arrayIndex);
			if (this.isNullKeyPresent)
			{
				TKey tKey = default(TKey);
				array[arrayIndex + this.innerDictionary.Count] = new KeyValuePair<TKey, TValue>(tKey, this.nullKeyValue);
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			IEnumerator<KeyValuePair<TKey, TValue>> enumerator = this.innerDictionary.GetEnumerator();
			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
			if (this.isNullKeyPresent)
			{
				TKey tKey = default(TKey);
				yield return new KeyValuePair<TKey, TValue>(tKey, this.nullKeyValue);
			}
		}

		public bool Remove(TKey key)
		{
			if (key != null)
			{
				return this.innerDictionary.Remove(key);
			}
			else
			{
				bool flag = this.isNullKeyPresent;
				this.isNullKeyPresent = false;
				this.nullKeyValue = default(TValue);
				return flag;
			}
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			if (item.Key != null)
			{
				return this.innerDictionary.Remove(item);
			}
			else
			{
				if (!this.Contains(item))
				{
					return false;
				}
				else
				{
					this.isNullKeyPresent = false;
					this.nullKeyValue = default(TValue);
					return true;
				}
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (key != null)
			{
				return this.innerDictionary.TryGetValue(key, out value);
			}
			else
			{
				if (!this.isNullKeyPresent)
				{
					value = default(TValue);
					return false;
				}
				else
				{
					value = this.nullKeyValue;
					return true;
				}
			}
		}

		private class NullKeyDictionaryKeyCollection<TypeKey, TypeValue> : ICollection<TypeKey>, IEnumerable<TypeKey>, IEnumerable
		{
			private NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary;

			public int Count
			{
				get
				{
					int count = this.nullKeyDictionary.innerDictionary.Keys.Count;
					if (this.nullKeyDictionary.isNullKeyPresent)
					{
						count++;
					}
					return count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			public NullKeyDictionaryKeyCollection(NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary)
			{
				this.nullKeyDictionary = nullKeyDictionary;
			}

			public void Add(TypeKey item)
			{
				throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
			}

			public void Clear()
			{
				throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
			}

			public bool Contains(TypeKey item)
			{
				if (item == null)
				{
					return this.nullKeyDictionary.isNullKeyPresent;
				}
				else
				{
					return this.nullKeyDictionary.innerDictionary.Keys.Contains(item);
				}
			}

			public void CopyTo(TypeKey[] array, int arrayIndex)
			{
				this.nullKeyDictionary.innerDictionary.Keys.CopyTo(array, arrayIndex);
				if (this.nullKeyDictionary.isNullKeyPresent)
				{
					TypeKey typeKey = default(TypeKey);
					array[arrayIndex + this.nullKeyDictionary.innerDictionary.Keys.Count] = typeKey;
				}
			}

			public IEnumerator<TypeKey> GetEnumerator()
			{
				foreach (TypeKey key in this.nullKeyDictionary.innerDictionary.Keys)
				{
					yield return key;
				}
				if (this.nullKeyDictionary.isNullKeyPresent)
				{
					TypeKey typeKey = default(TypeKey);
					yield return typeKey;
				}
			}

			public bool Remove(TypeKey item)
			{
				throw Fx.Exception.AsError(new NotSupportedException(InternalSR.KeyCollectionUpdatesNotAllowed));
			}

			IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}

		private class NullKeyDictionaryValueCollection<TypeKey, TypeValue> : ICollection<TypeValue>, IEnumerable<TypeValue>, IEnumerable
		{
			private NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary;

			public int Count
			{
				get
				{
					int count = this.nullKeyDictionary.innerDictionary.Values.Count;
					if (this.nullKeyDictionary.isNullKeyPresent)
					{
						count++;
					}
					return count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return true;
				}
			}

			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			public NullKeyDictionaryValueCollection(NullableKeyDictionary<TypeKey, TypeValue> nullKeyDictionary)
			{
				this.nullKeyDictionary = nullKeyDictionary;
			}

			public void Add(TypeValue item)
			{
				throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
			}

			public void Clear()
			{
				throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
			}

			public bool Contains(TypeValue item)
			{
				if (this.nullKeyDictionary.innerDictionary.Values.Contains(item))
				{
					return true;
				}
				else
				{
					if (!this.nullKeyDictionary.isNullKeyPresent)
					{
						return false;
					}
					else
					{
						return this.nullKeyDictionary.nullKeyValue.Equals(item);
					}
				}
			}

			public void CopyTo(TypeValue[] array, int arrayIndex)
			{
				this.nullKeyDictionary.innerDictionary.Values.CopyTo(array, arrayIndex);
				if (this.nullKeyDictionary.isNullKeyPresent)
				{
					array[arrayIndex + this.nullKeyDictionary.innerDictionary.Values.Count] = this.nullKeyDictionary.nullKeyValue;
				}
			}

			public IEnumerator<TypeValue> GetEnumerator()
			{
				foreach (TypeValue value in this.nullKeyDictionary.innerDictionary.Values)
				{
					yield return value;
				}
				if (this.nullKeyDictionary.isNullKeyPresent)
				{
					yield return this.nullKeyDictionary.nullKeyValue;
				}
			}

			public bool Remove(TypeValue item)
			{
				throw Fx.Exception.AsError(new NotSupportedException(InternalSR.ValueCollectionUpdatesNotAllowed));
			}

			IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}
		}
	}
}