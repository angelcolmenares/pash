namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class CollectionExtension
    {
        internal static bool ListEquals<T>(this ICollection<T> first, ICollection<T> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            IEnumerator<T> enumerator = first.GetEnumerator();
            IEnumerator<T> enumerator2 = second.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator2.MoveNext();
                if (!comparer.Equals(enumerator.Current, enumerator2.Current))
                {
                    return false;
                }
            }
            return true;
        }

        internal static int ListHashCode<T>(this IEnumerable<T> list)
        {
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            int num = 0x1997;
            foreach (T local in list)
            {
                num ^= (num << 5) ^ comparer.GetHashCode(local);
            }
            return num;
        }

        internal static U[] Map<T, U>(this ICollection<T> collection, Func<T, U> select)
        {
            U[] localArray = new U[collection.Count];
            int num = 0;
            foreach (T local in collection)
            {
                localArray[num++] = select(local);
            }
            return localArray;
        }

        internal static bool TrueForAll<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            foreach (T local in collection)
            {
                if (!predicate(local))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

