namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal class LookupPathCollection : Collection<string>
    {
        internal LookupPathCollection()
        {
        }

        internal LookupPathCollection(IEnumerable<string> collection)
        {
            foreach (string str in collection)
            {
                this.Add(str);
            }
        }

        public int Add(string item)
        {
            int index = -1;
            if (!this.Contains(item))
            {
                base.Add(item);
                index = base.IndexOf(item);
            }
            return index;
        }

        internal void AddRange(ICollection<string> collection)
        {
            foreach (string str in collection)
            {
                this.Add(str);
            }
        }

        public bool Contains(string item)
        {
            foreach (string str in this)
            {
                if (string.Equals(item, str, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                throw PSTraceSource.NewArgumentException("item");
            }
            for (int i = 0; i < base.Count; i++)
            {
                if (string.Equals(base[i], item, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        internal Collection<int> IndexOfRelativePath()
        {
            Collection<int> collection = new Collection<int>();
            for (int i = 0; i < base.Count; i++)
            {
                string str = base[i];
                if (!string.IsNullOrEmpty(str) && str.StartsWith(".", StringComparison.CurrentCulture))
                {
                    collection.Add(i);
                }
            }
            return collection;
        }
    }
}

