namespace System.Management.Automation
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Reflection;

    internal class CacheTable
    {
        private HybridDictionary indexes = new HybridDictionary(true);
        internal Collection<object> memberCollection = new Collection<object>();

        internal CacheTable()
        {
        }

        internal void Add(string name, object member)
        {
            this.indexes[name] = new int?(this.memberCollection.Count);
            this.memberCollection.Add(member);
        }

        internal object this[string name]
        {
            get
            {
                object obj2 = this.indexes[name];
                if (obj2 == null)
                {
                    return null;
                }
                int? nullable = (int?) obj2;
                return this.memberCollection[nullable.Value];
            }
        }
    }
}

