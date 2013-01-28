using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Internal
{
	internal class VersioningTree<TKey, TValue>
	{
		public readonly TKey Key;

		public readonly TValue Value;

		public readonly int Height;

		public readonly VersioningTree<TKey, TValue> LeftChild;

		public readonly VersioningTree<TKey, TValue> RightChild;

		public VersioningTree(TKey key, TValue value, VersioningTree<TKey, TValue> leftChild, VersioningTree<TKey, TValue> rightChild)
		{
			this.Key = key;
			this.Value = value;
			this.Height = VersioningTree<TKey, TValue>.Max(VersioningTree<TKey, TValue>.GetHeight(leftChild), VersioningTree<TKey, TValue>.GetHeight(rightChild)) + 1;
			this.LeftChild = leftChild;
			this.RightChild = rightChild;
		}

		private static int GetHeight(VersioningTree<TKey, TValue> tree)
		{
			if (tree == null)
			{
				return 0;
			}
			else
			{
				return tree.Height;
			}
		}

		public TValue GetValue(TKey key, Func<TKey, TKey, int> compareFunction)
		{
			TValue tValue = default(TValue);
			if (!this.TryGetValue(key, compareFunction, out tValue))
			{
				throw new KeyNotFoundException(key.ToString());
			}
			else
			{
				return tValue;
			}
		}

		private VersioningTree<TKey, TValue> MakeLeftmost(VersioningTree<TKey, TValue> leftmost)
		{
			if (this.LeftChild != null)
			{
				return new VersioningTree<TKey, TValue>(this.Key, this.Value, this.LeftChild.MakeLeftmost(leftmost), this.RightChild);
			}
			else
			{
				return new VersioningTree<TKey, TValue>(this.Key, this.Value, leftmost, this.RightChild);
			}
		}

		private VersioningTree<TKey, TValue> MakeRightmost(VersioningTree<TKey, TValue> rightmost)
		{
			if (this.RightChild != null)
			{
				return new VersioningTree<TKey, TValue>(this.Key, this.Value, this.LeftChild, this.RightChild.MakeRightmost(rightmost));
			}
			else
			{
				return new VersioningTree<TKey, TValue>(this.Key, this.Value, this.LeftChild, rightmost);
			}
		}

		private static int Max(int x, int y)
		{
			if (x > y)
			{
				return x;
			}
			else
			{
				return y;
			}
		}

		public VersioningTree<TKey, TValue> Remove(TKey key, Func<TKey, TKey, int> compareFunction)
		{
			int num = compareFunction(key, this.Key);
			if (num >= 0)
			{
				if (num != 0)
				{
					if (this.RightChild != null)
					{
						return new VersioningTree<TKey, TValue>(this.Key, this.Value, this.LeftChild, this.RightChild.Remove(key, compareFunction));
					}
					else
					{
						throw new KeyNotFoundException(key.ToString());
					}
				}
				else
				{
					if (this.LeftChild != null)
					{
						if (this.RightChild != null)
						{
							if (this.LeftChild.Height >= this.RightChild.Height)
							{
								return this.RightChild.MakeLeftmost(this.LeftChild);
							}
							else
							{
								return this.LeftChild.MakeRightmost(this.RightChild);
							}
						}
						else
						{
							return this.LeftChild;
						}
					}
					else
					{
						return this.RightChild;
					}
				}
			}
			else
			{
				if (this.LeftChild != null)
				{
					return new VersioningTree<TKey, TValue>(this.Key, this.Value, this.LeftChild.Remove(key, compareFunction), this.RightChild);
				}
				else
				{
					throw new KeyNotFoundException(key.ToString());
				}
			}
		}

		public VersioningTree<TKey, TValue> SetKeyValue(TKey key, TValue value, Func<TKey, TKey, int> compareFunction)
		{
			VersioningTree<TKey, TValue> leftChild;
			VersioningTree<TKey, TValue> rightChild;
			TKey tKey;
			TValue tValue;
			VersioningTree<TKey, TValue> versioningTree;
			VersioningTree<TKey, TValue> rightChild1;
			TKey tKey1;
			TValue tValue1;
			VersioningTree<TKey, TValue> leftChild1 = this.LeftChild;
			VersioningTree<TKey, TValue> versioningTree1 = this.RightChild;
			int num = compareFunction(key, this.Key);
			if (num >= 0)
			{
				if (num != 0)
				{
					if (VersioningTree<TKey, TValue>.GetHeight(leftChild1) >= VersioningTree<TKey, TValue>.GetHeight(versioningTree1))
					{
						return new VersioningTree<TKey, TValue>(this.Key, this.Value, leftChild1, VersioningTree<TKey, TValue>.SetKeyValue(versioningTree1, key, value, compareFunction));
					}
					else
					{
						int num1 = compareFunction(key, versioningTree1.Key);
						TKey tKey2 = this.Key;
						TValue tValue2 = this.Value;
						VersioningTree<TKey, TValue> versioningTree2 = leftChild1;
						if (num1 < 0)
						{
							leftChild = VersioningTree<TKey, TValue>.SetKeyValue(versioningTree1.LeftChild, key, value, compareFunction);
						}
						else
						{
							leftChild = versioningTree1.LeftChild;
						}
						VersioningTree<TKey, TValue> versioningTree3 = new VersioningTree<TKey, TValue>(tKey2, tValue2, versioningTree2, leftChild);
						if (num1 > 0)
						{
							rightChild = VersioningTree<TKey, TValue>.SetKeyValue(versioningTree1.RightChild, key, value, compareFunction);
						}
						else
						{
							rightChild = versioningTree1.RightChild;
						}
						VersioningTree<TKey, TValue> versioningTree4 = rightChild;
						if (num1 == 0)
						{
							tKey = key;
						}
						else
						{
							tKey = versioningTree1.Key;
						}
						if (num1 == 0)
						{
							tValue = value;
						}
						else
						{
							tValue = versioningTree1.Value;
						}
						return new VersioningTree<TKey, TValue>(tKey, tValue, versioningTree3, versioningTree4);
					}
				}
				else
				{
					return new VersioningTree<TKey, TValue>(key, value, leftChild1, versioningTree1);
				}
			}
			else
			{
				if (VersioningTree<TKey, TValue>.GetHeight(leftChild1) <= VersioningTree<TKey, TValue>.GetHeight(versioningTree1))
				{
					return new VersioningTree<TKey, TValue>(this.Key, this.Value, VersioningTree<TKey, TValue>.SetKeyValue(leftChild1, key, value, compareFunction), versioningTree1);
				}
				else
				{
					int num2 = compareFunction(key, leftChild1.Key);
					if (num2 < 0)
					{
						versioningTree = VersioningTree<TKey, TValue>.SetKeyValue(leftChild1.LeftChild, key, value, compareFunction);
					}
					else
					{
						versioningTree = leftChild1.LeftChild;
					}
					VersioningTree<TKey, TValue> versioningTree5 = versioningTree;
					TKey tKey3 = this.Key;
					TValue tValue3 = this.Value;
					if (num2 > 0)
					{
						rightChild1 = VersioningTree<TKey, TValue>.SetKeyValue(leftChild1.RightChild, key, value, compareFunction);
					}
					else
					{
						rightChild1 = leftChild1.RightChild;
					}
					VersioningTree<TKey, TValue> versioningTree6 = new VersioningTree<TKey, TValue>(tKey3, tValue3, rightChild1, versioningTree1);
					if (num2 == 0)
					{
						tKey1 = key;
					}
					else
					{
						tKey1 = leftChild1.Key;
					}
					if (num2 == 0)
					{
						tValue1 = value;
					}
					else
					{
						tValue1 = leftChild1.Value;
					}
					return new VersioningTree<TKey, TValue>(tKey1, tValue1, versioningTree5, versioningTree6);
				}
			}
		}

		private static VersioningTree<TKey, TValue> SetKeyValue(VersioningTree<TKey, TValue> me, TKey key, TValue value, Func<TKey, TKey, int> compareFunction)
		{
			if (me != null)
			{
				return me.SetKeyValue(key, value, compareFunction);
			}
			else
			{
				return new VersioningTree<TKey, TValue>(key, value, null, null);
			}
		}

		public bool TryGetValue(TKey key, Func<TKey, TKey, int> compareFunction, out TValue value)
		{
			VersioningTree<TKey, TValue> rightChild;
			VersioningTree<TKey, TValue> versioningTree = this;
			while (versioningTree != null)
			{
				int num = compareFunction(key, versioningTree.Key);
				if (num != 0)
				{
					if (num < 0)
					{
						VersioningTree<TKey, TValue> leftChild = versioningTree.LeftChild;
						rightChild = leftChild;
						versioningTree = leftChild;
					}
					else
					{
						rightChild = versioningTree.RightChild;
					}
					versioningTree = rightChild;
				}
				else
				{
					value = versioningTree.Value;
					return true;
				}
			}
			value = default(TValue);
			return false;
		}
	}
}