namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Management.Automation.Language;

    internal static class HashtableOps
    {
        internal static object Add(IDictionary lvalDict, IDictionary rvalDict)
        {
            IDictionary dictionary;
            if (lvalDict is OrderedDictionary)
            {
                dictionary = new OrderedDictionary(StringComparer.CurrentCultureIgnoreCase);
            }
            else
            {
                dictionary = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
            }
            foreach (object obj2 in lvalDict.Keys)
            {
                dictionary.Add(obj2, lvalDict[obj2]);
            }
            foreach (object obj3 in rvalDict.Keys)
            {
                dictionary.Add(obj3, rvalDict[obj3]);
            }
            return dictionary;
        }

        internal static void AddKeyValuePair(IDictionary hashtable, object key, object value, IScriptExtent errorExtent)
        {
            key = PSObject.Base(key);
            if (key == null)
            {
                throw InterpreterError.NewInterpreterException(hashtable, typeof(RuntimeException), errorExtent, "InvalidNullKey", ParserStrings.InvalidNullKey, new object[0]);
            }
            if (hashtable.Contains(key))
            {
                string str = PSObject.ToStringParser(null, key);
                if (str.Length > 40)
                {
                    str = str.Substring(0, 40) + "...";
                }
                throw InterpreterError.NewInterpreterException(hashtable, typeof(RuntimeException), errorExtent, "DuplicateKeyInHashLiteral", ParserStrings.DuplicateKeyInHashLiteral, new object[] { str });
            }
            hashtable.Add(key, value);
        }
    }
}

