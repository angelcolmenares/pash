namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Xml;

    [Serializable]
    public sealed class PSPrimitiveDictionary : Hashtable
    {
        private static readonly Type[] handshakeFriendlyTypes = new Type[] { 
            typeof(bool), typeof(byte), typeof(char), typeof(DateTime), typeof(decimal), typeof(double), typeof(Guid), typeof(int), typeof(long), typeof(sbyte), typeof(float), typeof(string), typeof(TimeSpan), typeof(ushort), typeof(uint), typeof(ulong), 
            typeof(Uri), typeof(byte[]), typeof(Version), typeof(ProgressRecord), typeof(XmlDocument), typeof(PSPrimitiveDictionary)
         };

        public PSPrimitiveDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public PSPrimitiveDictionary(Hashtable other) : base(StringComparer.OrdinalIgnoreCase)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            foreach (DictionaryEntry entry in other)
            {
                Hashtable hashtable = PSObject.Base(entry.Value) as Hashtable;
                if (hashtable != null)
                {
                    this.Add(entry.Key, new PSPrimitiveDictionary(hashtable));
                }
                else
                {
                    this.Add(entry.Key, entry.Value);
                }
            }
        }

        private PSPrimitiveDictionary(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Add(object key, object value)
        {
            string str = this.VerifyKey(key);
            this.VerifyValue(value);
            base.Add(str, value);
        }

        public void Add(string key, bool value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, byte value)
        {
			this.Add((object)key, (object)value);
        }

        public void Add(string key, decimal value)
        {
			this.Add((object)key, (object)value);
        }

        public void Add(string key, double value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, Guid value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, int value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, bool[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, byte[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, char[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, DateTime[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, decimal[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, char value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, DateTime value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, long value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, sbyte value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, float value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, double[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, Guid[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, int[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, long[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, PSPrimitiveDictionary[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, sbyte[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, float[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, string[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, TimeSpan[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, ushort[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, PSPrimitiveDictionary value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, string value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, TimeSpan value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, ushort value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, uint value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, ulong value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, Uri value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, Version value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, uint[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, ulong[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, Uri[] value)
        {
            this.Add((object)key, (object)value);
        }

        public void Add(string key, Version[] value)
        {
            this.Add((object)key, (object)value);
        }

        public override object Clone()
        {
            return new PSPrimitiveDictionary(this);
        }

        internal static PSPrimitiveDictionary CloneAndAddPSVersionTable(PSPrimitiveDictionary originalHash)
        {
            if ((originalHash != null) && originalHash.ContainsKey("PSVersionTable"))
            {
                return (PSPrimitiveDictionary) originalHash.Clone();
            }
            PSPrimitiveDictionary dictionary = originalHash;
            if (originalHash != null)
            {
                dictionary = (PSPrimitiveDictionary) originalHash.Clone();
            }
            else
            {
                dictionary = new PSPrimitiveDictionary();
            }
            PSPrimitiveDictionary dictionary2 = new PSPrimitiveDictionary(PSVersionInfo.GetPSVersionTable());
            dictionary.Add("PSVersionTable", dictionary2);
            return dictionary;
        }

        internal static bool TryPathGet<T>(IDictionary data, out T result, params string[] keys)
        {
            IDictionary dictionary;
            if ((data == null) || !data.Contains(keys[0]))
            {
                result = default(T);
                return false;
            }
            if (keys.Length == 1)
            {
                return LanguagePrimitives.TryConvertTo<T>(data[keys[0]], out result);
            }
            if (LanguagePrimitives.TryConvertTo<IDictionary>(data[keys[0]], out dictionary) && (dictionary != null))
            {
                string[] destinationArray = new string[keys.Length - 1];
                Array.Copy(keys, 1, destinationArray, 0, destinationArray.Length);
                return TryPathGet<T>(dictionary, out result, destinationArray);
            }
            result = default(T);
            return false;
        }

        private string VerifyKey(object key)
        {
            key = PSObject.Base(key);
            string str = key as string;
            if (str == null)
            {
                throw new ArgumentException(StringUtil.Format(Serialization.PrimitiveHashtableInvalidKey, key.GetType().FullName));
            }
            return str;
        }

        private void VerifyValue(object value)
        {
            if (value != null)
            {
                value = PSObject.Base(value);
                Type type = value.GetType();
                foreach (Type type2 in handshakeFriendlyTypes)
                {
                    if (type == type2)
                    {
                        return;
                    }
                }
                if (!type.IsArray && !type.Equals(typeof(ArrayList)))
                {
                    throw new ArgumentException(StringUtil.Format(Serialization.PrimitiveHashtableInvalidValue, value.GetType().FullName));
                }
                foreach (object obj2 in (IEnumerable) value)
                {
                    this.VerifyValue(obj2);
                }
            }
        }

        public override object this[object key]
        {
            get
            {
                return base[key];
            }
            set
            {
                string str = this.VerifyKey(key);
                this.VerifyValue(value);
                base[str] = value;
            }
        }

        public object this[string key]
        {
            get
            {
                return base[key];
            }
            set
            {
                this.VerifyValue(value);
                base[key] = value;
            }
        }
    }
}

