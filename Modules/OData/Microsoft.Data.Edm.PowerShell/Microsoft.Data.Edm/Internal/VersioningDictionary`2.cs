using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Internal
{
	internal abstract class VersioningDictionary<TKey, TValue>
	{
		protected readonly Func<TKey, TKey, int> CompareFunction;

		protected VersioningDictionary(Func<TKey, TKey, int> compareFunction)
		{
			this.CompareFunction = compareFunction;
		}

		public static VersioningDictionary<TKey, TValue> Create(Func<TKey, TKey, int> compareFunction)
		{
			return new VersioningDictionary<TKey, TValue>.EmptyVersioningDictionary(compareFunction);
		}

		public TValue Get(TKey key)
		{
			TValue tValue = default(TValue);
			if (!this.TryGetValue(key, out tValue))
			{
				throw new KeyNotFoundException(key.ToString());
			}
			else
			{
				return tValue;
			}
		}

		public abstract VersioningDictionary<TKey, TValue> Remove(TKey keyToRemove);

		public abstract VersioningDictionary<TKey, TValue> Set(TKey keyToSet, TValue newValue);

		public abstract bool TryGetValue(TKey key, out TValue value);

		internal sealed class EmptyVersioningDictionary : VersioningDictionary<TKey, TValue>
		{
			public EmptyVersioningDictionary(Func<TKey, TKey, int> compareFunction) : base(compareFunction)
			{
			}

			public override VersioningDictionary<TKey, TValue> Remove(TKey keyToRemove)
			{
				throw new KeyNotFoundException(keyToRemove.ToString());
			}

			public override VersioningDictionary<TKey, TValue> Set(TKey keyToSet, TValue newValue)
			{
				return new VersioningDictionary<TKey, TValue>.OneKeyDictionary(this.CompareFunction, keyToSet, newValue);
			}

			public override bool TryGetValue(TKey key, out TValue value)
			{
				value = default(TValue);
				return false;
			}
		}

		internal sealed class HashTreeDictionary : VersioningDictionary<TKey, TValue>
		{
			private const int HashSize = 17;

			private readonly VersioningTree<TKey, TValue>[] treeBuckets;

			public HashTreeDictionary(Func<TKey, TKey, int> compareFunction, VersioningTree<TKey, TValue> tree, TKey key, TValue value) : base(compareFunction)
			{
				this.treeBuckets = new VersioningTree<TKey, TValue>[17];
				this.SetKeyValues(tree);
				this.SetKeyValue(key, value);
			}

			public HashTreeDictionary(Func<TKey, TKey, int> compareFunction, VersioningTree<TKey, TValue>[] trees, TKey key, TValue value) : base(compareFunction)
			{
				this.treeBuckets = (VersioningTree<TKey, TValue>[])trees.Clone();
				this.SetKeyValue(key, value);
			}

			public HashTreeDictionary(Func<TKey, TKey, int> compareFunction, VersioningTree<TKey, TValue>[] trees, TKey key) : base(compareFunction)
			{
				this.treeBuckets = (VersioningTree<TKey, TValue>[])trees.Clone();
				this.RemoveKey(key);
			}

			private int GetBucket(TKey key)
			{
				int hashCode = key.GetHashCode();
				if (hashCode < 0)
				{
					hashCode = -hashCode;
				}
				return hashCode % 17;
			}

			public override VersioningDictionary<TKey, TValue> Remove(TKey keyToRemove)
			{
				return new VersioningDictionary<TKey, TValue>.HashTreeDictionary(this.CompareFunction, this.treeBuckets, keyToRemove);
			}

			private void RemoveKey(TKey keyToRemove)
			{
				int bucket = this.GetBucket(keyToRemove);
				if (this.treeBuckets[bucket] != null)
				{
					this.treeBuckets[bucket] = this.treeBuckets[bucket].Remove(keyToRemove, this.CompareFunction);
					return;
				}
				else
				{
					throw new KeyNotFoundException(keyToRemove.ToString());
				}
			}

			public override VersioningDictionary<TKey, TValue> Set(TKey keyToSet, TValue newValue)
			{
				return new VersioningDictionary<TKey, TValue>.HashTreeDictionary(this.CompareFunction, this.treeBuckets, keyToSet, newValue);
			}

			private void SetKeyValue(TKey keyToSet, TValue newValue)
			{
				int bucket = this.GetBucket(keyToSet);
				if (this.treeBuckets[bucket] != null)
				{
					this.treeBuckets[bucket] = this.treeBuckets[bucket].SetKeyValue(keyToSet, newValue, this.CompareFunction);
					return;
				}
				else
				{
					this.treeBuckets[bucket] = new VersioningTree<TKey, TValue>(keyToSet, newValue, null, null);
					return;
				}
			}

			private void SetKeyValues(VersioningTree<TKey, TValue> tree)
			{
				if (tree != null)
				{
					this.SetKeyValue(tree.Key, tree.Value);
					this.SetKeyValues(tree.LeftChild);
					this.SetKeyValues(tree.RightChild);
					return;
				}
				else
				{
					return;
				}
			}

			public override bool TryGetValue(TKey key, out TValue value)
			{
				VersioningTree<TKey, TValue> versioningTree = this.treeBuckets[this.GetBucket(key)];
				if (versioningTree != null)
				{
					return versioningTree.TryGetValue(key, this.CompareFunction, out value);
				}
				else
				{
					value = default(TValue);
					return false;
				}
			}
		}

		internal sealed class OneKeyDictionary : VersioningDictionary<TKey, TValue>
		{
			private readonly TKey key;

			private readonly TValue @value;

			public OneKeyDictionary(Func<TKey, TKey, int> compareFunction, TKey key, TValue value) : base(compareFunction)
			{
				this.key = key;
				this.@value = value;
			}

			public override VersioningDictionary<TKey, TValue> Remove(TKey keyToRemove)
			{
				if (this.CompareFunction(keyToRemove, this.key) != 0)
				{
					throw new KeyNotFoundException(keyToRemove.ToString());
				}
				else
				{
					return new VersioningDictionary<TKey, TValue>.EmptyVersioningDictionary(this.CompareFunction);
				}
			}

			public override VersioningDictionary<TKey, TValue> Set(TKey keyToSet, TValue newValue)
			{
				if (this.CompareFunction(keyToSet, this.key) != 0)
				{
					return new VersioningDictionary<TKey, TValue>.TwoKeyDictionary(this.CompareFunction, this.key, this.@value, keyToSet, newValue);
				}
				else
				{
					return new VersioningDictionary<TKey, TValue>.OneKeyDictionary(this.CompareFunction, keyToSet, newValue);
				}
			}

			public override bool TryGetValue(TKey key, out TValue value)
			{
				if (this.CompareFunction(key, this.key) != 0)
				{
					value = default(TValue);
					return false;
				}
				else
				{
					value = this.@value;
					return true;
				}
			}
		}

		internal sealed class TreeDictionary : VersioningDictionary<TKey, TValue>
		{
			private const int MaxTreeHeight = 10;

			private readonly VersioningTree<TKey, TValue> tree;

			public TreeDictionary(Func<TKey, TKey, int> compareFunction, TKey firstKey, TValue firstValue, TKey secondKey, TValue secondValue, TKey thirdKey, TValue thirdValue) : base(compareFunction)
			{
				this.tree = (new VersioningTree<TKey, TValue>(firstKey, firstValue, null, null)).SetKeyValue(secondKey, secondValue, this.CompareFunction).SetKeyValue(thirdKey, thirdValue, this.CompareFunction);
			}

			public TreeDictionary(Func<TKey, TKey, int> compareFunction, VersioningTree<TKey, TValue> tree) : base(compareFunction)
			{
				this.tree = tree;
			}

			public override VersioningDictionary<TKey, TValue> Remove(TKey keyToRemove)
			{
				return new VersioningDictionary<TKey, TValue>.TreeDictionary(this.CompareFunction, this.tree.Remove(keyToRemove, this.CompareFunction));
			}

			public override VersioningDictionary<TKey, TValue> Set(TKey keyToSet, TValue newValue)
			{
				if (this.tree.Height <= 10)
				{
					return new VersioningDictionary<TKey, TValue>.TreeDictionary(this.CompareFunction, this.tree.SetKeyValue(keyToSet, newValue, this.CompareFunction));
				}
				else
				{
					return new VersioningDictionary<TKey, TValue>.HashTreeDictionary(this.CompareFunction, this.tree, keyToSet, newValue);
				}
			}

			public override bool TryGetValue(TKey key, out TValue value)
			{
				if (this.tree != null)
				{
					return this.tree.TryGetValue(key, this.CompareFunction, out value);
				}
				else
				{
					value = default(TValue);
					return false;
				}
			}
		}

		internal sealed class TwoKeyDictionary : VersioningDictionary<TKey, TValue>
		{
			private readonly TKey firstKey;

			private readonly TValue firstValue;

			private readonly TKey secondKey;

			private readonly TValue secondValue;

			public TwoKeyDictionary(Func<TKey, TKey, int> compareFunction, TKey firstKey, TValue firstValue, TKey secondKey, TValue secondValue) : base(compareFunction)
			{
				this.firstKey = firstKey;
				this.firstValue = firstValue;
				this.secondKey = secondKey;
				this.secondValue = secondValue;
			}

			public override VersioningDictionary<TKey, TValue> Remove(TKey keyToRemove)
			{
				if (this.CompareFunction(keyToRemove, this.firstKey) != 0)
				{
					if (this.CompareFunction(keyToRemove, this.secondKey) != 0)
					{
						throw new KeyNotFoundException(keyToRemove.ToString());
					}
					else
					{
						return new VersioningDictionary<TKey, TValue>.OneKeyDictionary(this.CompareFunction, this.firstKey, this.firstValue);
					}
				}
				else
				{
					return new VersioningDictionary<TKey, TValue>.OneKeyDictionary(this.CompareFunction, this.secondKey, this.secondValue);
				}
			}

			public override VersioningDictionary<TKey, TValue> Set(TKey keyToSet, TValue newValue)
			{
				if (this.CompareFunction(keyToSet, this.firstKey) != 0)
				{
					if (this.CompareFunction(keyToSet, this.secondKey) != 0)
					{
						return new VersioningDictionary<TKey, TValue>.TreeDictionary(this.CompareFunction, this.firstKey, this.firstValue, this.secondKey, this.secondValue, keyToSet, newValue);
					}
					else
					{
						return new VersioningDictionary<TKey, TValue>.TwoKeyDictionary(this.CompareFunction, this.firstKey, this.firstValue, keyToSet, newValue);
					}
				}
				else
				{
					return new VersioningDictionary<TKey, TValue>.TwoKeyDictionary(this.CompareFunction, keyToSet, newValue, this.secondKey, this.secondValue);
				}
			}

			public override bool TryGetValue(TKey key, out TValue value)
			{
				if (this.CompareFunction(key, this.firstKey) != 0)
				{
					if (this.CompareFunction(key, this.secondKey) != 0)
					{
						value = default(TValue);
						return false;
					}
					else
					{
						value = this.secondValue;
						return true;
					}
				}
				else
				{
					value = this.firstValue;
					return true;
				}
			}
		}
	}
}