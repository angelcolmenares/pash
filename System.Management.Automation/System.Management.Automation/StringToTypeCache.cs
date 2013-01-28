namespace System.Management.Automation
{
    using System;
    using System.Collections.Generic;

    internal class StringToTypeCache
    {
        internal Dictionary<string, Type> cache = new Dictionary<string, Type>(0x100, StringComparer.OrdinalIgnoreCase);

        internal StringToTypeCache()
        {
            this.Reset();
            this.AddAssemblyLoadEventHandler();
        }

        internal void Add(string typeName, Type type)
        {
            lock (this.cache)
            {
                if (!this.cache.ContainsKey(typeName))
                {
                    this.cache.Add(typeName, type);
                }
                else if (this.cache[typeName] != type)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        internal void AddAssemblyLoadEventHandler()
        {
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(this.currentDomain_AssemblyLoad);
        }

        internal void AddOrReplace(string typeName, Type type)
        {
            lock (this.cache)
            {
                this.cache[typeName] = type;
            }
        }

        private void currentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            this.Reset();
        }

        internal Type Get(string typeName)
        {
            lock (this.cache)
            {
                Type type;
                if (this.cache.TryGetValue(typeName, out type))
                {
                    return type;
                }
                return null;
            }
        }

        internal bool Remove(string typeName)
        {
            lock (this.cache)
            {
                return this.cache.Remove(typeName);
            }
        }

        private void Reset()
        {
            lock (this.cache)
            {
                this.cache.Clear();
                TypeAccelerators.FillCache(this.cache);
            }
        }
    }
}

