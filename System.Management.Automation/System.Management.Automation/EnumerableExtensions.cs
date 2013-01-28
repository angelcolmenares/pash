namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> Append<T>(this IEnumerable<T> collection, T element)
        {
            foreach (T iteratorVariable0 in collection)
            {
                yield return iteratorVariable0;
            }
            yield return element;
        }

        internal static IEnumerable<T> Prepend<T>(this IEnumerable<T> collection, T element)
        {
            yield return element;
            foreach (T iteratorVariable0 in collection)
            {
                yield return iteratorVariable0;
            }
        }

        internal static int SequenceGetHashCode<T>(this IEnumerable<T> xs) where T: class
        {
            if (xs == null)
            {
                return 0x4ea3fed;
            }
            int num = 0x29;
            foreach (T local in xs)
            {
                num *= 0x3b;
                if (local != null)
                {
                    num += local.GetHashCode();
                }
            }
            return num;
        }

        
    }
}

