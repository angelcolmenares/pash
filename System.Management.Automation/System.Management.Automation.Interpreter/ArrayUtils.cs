namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal static class ArrayUtils
    {
        internal static T[] AddLast<T>(this IList<T> list, T item)
        {
            T[] array = new T[list.Count + 1];
            list.CopyTo(array, 0);
            array[list.Count] = item;
            return array;
        }
    }
}

