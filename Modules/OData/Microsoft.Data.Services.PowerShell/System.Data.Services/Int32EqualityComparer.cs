namespace System.Data.Services
{
    using System;
    using System.Collections.Generic;

    internal class Int32EqualityComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y)
        {
            return (x == y);
        }

        public int GetHashCode(int obj)
        {
            return obj;
        }
    }
}

