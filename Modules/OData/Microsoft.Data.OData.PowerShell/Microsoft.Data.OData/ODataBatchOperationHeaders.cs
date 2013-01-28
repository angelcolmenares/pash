namespace Microsoft.Data.OData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal sealed class ODataBatchOperationHeaders : IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        private readonly Dictionary<string, string> caseSensitiveDictionary = new Dictionary<string, string>(StringComparer.Ordinal);

        public void Add(string key, string value)
        {
            this.caseSensitiveDictionary.Add(key, value);
        }

        public bool ContainsKeyOrdinal(string key)
        {
            return this.caseSensitiveDictionary.ContainsKey(key);
        }

        private string FindKeyIgnoreCase(string key)
        {
            string str = null;
            foreach (string str2 in this.caseSensitiveDictionary.Keys)
            {
                if (string.Compare(str2, key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (str != null)
                    {
                        throw new ODataException(Strings.ODataBatchOperationHeaderDictionary_DuplicateCaseInsensitiveKeys(key));
                    }
                    str = str2;
                }
            }
            return str;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.caseSensitiveDictionary.GetEnumerator();
        }

        public bool Remove(string key)
        {
            if (this.caseSensitiveDictionary.Remove(key))
            {
                return true;
            }
            key = this.FindKeyIgnoreCase(key);
            if (key == null)
            {
                return false;
            }
            return this.caseSensitiveDictionary.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.caseSensitiveDictionary.GetEnumerator();
        }

        public bool TryGetValue(string key, out string value)
        {
            if (this.caseSensitiveDictionary.TryGetValue(key, out value))
            {
                return true;
            }
            key = this.FindKeyIgnoreCase(key);
            if (key == null)
            {
                value = null;
                return false;
            }
            return this.caseSensitiveDictionary.TryGetValue(key, out value);
        }

        public string this[string key]
        {
            get
            {
                string str;
                if (!this.TryGetValue(key, out str))
                {
                    throw new KeyNotFoundException(Strings.ODataBatchOperationHeaderDictionary_KeyNotFound(key));
                }
                return str;
            }
            set
            {
                this.caseSensitiveDictionary[key] = value;
            }
        }
    }
}

