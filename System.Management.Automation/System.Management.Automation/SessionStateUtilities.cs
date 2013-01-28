namespace System.Management.Automation
{
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    internal static class SessionStateUtilities
    {
        internal static bool CollectionContainsValue(IEnumerable collection, object value, IComparer comparer)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach (object obj2 in collection)
            {
                if (comparer != null)
                {
                    if (comparer.Compare(obj2, value) == 0)
                    {
                        return true;
                    }
                }
                else if (obj2.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        internal static Collection<T> ConvertArrayToCollection<T>(T[] array)
        {
            Collection<T> collection = new Collection<T>();
            if (array != null)
            {
                foreach (T local in array)
                {
                    collection.Add(local);
                }
            }
            return collection;
        }

        internal static Collection<T> ConvertListToCollection<T>(List<T> list)
        {
            Collection<T> collection = new Collection<T>();
            if (list != null)
            {
                foreach (T local in list)
                {
                    collection.Add(local);
                }
            }
            return collection;
        }

        internal static Collection<WildcardPattern> CreateWildcardsFromStrings(Collection<string> globPatterns, WildcardOptions options)
        {
            Collection<WildcardPattern> collection = new Collection<WildcardPattern>();
            if ((globPatterns != null) && (globPatterns.Count > 0))
            {
                foreach (string str in globPatterns)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        collection.Add(new WildcardPattern(str, options));
                    }
                }
            }
            return collection;
        }

        internal static Collection<WildcardPattern> CreateWildcardsFromStrings(string[] globPatterns, WildcardOptions options)
        {
            return CreateWildcardsFromStrings(ConvertArrayToCollection<string>(globPatterns), options);
        }

        internal static FileMode GetFileModeFromOpenMode(OpenMode openMode)
        {
            FileMode create = FileMode.Create;
            switch (openMode)
            {
                case OpenMode.Add:
                    return FileMode.Append;

                case OpenMode.New:
                    return FileMode.CreateNew;

                case OpenMode.Overwrite:
                    return FileMode.Create;
            }
            return create;
        }

        internal static bool MatchesAnyWildcardPattern(string text, IEnumerable<WildcardPattern> patterns, bool defaultValue)
        {
            bool flag = false;
            bool flag2 = false;
            if (patterns != null)
            {
                foreach (WildcardPattern pattern in patterns)
                {
                    flag2 = true;
                    if (pattern.IsMatch(text))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag2)
            {
                flag = defaultValue;
            }
            return flag;
        }
    }
}

