namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation.Language;
    using System.Reflection;
    using System.Text;

    public sealed class DefaultParameterDictionary : Hashtable
    {
        private bool _isChanged;

        public DefaultParameterDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
            this._isChanged = true;
        }

        public DefaultParameterDictionary(IDictionary dictionary) : base(StringComparer.OrdinalIgnoreCase)
        {
            if (dictionary == null)
            {
                throw new PSArgumentNullException("dictionary");
            }
            List<object> list = new List<object>();
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is string)
                {
                    string key = ((string) entry.Key).Trim();
                    string cmdletName = null;
                    string parameterName = null;
                    if (!CheckKeyIsValid(key, ref cmdletName, ref parameterName))
                    {
                        if (key.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                        {
                            base.Add(entry.Key, entry.Value);
                        }
                        else
                        {
                            list.Add(entry.Key);
                        }
                        continue;
                    }
                }
                if (list.Count == 0)
                {
                    base.Add(entry.Key, entry.Value);
                }
            }
            StringBuilder builder = new StringBuilder();
            foreach (object obj2 in list)
            {
                builder.Append(obj2.ToString() + ", ");
            }
            if (builder.Length > 0)
            {
                builder.Remove(builder.Length - 2, 2);
                string resourceId = (list.Count > 1) ? "MultipleKeysInBadFormat" : "SingleKeyInBadFormat";
                throw PSTraceSource.NewInvalidOperationException("ParameterBinderStrings", resourceId, new object[] { builder });
            }
            this._isChanged = true;
        }

        public override void Add(object key, object value)
        {
            if (key == null)
            {
                throw new PSArgumentNullException("key");
            }
            if (key is string)
            {
                string str = ((string) key).Trim();
                string cmdletName = null;
                string parameterName = null;
                if (!CheckKeyIsValid(str, ref cmdletName, ref parameterName) && !str.Equals("Disabled", StringComparison.OrdinalIgnoreCase))
                {
                    throw PSTraceSource.NewInvalidOperationException("ParameterBinderStrings", "SingleKeyInBadFormat", new object[] { key.ToString() });
                }
            }
            base.Add(key, value);
            this._isChanged = true;
        }

        public bool ChangeSinceLastCheck()
        {
            bool flag = this._isChanged;
            this._isChanged = false;
            return flag;
        }

        internal static bool CheckKeyIsValid(string key, ref string cmdletName, ref string parameterName)
        {
            if (key == string.Empty)
            {
                return false;
            }
            int index = GetValueToken(0, key, ref cmdletName, true);
            if (index == -1)
            {
                return false;
            }
            index = SkipWhiteSpace(index, key);
            if ((index == -1) || (key[index] != ':'))
            {
                return false;
            }
            index = SkipWhiteSpace(index + 1, key);
            if (index == -1)
            {
                return false;
            }
            index = GetValueToken(index, key, ref parameterName, false);
            return ((index != -1) && (index == key.Length));
        }

        public override void Clear()
        {
            base.Clear();
            this._isChanged = true;
        }

        private static int GetValueToken(int index, string key, ref string name, bool getCmdletName)
        {
            char c = '\0';
            if (key[index].IsSingleQuote() || key[index].IsDoubleQuote())
            {
                c = key[index];
                index++;
            }
            StringBuilder builder = new StringBuilder(string.Empty);
            while (index < key.Length)
            {
                if (c != '\0')
                {
                    if ((c.IsSingleQuote() && key[index].IsSingleQuote()) || (c.IsDoubleQuote() && key[index].IsDoubleQuote()))
                    {
                        name = builder.ToString().Trim();
                        if (name.Length != 0)
                        {
                            return (index + 1);
                        }
                        return -1;
                    }
                    builder.Append(key[index]);
                }
                else if (getCmdletName)
                {
                    if (key[index] == ':')
                    {
                        name = builder.ToString().Trim();
                        if (name.Length != 0)
                        {
                            return index;
                        }
                        return -1;
                    }
                    builder.Append(key[index]);
                }
                else
                {
                    builder.Append(key[index]);
                }
                index++;
            }
            if (!getCmdletName && (c == '\0'))
            {
                name = builder.ToString().Trim();
                return index;
            }
            return -1;
        }

        public override void Remove(object key)
        {
            base.Remove(key);
            this._isChanged = true;
        }

        private static int SkipWhiteSpace(int index, string key)
        {
            while (index < key.Length)
            {
                if ((!key[index].IsWhitespace() && (key[index] != '\r')) && (key[index] != '\n'))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public override object this[object key]
        {
            get
            {
                return base[key];
            }
            set
            {
                if (this.ContainsKey(key))
                {
                    base[key] = value;
                    this._isChanged = true;
                }
                else
                {
                    this.Add(key, value);
                }
            }
        }
    }
}

