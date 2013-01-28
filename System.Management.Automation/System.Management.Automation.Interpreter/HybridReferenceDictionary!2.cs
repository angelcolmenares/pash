namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class HybridReferenceDictionary<TKey, TValue> where TKey: class
    {
        private const int _arraySize = 10;
        private int _count;
        private Dictionary<TKey, TValue> _dict;
        private KeyValuePair<TKey, TValue>[] _keysAndValues;

        public HybridReferenceDictionary()
        {
        }

        public HybridReferenceDictionary(int initialCapicity)
        {
            if (initialCapicity > 10)
            {
                this._dict = new Dictionary<TKey, TValue>(initialCapicity);
            }
            else
            {
                this._keysAndValues = new KeyValuePair<TKey, TValue>[initialCapicity];
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (this._dict != null)
            {
                return this._dict.ContainsKey(key);
            }
            if (this._keysAndValues != null)
            {
                for (int i = 0; i < this._keysAndValues.Length; i++)
                {
                    if (this._keysAndValues[i].Key == key)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (this._dict != null)
            {
                return this._dict.GetEnumerator();
            }
            return this.GetEnumeratorWorker();
        }

        private IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorWorker()
        {
            if (this._keysAndValues != null)
            {
                for (int i = 0; i < this._keysAndValues.Length; i++)
                {
                    if (this._keysAndValues[i].Key != null)
                    {
                        yield return this._keysAndValues[i];
                    }
                }
            }
        }

        public bool Remove(TKey key)
        {
            if (this._dict != null)
            {
                return this._dict.Remove(key);
            }
            if (this._keysAndValues != null)
            {
                for (int i = 0; i < this._keysAndValues.Length; i++)
                {
                    if (this._keysAndValues[i].Key == key)
                    {
                        this._keysAndValues[i] = new KeyValuePair<TKey, TValue>();
                        this._count--;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this._dict != null)
            {
                return this._dict.TryGetValue(key, out value);
            }
            if (this._keysAndValues != null)
            {
                for (int i = 0; i < this._keysAndValues.Length; i++)
                {
                    if (this._keysAndValues[i].Key == key)
                    {
                        value = this._keysAndValues[i].Value;
                        return true;
                    }
                }
            }
            value = default(TValue);
            return false;
        }

        public int Count
        {
            get
            {
                if (this._dict != null)
                {
                    return this._dict.Count;
                }
                return this._count;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue local;
                if (!this.TryGetValue(key, out local))
                {
                    throw new KeyNotFoundException();
                }
                return local;
            }
            set
            {
                if (this._dict != null)
                {
                    this._dict[key] = value;
                }
                else
                {
                    int num;
                    if (this._keysAndValues != null)
                    {
                        num = -1;
                        for (int i = 0; i < this._keysAndValues.Length; i++)
                        {
                            if (this._keysAndValues[i].Key == key)
                            {
                                this._keysAndValues[i] = new KeyValuePair<TKey, TValue>(key, value);
                                return;
                            }
                            if (this._keysAndValues[i].Key == null)
                            {
                                num = i;
                            }
                        }
                    }
                    else
                    {
                        this._keysAndValues = new KeyValuePair<TKey, TValue>[10];
                        num = 0;
                    }
                    if (num != -1)
                    {
                        this._count++;
                        this._keysAndValues[num] = new KeyValuePair<TKey, TValue>(key, value);
                    }
                    else
                    {
                        this._dict = new Dictionary<TKey, TValue>();
                        for (int j = 0; j < this._keysAndValues.Length; j++)
                        {
                            this._dict[this._keysAndValues[j].Key] = this._keysAndValues[j].Value;
                        }
                        this._keysAndValues = null;
                        this._dict[key] = value;
                    }
                }
            }
        }

        
    }
}

