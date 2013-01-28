using System;
using System.Collections.Generic;

namespace System.Management.Automation.Interpreter
{
    internal class CacheDict<TKey, TValue>
    {
        private readonly Dictionary<TKey, CacheDict<TKey, TValue>.KeyInfo> _dict;

        private readonly LinkedList<TKey> _list;

        private readonly int _maxSize;

        public TValue this[TKey key]
        {
            get
            {
                TValue tValue = default(TValue);
                if (!this.TryGetValue(key, out tValue))
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    return tValue;
                }
            }
            set
            {
                this.Add(key, value);
            }
        }

        public CacheDict(int maxSize)
        {
            this._dict = new Dictionary<TKey, CacheDict<TKey, TValue>.KeyInfo>();
            this._list = new LinkedList<TKey>();
            this._maxSize = maxSize;
        }

        public void Add(TKey key, TValue value)
        {
            CacheDict<TKey, TValue>.KeyInfo keyInfo;
            if (!this._dict.TryGetValue(key, out keyInfo))
            {
                if (this._list.Count == this._maxSize)
                {
                    LinkedListNode<TKey> last = this._list.Last;
                    this._list.RemoveLast();
                    this._dict.Remove(last.Value);
                }
            }
            else
            {
                this._list.Remove(keyInfo.List);
            }
            LinkedListNode<TKey> linkedListNode = new LinkedListNode<TKey>(key);
            this._list.AddFirst(linkedListNode);
            this._dict[key] = new CacheDict<TKey, TValue>.KeyInfo(value, linkedListNode);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            CacheDict<TKey, TValue>.KeyInfo keyInfo;
            if (!this._dict.TryGetValue(key, out keyInfo))
            {
                value = default(TValue);
                return false;
            }
            else
            {
                LinkedListNode<TKey> list = keyInfo.List;
                if (list.Previous != null)
                {
                    this._list.Remove(list);
                    this._list.AddFirst(list);
                }
                value = keyInfo.Value;
                return true;
            }
        }

        private struct KeyInfo
        {
            internal readonly TValue Value;

            internal readonly LinkedListNode<TKey> List;

            internal KeyInfo(TValue value, LinkedListNode<TKey> list)
            {
                this.Value = value;
                this.List = list;
            }
        }
    }
}