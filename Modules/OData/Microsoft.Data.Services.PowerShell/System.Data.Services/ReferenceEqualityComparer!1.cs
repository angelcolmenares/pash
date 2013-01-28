namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class ReferenceEqualityComparer<T> : ReferenceEqualityComparer, IEqualityComparer<T>
    {
        private static ReferenceEqualityComparer<T> instance;

        private ReferenceEqualityComparer()
        {
        }

        public bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }

        internal static ReferenceEqualityComparer<T> Instance
        {
            get
            {
                if (ReferenceEqualityComparer<T>.instance == null)
                {
                    ReferenceEqualityComparer<T> comparer = new ReferenceEqualityComparer<T>();
                    Interlocked.CompareExchange<ReferenceEqualityComparer<T>>(ref ReferenceEqualityComparer<T>.instance, comparer, null);
                }
                return ReferenceEqualityComparer<T>.instance;
            }
        }
    }
}

