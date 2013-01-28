namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal static class Utils
    {
        internal static Task FlushAsync(this Stream stream)
        {
            return Task.Factory.StartNew(new Action(stream.Flush));
        }

        internal static KeyValuePair<int, T>[] StableSort<T>(this T[] array, Comparison<T> comparison)
        {
            ExceptionUtils.CheckArgumentNotNull<T[]>(array, "array");
            ExceptionUtils.CheckArgumentNotNull<Comparison<T>>(comparison, "comparison");
            KeyValuePair<int, T>[] pairArray = new KeyValuePair<int, T>[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                pairArray[i] = new KeyValuePair<int, T>(i, array[i]);
            }
            Array.Sort<KeyValuePair<int, T>>(pairArray, new StableComparer<T>(comparison));
            return pairArray;
        }

        internal static bool TryDispose(object o)
        {
            IDisposable disposable = o as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
                return true;
            }
            return false;
        }

        private sealed class StableComparer<T> : IComparer<KeyValuePair<int, T>>
        {
            private readonly Comparison<T> innerComparer;

            public StableComparer(Comparison<T> innerComparer)
            {
                this.innerComparer = innerComparer;
            }

            public int Compare(KeyValuePair<int, T> x, KeyValuePair<int, T> y)
            {
                int num = this.innerComparer(x.Value, y.Value);
                if (num == 0)
                {
                    num = x.Key - y.Key;
                }
                return num;
            }
        }
    }
}

